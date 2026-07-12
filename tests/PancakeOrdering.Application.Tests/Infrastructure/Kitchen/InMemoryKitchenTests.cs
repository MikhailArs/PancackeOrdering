using CoreResults = PancakeOrdering.Core.Common.Results;
using PancakeOrdering.Application;
using PancakeOrdering.Application.Orders.Snapshots;
using PancakeOrdering.Application.Ports;
using PancakeOrdering.Contracts.Dtos;
using PancakeOrdering.Contracts.Requests;
using PancakeOrdering.Contracts.Results;
using PancakeOrdering.Contracts.Services;
using PancakeOrdering.Infrastructure.Kitchen;

namespace PancakeOrdering.Application.Tests.Infrastructure.Kitchen
{
    public sealed class InMemoryKitchenTests
    {
        [Test]
        [Property("Requirement", "FR-8")]
        [Property("Requirement", "FR-9")]
        [Property("Requirement", "FR-10")]
        public async Task DraftAvailability_DoesNotConsumeStock()
        {
            var composition = CreateComposition(new Dictionary<IngredientTypeDto, int>
            {
                [IngredientTypeDto.Honey] = 1
            });
            var orderId = CreateOrder(composition.Service);

            var addPancakeResult = await composition.Service.AddPancakeAsync(
                new AddPancakeRequest(orderId, new[] { IngredientTypeDto.Honey }));
            var addEmptyPancakeResult = await composition.Service.AddPancakeAsync(
                new AddPancakeRequest(orderId, Array.Empty<IngredientTypeDto>()));
            var secondPancakeId = addEmptyPancakeResult.Value!.Pancakes
                .Single(pancake => pancake.Ingredients.Count == 0)
                .PancakeId;
            var addIngredientResult = await composition.Service.AddIngredientAsync(
                new AddIngredientRequest(orderId, secondPancakeId, IngredientTypeDto.Honey));

            Assert.That(addPancakeResult.IsSuccess, Is.True);
            Assert.That(addEmptyPancakeResult.IsSuccess, Is.True);
            Assert.That(addIngredientResult.IsSuccess, Is.True);
            Assert.That(composition.Kitchen.GetAvailableQuantity(IngredientTypeDto.Honey), Is.EqualTo(1));
        }

        [Test]
        [Property("Requirement", "FR-8")]
        [Property("Requirement", "FR-9")]
        [Property("Requirement", "FR-10")]
        public async Task UnavailableIngredient_PreventsDraftModification()
        {
            var composition = CreateComposition(new Dictionary<IngredientTypeDto, int>
            {
                [IngredientTypeDto.Jam] = 0
            });
            var orderId = CreateOrder(composition.Service);

            var addPancakeResult = await composition.Service.AddPancakeAsync(
                new AddPancakeRequest(orderId, new[] { IngredientTypeDto.Jam }));
            var afterFailedPancakeAdd = composition.Service.GetOrder(new GetOrderRequest(orderId));
            var addEmptyPancakeResult = await composition.Service.AddPancakeAsync(
                new AddPancakeRequest(orderId, Array.Empty<IngredientTypeDto>()));
            var pancakeId = addEmptyPancakeResult.Value!.Pancakes.Single().PancakeId;
            var addIngredientResult = await composition.Service.AddIngredientAsync(
                new AddIngredientRequest(orderId, pancakeId, IngredientTypeDto.Jam));
            var afterFailedIngredientAdd = composition.Service.GetOrder(new GetOrderRequest(orderId));

            Assert.That(addPancakeResult.IsSuccess, Is.False);
            Assert.That(addPancakeResult.Error, Is.EqualTo(OperationErrorCode.IngredientUnavailable));
            Assert.That(afterFailedPancakeAdd.IsSuccess, Is.True);
            Assert.That(afterFailedPancakeAdd.Value!.Pancakes, Is.Empty);
            Assert.That(addEmptyPancakeResult.IsSuccess, Is.True);
            Assert.That(addIngredientResult.IsSuccess, Is.False);
            Assert.That(addIngredientResult.Error, Is.EqualTo(OperationErrorCode.IngredientUnavailable));
            Assert.That(afterFailedIngredientAdd.IsSuccess, Is.True);
            Assert.That(afterFailedIngredientAdd.Value!.Pancakes.Single().Ingredients, Is.Empty);
        }

        [Test]
        [Property("Requirement", "FR-4")]
        [Property("Requirement", "NFR-3")]
        public async Task AcceptOrder_PullsOrderDtoByOrderId()
        {
            var orderId = Guid.NewGuid();
            var queryService = new QueryServiceSpy(CreateOrderDto(
                orderId,
                OrderStatusDto.Draft,
                new[] { CreatePancakeDto(1, IngredientTypeDto.Honey) }));
            var kitchen = new InMemoryKitchen(
                queryService,
                new Dictionary<IngredientTypeDto, int>
                {
                    [IngredientTypeDto.Honey] = 1
                });

            var result = await kitchen.AcceptOrderAsync(orderId);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(queryService.CallCount, Is.EqualTo(1));
            Assert.That(queryService.OrderIds, Is.EqualTo(new[] { orderId }));
            Assert.That(kitchen.GetAvailableQuantity(IngredientTypeDto.Honey), Is.EqualTo(0));
        }

        [Test]
        [Property("Requirement", "FR-3")]
        [Property("Requirement", "FR-4")]
        [Property("Requirement", "FR-10")]
        public async Task Confirm_WhenKitchenAccepts_ConsumesStock()
        {
            var composition = CreateComposition(new Dictionary<IngredientTypeDto, int>
            {
                [IngredientTypeDto.Honey] = 2
            });
            var orderId = CreateOrder(composition.Service);
            await AddPancakeAsync(composition.Service, orderId, IngredientTypeDto.Honey);

            var result = await composition.Service.ConfirmOrderAsync(new ConfirmOrderRequest(orderId));

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value!.Status, Is.EqualTo(OrderStatusDto.Confirmed));
            Assert.That(composition.Kitchen.GetAvailableQuantity(IngredientTypeDto.Honey), Is.EqualTo(1));
        }

        [Test]
        [Property("Requirement", "FR-4")]
        [Property("Requirement", "FR-10")]
        public async Task InMemoryKitchen_MayProvideKitchenAndAvailabilityCapabilities()
        {
            var snapshotStore = new OrderSnapshotStore();
            var queryService = new OrderQueryService(snapshotStore);
            var kitchen = new InMemoryKitchen(
                queryService,
                new Dictionary<IngredientTypeDto, int>
                {
                    [IngredientTypeDto.Honey] = 1
                });
            IKitchenGateway kitchenGateway = kitchen;
            IIngredientAvailability ingredientAvailability = kitchen;
            var applicationService = new OrderApplicationService(
                kitchenGateway,
                new DeliveryGatewayFake(),
                new ArchiveGatewayFake(),
                ingredientAvailability,
                snapshotStore);
            var service = new PancakeOrderingService(applicationService, queryService);
            var orderId = CreateOrder(service);

            var addResult = await service.AddPancakeAsync(
                new AddPancakeRequest(orderId, new[] { IngredientTypeDto.Honey }));
            var confirmResult = await service.ConfirmOrderAsync(new ConfirmOrderRequest(orderId));

            Assert.That(addResult.IsSuccess, Is.True);
            Assert.That(confirmResult.IsSuccess, Is.True);
            Assert.That(confirmResult.Value!.Status, Is.EqualTo(OrderStatusDto.Confirmed));
            Assert.That(kitchen.GetAvailableQuantity(IngredientTypeDto.Honey), Is.EqualTo(0));
        }

        [Test]
        [Property("Requirement", "FR-3")]
        [Property("Requirement", "FR-4")]
        [Property("Requirement", "FR-10")]
        public async Task Confirm_WhenKitchenRunsOutOfStock_DeclinesWithoutNegativeInventory()
        {
            var composition = CreateComposition(new Dictionary<IngredientTypeDto, int>
            {
                [IngredientTypeDto.Honey] = 2
            });
            var firstOrderId = await CreateDraftOrderWithPancakeAsync(composition.Service, IngredientTypeDto.Honey);
            var secondOrderId = await CreateDraftOrderWithPancakeAsync(composition.Service, IngredientTypeDto.Honey);
            var thirdOrderId = await CreateDraftOrderWithPancakeAsync(composition.Service, IngredientTypeDto.Honey);

            var firstResult = await composition.Service.ConfirmOrderAsync(new ConfirmOrderRequest(firstOrderId));
            var secondResult = await composition.Service.ConfirmOrderAsync(new ConfirmOrderRequest(secondOrderId));
            var thirdResult = await composition.Service.ConfirmOrderAsync(new ConfirmOrderRequest(thirdOrderId));
            var thirdOrder = composition.Service.GetOrder(new GetOrderRequest(thirdOrderId));

            Assert.That(firstResult.IsSuccess, Is.True);
            Assert.That(secondResult.IsSuccess, Is.True);
            Assert.That(thirdResult.IsSuccess, Is.False);
            Assert.That(thirdResult.Error, Is.EqualTo(OperationErrorCode.KitchenDeclined));
            Assert.That(thirdOrder.IsSuccess, Is.True);
            Assert.That(thirdOrder.Value!.Status, Is.EqualTo(OrderStatusDto.Draft));
            Assert.That(composition.Kitchen.GetAvailableQuantity(IngredientTypeDto.Honey), Is.EqualTo(0));
        }

        [Test]
        [Property("Requirement", "FR-4")]
        [Property("Requirement", "FR-10")]
        public async Task AcceptOrder_WhenOneIngredientIsUnavailable_DeductsNothing()
        {
            var composition = CreateComposition(
                new Dictionary<IngredientTypeDto, int>
                {
                    [IngredientTypeDto.Honey] = 2,
                    [IngredientTypeDto.Jam] = 0
                },
                checkDraftAvailability: false);
            var orderId = CreateOrder(composition.Service);
            var addResult = await composition.Service.AddPancakeAsync(
                new AddPancakeRequest(orderId, new[] { IngredientTypeDto.Honey, IngredientTypeDto.Jam }));

            var confirmResult = await composition.Service.ConfirmOrderAsync(new ConfirmOrderRequest(orderId));
            var orderResult = composition.Service.GetOrder(new GetOrderRequest(orderId));

            Assert.That(addResult.IsSuccess, Is.True);
            Assert.That(confirmResult.IsSuccess, Is.False);
            Assert.That(confirmResult.Error, Is.EqualTo(OperationErrorCode.KitchenDeclined));
            Assert.That(orderResult.IsSuccess, Is.True);
            Assert.That(orderResult.Value!.Status, Is.EqualTo(OrderStatusDto.Draft));
            Assert.That(composition.Kitchen.GetAvailableQuantity(IngredientTypeDto.Honey), Is.EqualTo(2));
            Assert.That(composition.Kitchen.GetAvailableQuantity(IngredientTypeDto.Jam), Is.EqualTo(0));
        }

        [Test]
        [Property("Requirement", "FR-4")]
        [Property("Requirement", "FR-10")]
        [Property("Requirement", "NFR-5")]
        public async Task ConcurrentConfirmations_CannotOverConsumeStock()
        {
            var composition = CreateComposition(new Dictionary<IngredientTypeDto, int>
            {
                [IngredientTypeDto.Chocolate] = 1
            });
            var firstOrderId = await CreateDraftOrderWithPancakeAsync(composition.Service, IngredientTypeDto.Chocolate);
            var secondOrderId = await CreateDraftOrderWithPancakeAsync(composition.Service, IngredientTypeDto.Chocolate);

            var firstTask = composition.Service.ConfirmOrderAsync(new ConfirmOrderRequest(firstOrderId));
            var secondTask = composition.Service.ConfirmOrderAsync(new ConfirmOrderRequest(secondOrderId));
            var results = await Task.WhenAll(firstTask, secondTask);
            var firstOrder = composition.Service.GetOrder(new GetOrderRequest(firstOrderId));
            var secondOrder = composition.Service.GetOrder(new GetOrderRequest(secondOrderId));

            Assert.That(results.Count(result => result.IsSuccess), Is.EqualTo(1));
            Assert.That(results.Count(result => result.Error == OperationErrorCode.KitchenDeclined), Is.EqualTo(1));
            Assert.That(new[] { firstOrder.Value!.Status, secondOrder.Value!.Status }
                .Count(status => status == OrderStatusDto.Confirmed), Is.EqualTo(1));
            Assert.That(new[] { firstOrder.Value.Status, secondOrder.Value.Status }
                .Count(status => status == OrderStatusDto.Draft), Is.EqualTo(1));
            Assert.That(composition.Kitchen.GetAvailableQuantity(IngredientTypeDto.Chocolate), Is.EqualTo(0));
        }

        [Test]
        [Property("Requirement", "FR-1")]
        [Property("Requirement", "FR-3")]
        [Property("Requirement", "FR-4")]
        [Property("Requirement", "FR-5")]
        [Property("Requirement", "FR-6")]
        [Property("Requirement", "FR-10")]
        public async Task MainFlow_WithEnoughKitchenStock_ReachesArchived()
        {
            var composition = CreateComposition(new Dictionary<IngredientTypeDto, int>
            {
                [IngredientTypeDto.Honey] = 5
            });
            var orderId = await CreateDraftOrderWithPancakeAsync(composition.Service, IngredientTypeDto.Honey);

            var confirmResult = await composition.Service.ConfirmOrderAsync(new ConfirmOrderRequest(orderId));
            var startPreparationResult = await composition.ApplicationService.StartPreparationAsync(orderId);
            var completePreparationResult = await composition.ApplicationService.CompletePreparationAsync(orderId);
            var startDeliveryResult = await composition.ApplicationService.StartDeliveryAsync(orderId);
            var completeDeliveryResult = await composition.ApplicationService.CompleteDeliveryAsync(orderId);
            var archiveResult = await composition.ApplicationService.ArchiveAsync(orderId);
            var orderResult = composition.Service.GetOrder(new GetOrderRequest(orderId));

            Assert.That(confirmResult.IsSuccess, Is.True);
            Assert.That(startPreparationResult.IsSuccess, Is.True);
            Assert.That(completePreparationResult.IsSuccess, Is.True);
            Assert.That(startDeliveryResult.IsSuccess, Is.True);
            Assert.That(completeDeliveryResult.IsSuccess, Is.True);
            Assert.That(archiveResult.IsSuccess, Is.True);
            Assert.That(orderResult.IsSuccess, Is.True);
            Assert.That(orderResult.Value!.Status, Is.EqualTo(OrderStatusDto.Archived));
        }

        [Test]
        [Property("Requirement", "FR-8")]
        [Property("Requirement", "FR-9")]
        [Property("Requirement", "FR-10")]
        public async Task RemovingDraftItems_DoesNotRestoreStock()
        {
            var composition = CreateComposition(new Dictionary<IngredientTypeDto, int>
            {
                [IngredientTypeDto.Honey] = 1,
                [IngredientTypeDto.Jam] = 1
            });
            var orderId = CreateOrder(composition.Service);
            var addPancakeResult = await composition.Service.AddPancakeAsync(
                new AddPancakeRequest(orderId, new[] { IngredientTypeDto.Honey }));
            var pancakeId = addPancakeResult.Value!.Pancakes.Single().PancakeId;
            var removePancakeResult = await composition.Service.RemovePancakeAsync(
                new RemovePancakeRequest(orderId, pancakeId));
            var addEmptyPancakeResult = await composition.Service.AddPancakeAsync(
                new AddPancakeRequest(orderId, Array.Empty<IngredientTypeDto>()));
            var emptyPancakeId = addEmptyPancakeResult.Value!.Pancakes.Single().PancakeId;
            var addIngredientResult = await composition.Service.AddIngredientAsync(
                new AddIngredientRequest(orderId, emptyPancakeId, IngredientTypeDto.Jam));
            var removeIngredientResult = await composition.Service.RemoveIngredientAsync(
                new RemoveIngredientRequest(orderId, emptyPancakeId, IngredientTypeDto.Jam));

            Assert.That(removePancakeResult.IsSuccess, Is.True);
            Assert.That(addIngredientResult.IsSuccess, Is.True);
            Assert.That(removeIngredientResult.IsSuccess, Is.True);
            Assert.That(composition.Kitchen.GetAvailableQuantity(IngredientTypeDto.Honey), Is.EqualTo(1));
            Assert.That(composition.Kitchen.GetAvailableQuantity(IngredientTypeDto.Jam), Is.EqualTo(1));
        }

        private static TestComposition CreateComposition(
            IReadOnlyDictionary<IngredientTypeDto, int> stock,
            bool checkDraftAvailability = true)
        {
            var snapshotStore = new OrderSnapshotStore();
            var queryService = new OrderQueryService(snapshotStore);
            var kitchen = new InMemoryKitchen(queryService, stock);
            var applicationService = new OrderApplicationService(
                kitchen,
                new DeliveryGatewayFake(),
                new ArchiveGatewayFake(),
                checkDraftAvailability ? kitchen : new IngredientAvailabilityFake(),
                snapshotStore);
            var service = new PancakeOrderingService(applicationService, queryService);

            return new TestComposition(applicationService, service, kitchen);
        }

        private static Guid CreateOrder(IPancakeOrderingService service)
        {
            var result = service.CreateOrder(new CreateOrderRequest(CreateAddress()));

            Assert.That(result.IsSuccess, Is.True);
            return result.Value!.OrderId;
        }

        private static async Task<Guid> CreateDraftOrderWithPancakeAsync(
            IPancakeOrderingService service,
            IngredientTypeDto ingredient)
        {
            var orderId = CreateOrder(service);
            await AddPancakeAsync(service, orderId, ingredient);

            return orderId;
        }

        private static async Task AddPancakeAsync(
            IPancakeOrderingService service,
            Guid orderId,
            IngredientTypeDto ingredient)
        {
            var result = await service.AddPancakeAsync(new AddPancakeRequest(orderId, new[] { ingredient }));

            Assert.That(result.IsSuccess, Is.True);
        }

        private static OrderDto CreateOrderDto(
            Guid orderId,
            OrderStatusDto status,
            IReadOnlyCollection<PancakeDto> pancakes)
        {
            return new OrderDto(
                orderId,
                status,
                CreateAddress(),
                Array.AsReadOnly(pancakes.ToArray()));
        }

        private static PancakeDto CreatePancakeDto(
            int pancakeId,
            params IngredientTypeDto[] ingredients)
        {
            return new PancakeDto(
                pancakeId,
                Array.AsReadOnly(ingredients.ToArray()));
        }

        private static DeliveryAddressDto CreateAddress() =>
            new("Main Street", "Tel Aviv", "Israel");

        private sealed class TestComposition
        {
            public TestComposition(
                OrderApplicationService applicationService,
                IPancakeOrderingService service,
                InMemoryKitchen kitchen)
            {
                ApplicationService = applicationService;
                Service = service;
                Kitchen = kitchen;
            }

            public OrderApplicationService ApplicationService { get; }

            public IPancakeOrderingService Service { get; }

            public InMemoryKitchen Kitchen { get; }
        }

        private sealed class QueryServiceSpy : IOrderQueryService
        {
            private readonly OrderDto _order;
            private readonly List<Guid> _orderIds = new();

            public QueryServiceSpy(OrderDto order)
            {
                _order = order;
            }

            public int CallCount => _orderIds.Count;

            public IReadOnlyList<Guid> OrderIds => _orderIds;

            public OperationResult<OrderDto> GetOrder(GetOrderRequest request)
            {
                _orderIds.Add(request.OrderId);
                return OperationResult<OrderDto>.Success(_order);
            }
        }

        private sealed class IngredientAvailabilityFake : IIngredientAvailability
        {
            public Task<CoreResults.Result> CheckAvailabilityAsync(IReadOnlyCollection<IngredientTypeDto> ingredients) =>
                Task.FromResult(CoreResults.Result.Success());
        }

        private sealed class DeliveryGatewayFake : IDeliveryGateway
        {
            public Task<CoreResults.Result> SubmitOrderAsync(Guid orderId) =>
                Task.FromResult(CoreResults.Result.Success());
        }

        private sealed class ArchiveGatewayFake : IArchiveGateway
        {
            public Task<CoreResults.Result> ArchiveOrderAsync(Guid orderId) =>
                Task.FromResult(CoreResults.Result.Success());
        }
    }
}
