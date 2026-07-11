using System.Collections.Concurrent;
using PancakeOrdering.Core.Application.Dispatching;
using PancakeOrdering.Core.Application.Ports;
using PancakeOrdering.Core.Common.Results;
using PancakeOrdering.Core.Domain.Enums;
using PancakeOrdering.Core.Domain.Orders;

namespace PancakeOrdering.Core.Application
{
    internal sealed class OrderApplicationService
    {
        private readonly IKitchenGateway _kitchenGateway;
        private readonly ConcurrentDictionary<Guid, StoredOrder> _orders = new();

        public OrderApplicationService(IKitchenGateway kitchenGateway)
        {
            _kitchenGateway = kitchenGateway;
        }

        public Result<Guid> CreateOrder(DeliveryAddress deliveryAddress)
        {
            var orderResult = Order.Create(deliveryAddress);
            if (!orderResult.IsSuccess)
                return Result.Failure<Guid>(orderResult.Error!.Value);

            var order = orderResult.Value!;

            _orders[order.OrderId] = new StoredOrder(order, new PerOrderCommandQueue());

            return Result.Success(order.OrderId);
        }

        public Task<Result<int>> AddPancakeAsync(Guid orderId, Ingredient ingredient) =>
            EnqueueAsync(orderId, order => Task.FromResult(order.AddPancake(ingredient)));



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
            EnqueueAsync(orderId, order => Task.FromResult(order.CompletePreparation()));

        public Task<Result> StartDeliveryAsync(Guid orderId) =>
            EnqueueAsync(orderId, order => Task.FromResult(order.StartDelivery()));

        public Task<Result> CompleteDeliveryAsync(Guid orderId) =>
            EnqueueAsync(orderId, order => Task.FromResult(order.CompleteDelivery()));

        public Task<Result> ArchiveAsync(Guid orderId) =>
            EnqueueAsync(orderId, order => Task.FromResult(order.Archive()));



        private Task<Result> EnqueueAsync(Guid orderId, Func<Order, Task<Result>> operation)
        {
            if (!TryGetStoredOrder(orderId, out var storedOrder))
                return Task.FromResult(Result.Failure(ErrorCode.OrderNotFound));

            return storedOrder.Queue.EnqueueAsync(() => operation(storedOrder.Order));
        }

        private Task<Result<T>> EnqueueAsync<T>(Guid orderId, Func<Order, Task<Result<T>>> operation)
        {
            if (!TryGetStoredOrder(orderId, out var storedOrder))
                return Task.FromResult(Result.Failure<T>(ErrorCode.OrderNotFound));

            return storedOrder.Queue.EnqueueAsync(() => operation(storedOrder.Order));
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
