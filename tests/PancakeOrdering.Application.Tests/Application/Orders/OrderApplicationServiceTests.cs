using System.Collections.Concurrent;
using PancakeOrdering.Core.Application;
using PancakeOrdering.Core.Application.Ports;
using PancakeOrdering.Core.Common.Results;
using PancakeOrdering.Core.Domain.Enums;
using PancakeOrdering.Core.Domain.Orders;

namespace PancakeOrdering.Application.Tests.Application.Orders
{
    public sealed class OrderApplicationServiceTests
    {
        private static readonly TimeSpan TestTimeout = TimeSpan.FromSeconds(3);

        [Test]
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
        public async Task Command_WithUnknownOrder_ReturnsOrderNotFound()
        {
            var service = CreateService();

            var result = await service.CancelAsync(Guid.NewGuid());

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error, Is.EqualTo(ErrorCode.OrderNotFound));
        }

        [Test]
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
        public async Task MainLifecycle_WithAcceptedKitchen_ReachesArchived()
        {
            var service = CreateService();
            var orderId = CreateOrder(service);

            var addPancakeResult = await service.AddPancakeAsync(orderId, Ingredient.Honey);
            var confirmResult = await service.ConfirmAsync(orderId);
            var startPreparationResult = await service.StartPreparationAsync(orderId);
            var completePreparationResult = await service.CompletePreparationAsync(orderId);
            var startDeliveryResult = await service.StartDeliveryAsync(orderId);
            var completeDeliveryResult = await service.CompleteDeliveryAsync(orderId);
            var archiveResult = await service.ArchiveAsync(orderId);
            var cancelResult = await service.CancelAsync(orderId);

            Assert.That(addPancakeResult.IsSuccess, Is.True);
            Assert.That(confirmResult.IsSuccess, Is.True);
            Assert.That(startPreparationResult.IsSuccess, Is.True);
            Assert.That(completePreparationResult.IsSuccess, Is.True);
            Assert.That(startDeliveryResult.IsSuccess, Is.True);
            Assert.That(completeDeliveryResult.IsSuccess, Is.True);
            Assert.That(archiveResult.IsSuccess, Is.True);
            Assert.That(cancelResult.IsSuccess, Is.False);
            Assert.That(cancelResult.Error, Is.EqualTo(ErrorCode.InvalidTransition));
        }

        [Test]
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

        private static OrderApplicationService CreateService(IKitchenGateway? kitchenGateway = null) =>
            new(kitchenGateway ?? new KitchenGatewayFake());

        private static DeliveryAddress CreateAddress() =>
            new("Main Street", "Tel Aviv", "Israel");

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
