using System.Collections.Concurrent;
using PancakeOrdering.Application.Dispatching;
using PancakeOrdering.Application.Orders.Snapshots;
using PancakeOrdering.Application.Ports;
using PancakeOrdering.Contracts.Dtos;
using PancakeOrdering.Core.Common.Results;
using PancakeOrdering.Core.Domain.Enums;
using PancakeOrdering.Core.Domain.Orders;

namespace PancakeOrdering.Application
{
    internal sealed class OrderApplicationService
    {
        private readonly IKitchenGateway _kitchenGateway;
        private readonly IDeliveryGateway _deliveryGateway;
        private readonly IArchiveGateway _archiveGateway;
        private readonly IIngredientAvailability _ingredientAvailability;
        private readonly OrderSnapshotStore _snapshotStore;
        private readonly ConcurrentDictionary<Guid, StoredOrder> _orders = new();

        public OrderApplicationService(
            IKitchenGateway kitchenGateway,
            IDeliveryGateway deliveryGateway,
            IArchiveGateway archiveGateway,
            IIngredientAvailability ingredientAvailability)
            : this(
                  kitchenGateway,
                  deliveryGateway,
                  archiveGateway,
                  ingredientAvailability,
                  new OrderSnapshotStore())
        {
        }

        internal OrderApplicationService(
            IKitchenGateway kitchenGateway,
            IDeliveryGateway deliveryGateway,
            IArchiveGateway archiveGateway,
            IIngredientAvailability ingredientAvailability,
            OrderSnapshotStore snapshotStore)
        {
            _kitchenGateway = kitchenGateway;
            _deliveryGateway = deliveryGateway;
            _archiveGateway = archiveGateway;
            _ingredientAvailability = ingredientAvailability;
            _snapshotStore = snapshotStore;
        }

        public Result<Guid> CreateOrder(DeliveryAddress deliveryAddress)
        {
            var orderResult = Order.Create(deliveryAddress);
            if (!orderResult.IsSuccess)
                return Result.Failure<Guid>(orderResult.Error!.Value);

            var order = orderResult.Value!;

            _orders[order.OrderId] = new StoredOrder(order, new PerOrderCommandQueue());
            _snapshotStore.Publish(order.OrderId, order);

            return Result.Success(order.OrderId);
        }

        public Task<Result<int>> AddPancakeAsync(Guid orderId, Ingredient ingredient) =>
            EnqueueAsync(orderId, async order =>
            {
                var ingredientResult = ToIngredientType(ingredient);
                if (!ingredientResult.IsSuccess)
                    return Result.Failure<int>(ingredientResult.Error!.Value);

                var availabilityResult = await CheckAvailabilityAsync([ingredientResult.Value]);
                if (!availabilityResult.IsSuccess)
                    return Result.Failure<int>(availabilityResult.Error!.Value);

                return order.AddPancake(ingredient);
            });

        public Task<Result<int>> AddPancakeAsync(Guid orderId, HashSet<Ingredient> ingredients) =>
            EnqueueAsync(orderId, async order =>
            {
                var ingredientTypesResult = ToIngredientTypes(ingredients);
                if (!ingredientTypesResult.IsSuccess)
                    return Result.Failure<int>(ingredientTypesResult.Error!.Value);

                var availabilityResult = await CheckAvailabilityAsync(ingredientTypesResult.Value!);
                if (!availabilityResult.IsSuccess)
                    return Result.Failure<int>(availabilityResult.Error!.Value);

                return order.AddPancake(ingredients);
            });

        public Task<Result> RemovePancakeAsync(Guid orderId, int pancakeId) =>
            EnqueueAsync(orderId, order => Task.FromResult(order.RemovePancake(pancakeId)));

        public Task<Result> ChangeDeliveryAddressAsync(Guid orderId, DeliveryAddress deliveryAddress) =>
            EnqueueAsync(orderId, order => Task.FromResult(order.ChangeDeliveryAddress(deliveryAddress)));

        public Task<Result> AddIngredientAsync(Guid orderId, int pancakeId, Ingredient ingredient) =>
            EnqueueAsync(orderId, async order =>
            {
                var ingredientResult = ToIngredientType(ingredient);
                if (!ingredientResult.IsSuccess)
                    return Result.Failure(ingredientResult.Error!.Value);

                var availabilityResult = await CheckAvailabilityAsync([ingredientResult.Value]);
                if (!availabilityResult.IsSuccess)
                    return availabilityResult;

                return order.AddIngredient(pancakeId, ingredient);
            });

        public Task<Result> RemoveIngredientAsync(Guid orderId, int pancakeId, Ingredient ingredient) =>
            EnqueueAsync(orderId, order => Task.FromResult(order.RemoveIngredient(pancakeId, ingredient)));

        public Task<Result> ConfirmAsync(Guid orderId) =>
            EnqueueAsync(orderId, async order =>
            {
                var validationResult = order.ValidateConfirmation();
                if (!validationResult.IsSuccess)
                    return validationResult;

                var kitchenResult = await _kitchenGateway.AcceptOrderAsync(orderId);
                if (!kitchenResult.IsSuccess)
                    return kitchenResult;

                return order.Confirm();
            });

        public Task<Result> CancelAsync(Guid orderId) =>
            EnqueueAsync(orderId, order => Task.FromResult(order.Cancel()));

        public Task<Result> StartPreparationAsync(Guid orderId) =>
            EnqueueAsync(orderId, order => Task.FromResult(order.StartPreparation()));

        public Task<Result> CompletePreparationAsync(Guid orderId) =>
            EnqueueAsync(orderId, async order =>
            {
                var preparationResult = order.CompletePreparation();
                if (!preparationResult.IsSuccess)
                    return preparationResult;

                return await _deliveryGateway.SubmitOrderAsync(orderId);
            });

        public Task<Result> StartDeliveryAsync(Guid orderId) =>
            EnqueueAsync(orderId, order => Task.FromResult(order.StartDelivery()));

        public Task<Result> CompleteDeliveryAsync(Guid orderId) =>
            EnqueueAsync(orderId, order => Task.FromResult(order.CompleteDelivery()));

        public Task<Result> ArchiveAsync(Guid orderId) =>
            EnqueueAsync(orderId, async order =>
            {
                var validationResult = order.ValidateArchiving();
                if (!validationResult.IsSuccess)
                    return validationResult;

                var archiveResult = await _archiveGateway.ArchiveOrderAsync(orderId);
                if (!archiveResult.IsSuccess)
                    return archiveResult;

                return order.Archive();
            });

        public Result<OrderStatus> GetStatus(Guid orderId)
        {
            var snapshotResult = _snapshotStore.GetSnapshot(orderId);
            return snapshotResult.IsSuccess
                ? Result.Success(snapshotResult.Value!.Status)
                : Result.Failure<OrderStatus>(snapshotResult.Error!.Value);
        }

        internal OrderSnapshotStore SnapshotStore => _snapshotStore;

        private Task<Result> EnqueueAsync(Guid orderId, Func<Order, Task<Result>> operation)
        {
            if (!TryGetStoredOrder(orderId, out var storedOrder))
                return Task.FromResult(Result.Failure(ErrorCode.OrderNotFound));

            return storedOrder.Queue.EnqueueAsync(async () =>
            {
                try
                {
                    return await operation(storedOrder.Order);
                }
                finally
                {
                    _snapshotStore.Publish(orderId, storedOrder.Order);
                }
            });
        }

        private Task<Result<T>> EnqueueAsync<T>(Guid orderId, Func<Order, Task<Result<T>>> operation)
        {
            if (!TryGetStoredOrder(orderId, out var storedOrder))
                return Task.FromResult(Result.Failure<T>(ErrorCode.OrderNotFound));

            return storedOrder.Queue.EnqueueAsync(async () =>
            {
                try
                {
                    return await operation(storedOrder.Order);
                }
                finally
                {
                    _snapshotStore.Publish(orderId, storedOrder.Order);
                }
            });
        }

        private async Task<Result> CheckAvailabilityAsync(IReadOnlyCollection<IngredientTypeDto> ingredients)
        {
            return ingredients.Count == 0
                ? Result.Success()
                : await _ingredientAvailability.CheckAvailabilityAsync(ingredients);
        }

        private static Result<IReadOnlyCollection<IngredientTypeDto>> ToIngredientTypes(HashSet<Ingredient> ingredients)
        {
            var result = new List<IngredientTypeDto>();
            foreach (var ingredient in ingredients)
            {
                var ingredientResult = ToIngredientType(ingredient);
                if (!ingredientResult.IsSuccess)
                    return Result.Failure<IReadOnlyCollection<IngredientTypeDto>>(ingredientResult.Error!.Value);

                result.Add(ingredientResult.Value);
            }

            return Result.Success<IReadOnlyCollection<IngredientTypeDto>>(result);
        }

        private static Result<IngredientTypeDto> ToIngredientType(Ingredient ingredient)
        {
            return ingredient switch
            {
                Ingredient.Honey => Result.Success(IngredientTypeDto.Honey),
                Ingredient.Jam => Result.Success(IngredientTypeDto.Jam),
                Ingredient.Chocolate => Result.Success(IngredientTypeDto.Chocolate),
                _ => Result.Failure<IngredientTypeDto>(ErrorCode.InternalError)
            };
        }

        private bool TryGetStoredOrder(Guid orderId, out StoredOrder storedOrder) =>
            _orders.TryGetValue(orderId, out storedOrder!);

        private sealed class StoredOrder
        {
            public StoredOrder(Order order, PerOrderCommandQueue queue)
            {
                Order = order;
                Queue = queue;
            }

            public Order Order { get; }

            public PerOrderCommandQueue Queue { get; }
        }
    }
}
