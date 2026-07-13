using System.Collections.Concurrent;
using PancakeOrdering.Application;
using PancakeOrdering.Application.Orders.Snapshots;
using PancakeOrdering.Application.Ports;
using PancakeOrdering.Contracts.Dtos;
using PancakeOrdering.Contracts.Requests;
using PancakeOrdering.Core.Common.Results;
using PancakeOrdering.Core.Domain.Enums;
using PancakeOrdering.Core.Domain.Orders;

namespace PancakeOrdering.Application.Tests.Application.Orders
{
    [TestFixture]
    [Property("TestSuiteId", "TST-04")]
    public sealed class OrderApplicationServiceTests
    {
        private static readonly TimeSpan TestTimeout = TimeSpan.FromSeconds(3);

        [Test]
        [Property("TestId", "TST-04.01")]
        [Property("Requirement", "FR-1")]
        [Property("Design", "SDD-3.1")]
        [Property("Design", "SDD-6.2")]
        public void CreateOrder_ReturnsStableNonEmptyUniqueIds()
        {
            var service = CreateService();

            var firstResult = service.CreateOrder(CreateAddress());
            var secondResult = service.CreateOrder(CreateAddress());

            Assert.That(firstResult.IsSuccess, Is.True);
            Assert.That(secondResult.IsSuccess, Is.True);
            Assert.That(firstResult.Value, Is.Not.EqualTo(Guid.Empty));
            Assert.That(secondResult.Value, Is.Not.EqualTo(Guid.Empty));
            Assert.That(firstResult.Value, Is.Not.EqualTo(secondResult.Value));
        }

        [Test]
        [Property("TestId", "TST-04.02")]
        [Property("Requirement", "NFR-4")]
        [Property("Design", "SDD-4.5")]
        [Property("Design", "SDD-4.6")]
        public async Task Command_WithUnknownOrder_ReturnsOrderNotFound()
        {
            var service = CreateService();

            var result = await service.CancelAsync(Guid.NewGuid());

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error, Is.EqualTo(ErrorCode.OrderNotFound));
        }

        [Test]
        [Property("TestId", "TST-04.03")]
        [Property("Requirement", "FR-3")]
        [Property("Requirement", "FR-4")]
        [Property("Design", "SDD-6.4")]
        [Property("Design", "SDD-6.8.3")]
        public async Task Confirm_WhenKitchenAccepts_ConfirmsOrder()
        {
            var kitchen = new KitchenGatewayFake();
            var service = CreateService(kitchen);
            var orderId = CreateOrder(service);
            await AddPancakeAsync(service, orderId);

            var result = await service.ConfirmAsync(orderId);
            var startPreparationResult = await service.StartPreparationAsync(orderId);

            Assert.That(kitchen.CallCount, Is.EqualTo(1));
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(startPreparationResult.IsSuccess, Is.True);
        }

        [Test]
        [Property("TestId", "TST-04.04")]
        [Property("Requirement", "FR-3")]
        [Property("Design", "SDD-5.2")]
        [Property("Design", "SDD-6.8.3")]
        public async Task Confirm_WhenDraftOrderIsEmpty_DoesNotCallKitchen()
        {
            var kitchen = new KitchenGatewayFake();
            var service = CreateService(kitchen);
            var orderId = CreateOrder(service);

            var result = await service.ConfirmAsync(orderId);
            var addPancakeResult = await service.AddPancakeAsync(orderId, Ingredient.Honey);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error, Is.EqualTo(ErrorCode.OrderMustContainPancake));
            Assert.That(kitchen.CallCount, Is.EqualTo(0));
            Assert.That(addPancakeResult.IsSuccess, Is.True);
        }

        [Test]
        [Property("TestId", "TST-04.05")]
        [Property("Requirement", "FR-3")]
        [Property("Requirement", "FR-4")]
        [Property("Design", "SDD-6.4")]
        [Property("Design", "SDD-6.8.3")]
        public async Task Confirm_WhenKitchenDeclines_KeepsOrderDraft()
        {
            var kitchen = new KitchenGatewayFake(_ => Task.FromResult(Result.Failure(ErrorCode.KitchenDeclined)));
            var service = CreateService(kitchen);
            var orderId = CreateOrder(service);
            await AddPancakeAsync(service, orderId);

            var result = await service.ConfirmAsync(orderId);
            var addPancakeResult = await service.AddPancakeAsync(orderId, Ingredient.Jam);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error, Is.EqualTo(ErrorCode.KitchenDeclined));
            Assert.That(addPancakeResult.IsSuccess, Is.True);
        }

        [Test]
        [Property("TestId", "TST-04.06")]
        [Property("Requirement", "FR-1")]
        [Property("Requirement", "FR-3")]
        [Property("Requirement", "FR-4")]
        [Property("Requirement", "FR-5")]
        [Property("Requirement", "FR-6")]
        [Property("Requirement", "NFR-5")]
        [Property("Design", "SDD-5.3")]
        [Property("Design", "SDD-6.4")]
        [Property("Design", "SDD-6.8.3")]
        public async Task MainLifecycle_WithAcceptedKitchen_ReachesArchived()
        {
            var kitchen = new KitchenGatewayFake();
            var delivery = new DeliveryGatewayFake();
            var archive = new ArchiveGatewayFake();
            var service = CreateService(kitchen, delivery, archive);
            var orderId = CreateOrder(service);

            var addPancakeResult = await service.AddPancakeAsync(orderId, Ingredient.Honey);
            var confirmResult = await service.ConfirmAsync(orderId);
            var startPreparationResult = await service.StartPreparationAsync(orderId);
            var completePreparationResult = await service.CompletePreparationAsync(orderId);
            var startDeliveryResult = await service.StartDeliveryAsync(orderId);
            var completeDeliveryResult = await service.CompleteDeliveryAsync(orderId);
            var queryService = CreateQueryService(service);
            var deliveredOrder = queryService.GetOrder(new GetOrderRequest(orderId));
            var archiveResult = await service.ArchiveAsync(orderId);
            var archivedOrder = queryService.GetOrder(new GetOrderRequest(orderId));
            var cancelResult = await service.CancelAsync(orderId);

            Assert.That(addPancakeResult.IsSuccess, Is.True);
            Assert.That(confirmResult.IsSuccess, Is.True);
            Assert.That(startPreparationResult.IsSuccess, Is.True);
            Assert.That(completePreparationResult.IsSuccess, Is.True);
            Assert.That(startDeliveryResult.IsSuccess, Is.True);
            Assert.That(completeDeliveryResult.IsSuccess, Is.True);
            Assert.That(kitchen.CallCount, Is.EqualTo(1));
            Assert.That(delivery.CallCount, Is.EqualTo(1));
            Assert.That(delivery.OrderIds, Is.EqualTo(new[] { orderId }));
            Assert.That(deliveredOrder.IsSuccess, Is.True);
            Assert.That(deliveredOrder.Value!.Status, Is.EqualTo(OrderStatusDto.Delivered));
            Assert.That(archive.CallCount, Is.EqualTo(1));
            Assert.That(archive.OrderIds, Is.EqualTo(new[] { orderId }));
            Assert.That(archiveResult.IsSuccess, Is.True);
            Assert.That(archivedOrder.IsSuccess, Is.True);
            Assert.That(archivedOrder.Value!.Status, Is.EqualTo(OrderStatusDto.Archived));
            Assert.That(cancelResult.IsSuccess, Is.False);
            Assert.That(cancelResult.Error, Is.EqualTo(ErrorCode.InvalidTransition));
        }

        [Test]
        [Property("TestId", "TST-04.07")]
        [Property("Requirement", "FR-5")]
        [Property("Design", "SDD-4.2")]
        [Property("Design", "SDD-6.4")]
        public async Task CompletePreparation_WhenValid_SubmitsOrderToDelivery()
        {
            var delivery = new DeliveryGatewayFake();
            var service = CreateService(deliveryGateway: delivery);
            var orderId = await CreatePreparingOrderAsync(service);

            var result = await service.CompletePreparationAsync(orderId);
            var startDeliveryResult = await service.StartDeliveryAsync(orderId);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(delivery.CallCount, Is.EqualTo(1));
            Assert.That(delivery.OrderIds, Is.EqualTo(new[] { orderId }));
            Assert.That(startDeliveryResult.IsSuccess, Is.True);
        }

        [Test]
        [Property("TestId", "TST-04.08")]
        [Property("Requirement", "FR-5")]
        [Property("Design", "SDD-5.3")]
        [Property("Design", "SDD-6.4")]
        public async Task CompletePreparation_WhenTransitionIsInvalid_DoesNotCallDelivery()
        {
            var delivery = new DeliveryGatewayFake();
            var service = CreateService(deliveryGateway: delivery);
            var orderId = await CreateConfirmedOrderAsync(service);

            var result = await service.CompletePreparationAsync(orderId);
            var startPreparationResult = await service.StartPreparationAsync(orderId);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error, Is.EqualTo(ErrorCode.InvalidTransition));
            Assert.That(delivery.CallCount, Is.EqualTo(0));
            Assert.That(startPreparationResult.IsSuccess, Is.True);
        }

        [Test]
        [Property("TestId", "TST-04.09")]
        [Property("Requirement", "FR-5")]
        [Property("Design", "SDD-6.4")]
        public async Task CompletePreparation_WhenDeliverySubmissionFails_LeavesOrderPrepared()
        {
            var delivery = new DeliveryGatewayFake(_ => Task.FromResult(Result.Failure(ErrorCode.DeliveryFailed)));
            var service = CreateService(deliveryGateway: delivery);
            var orderId = await CreatePreparingOrderAsync(service);

            var result = await service.CompletePreparationAsync(orderId);
            var order = CreateQueryService(service).GetOrder(new GetOrderRequest(orderId));

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error, Is.EqualTo(ErrorCode.DeliveryFailed));
            Assert.That(delivery.CallCount, Is.EqualTo(1));
            Assert.That(order.IsSuccess, Is.True);
            Assert.That(order.Value!.Status, Is.EqualTo(OrderStatusDto.Prepared));
        }

        [Test]
        [Property("TestId", "TST-04.10")]
        [Property("Requirement", "FR-5")]
        [Property("Requirement", "NFR-5")]
        [Property("Design", "SDD-6.1")]
        [Property("Design", "SDD-6.4")]
        public async Task CompletePreparation_WaitsForDeliverySubmissionBeforeNextOrderCommand()
        {
            var delivery = new ControlledDeliveryGateway();
            var service = CreateService(deliveryGateway: delivery);
            var orderId = await CreatePreparingOrderAsync(service);
            delivery.Block(orderId);

            var completePreparationTask = service.CompletePreparationAsync(orderId);
            await delivery.WaitUntilEnteredAsync(orderId);

            var startDeliveryTask = service.StartDeliveryAsync(orderId);
            await Task.Yield();

            Assert.That(startDeliveryTask.IsCompleted, Is.False);

            delivery.Release(orderId, Result.Success());

            var completePreparationResult = await WaitForAsync(completePreparationTask);
            var startDeliveryResult = await WaitForAsync(startDeliveryTask);

            Assert.That(completePreparationResult.IsSuccess, Is.True);
            Assert.That(startDeliveryResult.IsSuccess, Is.True);
        }

        [Test]
        [Property("TestId", "TST-04.11")]
        [Property("Requirement", "FR-5")]
        [Property("Design", "SDD-5.3")]
        public async Task CompleteDelivery_WhenValid_EndsInDelivered()
        {
            var archive = new ArchiveGatewayFake();
            var service = CreateService(archiveGateway: archive);
            var orderId = await CreateOutForDeliveryOrderAsync(service);

            var result = await service.CompleteDeliveryAsync(orderId);
            var order = CreateQueryService(service).GetOrder(new GetOrderRequest(orderId));

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(order.IsSuccess, Is.True);
            Assert.That(order.Value!.Status, Is.EqualTo(OrderStatusDto.Delivered));
            Assert.That(archive.CallCount, Is.EqualTo(0));
        }

        [Test]
        [Property("TestId", "TST-04.12")]
        [Property("Requirement", "FR-6")]
        [Property("Design", "SDD-5.3")]
        [Property("Design", "SDD-6.4")]
        public async Task CompleteDelivery_WhenTransitionIsInvalid_DoesNotArchive()
        {
            var archive = new ArchiveGatewayFake();
            var service = CreateService(archiveGateway: archive);
            var orderId = await CreatePreparedOrderAsync(service);

            var result = await service.CompleteDeliveryAsync(orderId);
            var order = CreateQueryService(service).GetOrder(new GetOrderRequest(orderId));

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error, Is.EqualTo(ErrorCode.InvalidTransition));
            Assert.That(order.IsSuccess, Is.True);
            Assert.That(order.Value!.Status, Is.EqualTo(OrderStatusDto.Prepared));
            Assert.That(archive.CallCount, Is.EqualTo(0));
        }

        [Test]
        [Property("TestId", "TST-04.13")]
        [Property("Requirement", "FR-6")]
        [Property("Design", "SDD-4.2")]
        [Property("Design", "SDD-6.4")]
        public async Task Archive_WhenGatewaySucceeds_ReachesArchived()
        {
            var archive = new ArchiveGatewayFake();
            var service = CreateService(archiveGateway: archive);
            var orderId = await CreateDeliveredOrderAsync(service);

            var result = await service.ArchiveAsync(orderId);
            var order = CreateQueryService(service).GetOrder(new GetOrderRequest(orderId));

            Assert.That(archive.CallCount, Is.EqualTo(1));
            Assert.That(archive.OrderIds, Is.EqualTo(new[] { orderId }));
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(order.IsSuccess, Is.True);
            Assert.That(order.Value!.Status, Is.EqualTo(OrderStatusDto.Archived));
        }

        [Test]
        [Property("TestId", "TST-04.14")]
        [Property("Requirement", "FR-6")]
        [Property("Design", "SDD-5.3")]
        [Property("Design", "SDD-6.4")]
        public async Task Archive_WhenTransitionIsInvalid_DoesNotCallGateway()
        {
            var archive = new ArchiveGatewayFake();
            var service = CreateService(archiveGateway: archive);
            var orderId = await CreatePreparedOrderAsync(service);

            var result = await service.ArchiveAsync(orderId);
            var order = CreateQueryService(service).GetOrder(new GetOrderRequest(orderId));

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error, Is.EqualTo(ErrorCode.InvalidTransition));
            Assert.That(archive.CallCount, Is.EqualTo(0));
            Assert.That(order.IsSuccess, Is.True);
            Assert.That(order.Value!.Status, Is.EqualTo(OrderStatusDto.Prepared));
        }

        [Test]
        [Property("TestId", "TST-04.15")]
        [Property("Requirement", "FR-6")]
        [Property("Design", "SDD-6.4")]
        public async Task Archive_WhenGatewayFails_LeavesOrderDelivered()
        {
            var archive = new ArchiveGatewayFake(_ => Task.FromResult(Result.Failure(ErrorCode.ArchiveFailed)));
            var service = CreateService(archiveGateway: archive);
            var orderId = await CreateDeliveredOrderAsync(service);

            var result = await service.ArchiveAsync(orderId);
            var order = CreateQueryService(service).GetOrder(new GetOrderRequest(orderId));

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error, Is.EqualTo(ErrorCode.ArchiveFailed));
            Assert.That(archive.CallCount, Is.EqualTo(1));
            Assert.That(order.IsSuccess, Is.True);
            Assert.That(order.Value!.Status, Is.EqualTo(OrderStatusDto.Delivered));
        }

        [Test]
        [Property("TestId", "TST-04.16")]
        [Property("Requirement", "FR-6")]
        [Property("Requirement", "NFR-5")]
        [Property("Design", "SDD-6.1")]
        [Property("Design", "SDD-6.4")]
        public async Task Archive_WaitsForGatewayBeforeNextOrderCommand()
        {
            var archive = new ControlledArchiveGateway();
            var service = CreateService(archiveGateway: archive);
            var orderId = await CreateDeliveredOrderAsync(service);
            archive.Block(orderId);

            var archiveTask = service.ArchiveAsync(orderId);
            await archive.WaitUntilEnteredAsync(orderId);

            var secondArchiveTask = service.ArchiveAsync(orderId);
            await Task.Yield();

            Assert.That(secondArchiveTask.IsCompleted, Is.False);

            archive.Release(orderId, Result.Success());

            var archiveResult = await WaitForAsync(archiveTask);
            var secondArchiveResult = await WaitForAsync(secondArchiveTask);
            var order = CreateQueryService(service).GetOrder(new GetOrderRequest(orderId));

            Assert.That(archiveResult.IsSuccess, Is.True);
            Assert.That(secondArchiveResult.IsSuccess, Is.False);
            Assert.That(secondArchiveResult.Error, Is.EqualTo(ErrorCode.InvalidTransition));
            Assert.That(archive.CallCount, Is.EqualTo(1));
            Assert.That(order.IsSuccess, Is.True);
            Assert.That(order.Value!.Status, Is.EqualTo(OrderStatusDto.Archived));
        }

        [Test]
        [Property("TestId", "TST-04.17")]
        [Property("Requirement", "NFR-5")]
        [Property("Design", "SDD-6.1")]
        [Property("Design", "SDD-6.4")]
        [Property("Design", "SDD-6.8.3")]
        [Property("Design", "SDD-7.2.2")]
        public async Task SameOrder_WaitsForCompletePreviousCommandIncludingKitchen()
        {
            var kitchen = new ControlledKitchenGateway();
            var service = CreateService(kitchen);
            var orderId = CreateOrder(service);
            kitchen.Block(orderId);
            await AddPancakeAsync(service, orderId);

            var confirmTask = service.ConfirmAsync(orderId);
            await kitchen.WaitUntilEnteredAsync(orderId);

            var cancelTask = service.CancelAsync(orderId);
            await Task.Yield();

            Assert.That(cancelTask.IsCompleted, Is.False);

            kitchen.Release(orderId, Result.Success());

            var confirmResult = await WaitForAsync(confirmTask);
            var cancelResult = await WaitForAsync(cancelTask);
            var addPancakeResult = await service.AddPancakeAsync(orderId, Ingredient.Jam);

            Assert.That(confirmResult.IsSuccess, Is.True);
            Assert.That(cancelResult.IsSuccess, Is.True);
            Assert.That(addPancakeResult.IsSuccess, Is.False);
            Assert.That(addPancakeResult.Error, Is.EqualTo(ErrorCode.CannotAddOrRemovePancakeInCurrentState));
        }

        [Test]
        [Property("TestId", "TST-04.18")]
        [Property("Requirement", "NFR-5")]
        [Property("Design", "SDD-6.5")]
        public async Task DifferentOrders_ExecuteConcurrently()
        {
            var kitchen = new ControlledKitchenGateway();
            var service = CreateService(kitchen);
            var firstOrderId = CreateOrder(service);
            var secondOrderId = CreateOrder(service);
            kitchen.Block(firstOrderId);
            await AddPancakeAsync(service, firstOrderId);
            await AddPancakeAsync(service, secondOrderId);

            var firstConfirmTask = service.ConfirmAsync(firstOrderId);
            await kitchen.WaitUntilEnteredAsync(firstOrderId);

            var secondConfirmTask = service.ConfirmAsync(secondOrderId);
            await kitchen.WaitUntilEnteredAsync(secondOrderId);

            var secondConfirmResult = await WaitForAsync(secondConfirmTask);
            var secondStartPreparationResult = await service.StartPreparationAsync(secondOrderId);

            Assert.That(secondConfirmResult.IsSuccess, Is.True);
            Assert.That(secondStartPreparationResult.IsSuccess, Is.True);
            Assert.That(firstConfirmTask.IsCompleted, Is.False);

            kitchen.Release(firstOrderId, Result.Success());

            var firstConfirmResult = await WaitForAsync(firstConfirmTask);
            var firstStartPreparationResult = await service.StartPreparationAsync(firstOrderId);

            Assert.That(firstConfirmResult.IsSuccess, Is.True);
            Assert.That(firstStartPreparationResult.IsSuccess, Is.True);
        }

        [Test]
        [Property("TestId", "TST-04.19")]
        [Property("Requirement", "NFR-5")]
        [Property("Design", "SDD-6.1")]
        [Property("Design", "SDD-6.4")]
        [Property("Design", "SDD-7.2.2")]
        public async Task CompetingCustomerAndKitchenCommands_FollowEnqueueOrder()
        {
            var service = CreateService();
            var firstOrderId = await CreateConfirmedOrderAsync(service);
            var secondOrderId = await CreateConfirmedOrderAsync(service);

            var firstStartPreparationTask = service.StartPreparationAsync(firstOrderId);
            var firstCancelTask = service.CancelAsync(firstOrderId);

            var firstStartPreparationResult = await firstStartPreparationTask;
            var firstCancelResult = await firstCancelTask;
            var firstCompletePreparationResult = await service.CompletePreparationAsync(firstOrderId);

            var secondCancelTask = service.CancelAsync(secondOrderId);
            var secondStartPreparationTask = service.StartPreparationAsync(secondOrderId);

            var secondCancelResult = await secondCancelTask;
            var secondStartPreparationResult = await secondStartPreparationTask;
            var secondAddPancakeResult = await service.AddPancakeAsync(secondOrderId, Ingredient.Jam);

            Assert.That(firstStartPreparationResult.IsSuccess, Is.True);
            Assert.That(firstCancelResult.IsSuccess, Is.False);
            Assert.That(firstCancelResult.Error, Is.EqualTo(ErrorCode.InvalidTransition));
            Assert.That(firstCompletePreparationResult.IsSuccess, Is.True);

            Assert.That(secondCancelResult.IsSuccess, Is.True);
            Assert.That(secondStartPreparationResult.IsSuccess, Is.False);
            Assert.That(secondStartPreparationResult.Error, Is.EqualTo(ErrorCode.InvalidTransition));
            Assert.That(secondAddPancakeResult.IsSuccess, Is.False);
            Assert.That(secondAddPancakeResult.Error, Is.EqualTo(ErrorCode.CannotAddOrRemovePancakeInCurrentState));
        }

        private static OrderApplicationService CreateService(
            IKitchenGateway? kitchenGateway = null,
            IDeliveryGateway? deliveryGateway = null,
            IArchiveGateway? archiveGateway = null,
            IIngredientAvailability? ingredientAvailability = null)
        {
            var snapshotStore = new OrderSnapshotStore();

            return new OrderApplicationService(
                kitchenGateway ?? new KitchenGatewayFake(),
                deliveryGateway ?? new DeliveryGatewayFake(),
                archiveGateway ?? new ArchiveGatewayFake(),
                ingredientAvailability ?? new IngredientAvailabilityFake(),
                snapshotStore);
        }

        private static DeliveryAddress CreateAddress() =>
            new("Main Street", "Tel Aviv", "Israel");

        private static OrderQueryService CreateQueryService(OrderApplicationService service) =>
            new(service.SnapshotStore);

        private static Guid CreateOrder(OrderApplicationService service)
        {
            var result = service.CreateOrder(CreateAddress());

            Assert.That(result.IsSuccess, Is.True);
            return result.Value;
        }

        private static async Task AddPancakeAsync(OrderApplicationService service, Guid orderId)
        {
            var result = await service.AddPancakeAsync(orderId, Ingredient.Honey);

            Assert.That(result.IsSuccess, Is.True);
        }

        private static async Task<Guid> CreateConfirmedOrderAsync(OrderApplicationService service)
        {
            var orderId = CreateOrder(service);
            await AddPancakeAsync(service, orderId);

            var confirmResult = await service.ConfirmAsync(orderId);

            Assert.That(confirmResult.IsSuccess, Is.True);
            return orderId;
        }

        private static async Task<Guid> CreatePreparingOrderAsync(OrderApplicationService service)
        {
            var orderId = await CreateConfirmedOrderAsync(service);
            var startPreparationResult = await service.StartPreparationAsync(orderId);

            Assert.That(startPreparationResult.IsSuccess, Is.True);
            return orderId;
        }

        private static async Task<Guid> CreatePreparedOrderAsync(OrderApplicationService service)
        {
            var orderId = await CreatePreparingOrderAsync(service);
            var completePreparationResult = await service.CompletePreparationAsync(orderId);

            Assert.That(completePreparationResult.IsSuccess, Is.True);
            return orderId;
        }

        private static async Task<Guid> CreateOutForDeliveryOrderAsync(OrderApplicationService service)
        {
            var orderId = await CreatePreparedOrderAsync(service);
            var startDeliveryResult = await service.StartDeliveryAsync(orderId);

            Assert.That(startDeliveryResult.IsSuccess, Is.True);
            return orderId;
        }

        private static async Task<Guid> CreateDeliveredOrderAsync(OrderApplicationService service)
        {
            var orderId = await CreateOutForDeliveryOrderAsync(service);
            var completeDeliveryResult = await service.CompleteDeliveryAsync(orderId);

            Assert.That(completeDeliveryResult.IsSuccess, Is.True);
            return orderId;
        }

        private static async Task<T> WaitForAsync<T>(Task<T> task) =>
            await task.WaitAsync(TestTimeout);

        private sealed class KitchenGatewayFake : IKitchenGateway
        {
            private readonly Func<Guid, Task<Result>> _acceptOrder;
            private readonly ConcurrentQueue<Guid> _calls = new();

            public KitchenGatewayFake(Func<Guid, Task<Result>>? acceptOrder = null)
            {
                _acceptOrder = acceptOrder ?? (_ => Task.FromResult(Result.Success()));
            }

            public int CallCount => _calls.Count;

            public Task<Result> AcceptOrderAsync(Guid orderId)
            {
                _calls.Enqueue(orderId);
                return _acceptOrder(orderId);
            }
        }

        private sealed class IngredientAvailabilityFake : IIngredientAvailability
        {
            public Task<Result> CheckAvailabilityAsync(IReadOnlyCollection<IngredientTypeDto> ingredients) =>
                Task.FromResult(Result.Success());
        }

        private sealed class DeliveryGatewayFake : IDeliveryGateway
        {
            private readonly Func<Guid, Task<Result>> _submitOrder;
            private readonly ConcurrentQueue<Guid> _calls = new();

            public DeliveryGatewayFake(Func<Guid, Task<Result>>? submitOrder = null)
            {
                _submitOrder = submitOrder ?? (_ => Task.FromResult(Result.Success()));
            }

            public int CallCount => _calls.Count;

            public Guid[] OrderIds => _calls.ToArray();

            public Task<Result> SubmitOrderAsync(Guid orderId)
            {
                _calls.Enqueue(orderId);
                return _submitOrder(orderId);
            }
        }

        private sealed class ArchiveGatewayFake : IArchiveGateway
        {
            private readonly Func<Guid, Task<Result>> _archiveOrder;
            private readonly ConcurrentQueue<Guid> _calls = new();

            public ArchiveGatewayFake(Func<Guid, Task<Result>>? archiveOrder = null)
            {
                _archiveOrder = archiveOrder ?? (_ => Task.FromResult(Result.Success()));
            }

            public int CallCount => _calls.Count;

            public Guid[] OrderIds => _calls.ToArray();

            public Task<Result> ArchiveOrderAsync(Guid orderId)
            {
                _calls.Enqueue(orderId);
                return _archiveOrder(orderId);
            }
        }

        private sealed class ControlledArchiveGateway : IArchiveGateway
        {
            private readonly ConcurrentDictionary<Guid, TaskCompletionSource<bool>> _enteredSignals = new();
            private readonly ConcurrentDictionary<Guid, TaskCompletionSource<Result>> _blockedOrders = new();
            private readonly ConcurrentQueue<Guid> _calls = new();

            public int CallCount => _calls.Count;

            public void Block(Guid orderId)
            {
                _blockedOrders[orderId] = NewCompletionSource<Result>();
            }

            public async Task WaitUntilEnteredAsync(Guid orderId)
            {
                var signal = _enteredSignals.GetOrAdd(orderId, _ => NewCompletionSource<bool>());
                await signal.Task.WaitAsync(TestTimeout);
            }

            public void Release(Guid orderId, Result result)
            {
                _blockedOrders[orderId].SetResult(result);
            }

            public Task<Result> ArchiveOrderAsync(Guid orderId)
            {
                _calls.Enqueue(orderId);

                _enteredSignals
                    .GetOrAdd(orderId, _ => NewCompletionSource<bool>())
                    .TrySetResult(true);

                return _blockedOrders.TryGetValue(orderId, out var release)
                    ? release.Task
                    : Task.FromResult(Result.Success());
            }

            private static TaskCompletionSource<T> NewCompletionSource<T>() =>
                new(TaskCreationOptions.RunContinuationsAsynchronously);
        }

        private sealed class ControlledDeliveryGateway : IDeliveryGateway
        {
            private readonly ConcurrentDictionary<Guid, TaskCompletionSource<bool>> _enteredSignals = new();
            private readonly ConcurrentDictionary<Guid, TaskCompletionSource<Result>> _blockedOrders = new();

            public void Block(Guid orderId)
            {
                _blockedOrders[orderId] = NewCompletionSource<Result>();
            }

            public async Task WaitUntilEnteredAsync(Guid orderId)
            {
                var signal = _enteredSignals.GetOrAdd(orderId, _ => NewCompletionSource<bool>());
                await signal.Task.WaitAsync(TestTimeout);
            }

            public void Release(Guid orderId, Result result)
            {
                _blockedOrders[orderId].SetResult(result);
            }

            public Task<Result> SubmitOrderAsync(Guid orderId)
            {
                _enteredSignals
                    .GetOrAdd(orderId, _ => NewCompletionSource<bool>())
                    .TrySetResult(true);

                return _blockedOrders.TryGetValue(orderId, out var release)
                    ? release.Task
                    : Task.FromResult(Result.Success());
            }

            private static TaskCompletionSource<T> NewCompletionSource<T>() =>
                new(TaskCreationOptions.RunContinuationsAsynchronously);
        }

        private sealed class ControlledKitchenGateway : IKitchenGateway
        {
            private readonly ConcurrentDictionary<Guid, TaskCompletionSource<bool>> _enteredSignals = new();
            private readonly ConcurrentDictionary<Guid, TaskCompletionSource<Result>> _blockedOrders = new();

            public void Block(Guid orderId)
            {
                _blockedOrders[orderId] = NewCompletionSource<Result>();
            }

            public async Task WaitUntilEnteredAsync(Guid orderId)
            {
                var signal = _enteredSignals.GetOrAdd(orderId, _ => NewCompletionSource<bool>());
                await signal.Task.WaitAsync(TestTimeout);
            }

            public void Release(Guid orderId, Result result)
            {
                _blockedOrders[orderId].SetResult(result);
            }

            public Task<Result> AcceptOrderAsync(Guid orderId)
            {
                _enteredSignals
                    .GetOrAdd(orderId, _ => NewCompletionSource<bool>())
                    .TrySetResult(true);

                return _blockedOrders.TryGetValue(orderId, out var release)
                    ? release.Task
                    : Task.FromResult(Result.Success());
            }

            private static TaskCompletionSource<T> NewCompletionSource<T>() =>
                new(TaskCreationOptions.RunContinuationsAsynchronously);
        }
    }
}
