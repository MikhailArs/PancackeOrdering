using PancakeOrdering.Core.Common.Results;
using PancakeOrdering.Core.Domain.Enums;
using PancakeOrdering.Core.Domain.Orders;
using PancakeOrdering.Core.Domain.States;

namespace PancakeOrdering.Core.Tests.Domain.Orders
{
    public sealed class OrderLifecycleTests
    {
        [Test]
        public void FullValidLifecycle_ReachesArchived()
        {
            var order = CreateOrderWithPancake();

            AssertTransition(order.Confirm(),             order, OrderStatus.Confirmed);
            AssertTransition(order.StartPreparation(),    order, OrderStatus.Preparing);
            AssertTransition(order.CompletePreparation(), order, OrderStatus.Prepared);
            AssertTransition(order.StartDelivery(),       order, OrderStatus.OutForDelivery);
            AssertTransition(order.CompleteDelivery(),    order, OrderStatus.Delivered);
            AssertTransition(order.Archive(),             order, OrderStatus.Archived);
        }

        [Test]
        public void Cancel_FromDraft_ChangesStatusToCancelled()
        {
            var order = CreateOrder();

            var result = order.Cancel();

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(order.Status, Is.EqualTo(OrderStatus.Cancelled));
        }

        [Test]
        public void Cancel_FromConfirmed_ChangesStatusToCancelled()
        {
            var order = CreateOrderWithPancake();
            var confirmResult = order.Confirm();
            Assert.That(confirmResult.IsSuccess, Is.True);

            var result = order.Cancel();

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(order.Status, Is.EqualTo(OrderStatus.Cancelled));
        }

        [Test]
        public void StartPreparation_FromDraft_ReturnsInvalidTransitionAndKeepsDraft()
        {
            var order = CreateOrder();

            var result = order.StartPreparation();

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error, Is.EqualTo(ErrorCode.InvalidTransition));
            Assert.That(order.Status, Is.EqualTo(OrderStatus.Draft));
        }

        [Test]
        public void Confirm_FromConfirmed_ReturnsInvalidTransitionAndKeepsConfirmed()
        {
            var order = CreateOrderWithPancake();
            var confirmResult = order.Confirm();
            Assert.That(confirmResult.IsSuccess, Is.True);

            var result = order.Confirm();

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error, Is.EqualTo(ErrorCode.InvalidTransition));
            Assert.That(order.Status, Is.EqualTo(OrderStatus.Confirmed));
        }

        [Test]
        public void Confirm_WithoutPancakes_ReturnsSpecificErrorAndKeepsDraft()
        {
            var order = CreateOrder();

            var result = order.Confirm();

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error, Is.EqualTo(ErrorCode.OrderMustContainPancake));
            Assert.That(order.Status, Is.EqualTo(OrderStatus.Draft));
        }

        [Test]
        public void LifecycleOperation_FromCancelled_ReturnsInvalidTransitionAndKeepsCancelled()
        {
            var order = CreateOrder();
            var cancelResult = order.Cancel();
            Assert.That(cancelResult.IsSuccess, Is.True);

            var result = order.Confirm();

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error, Is.EqualTo(ErrorCode.InvalidTransition));
            Assert.That(order.Status, Is.EqualTo(OrderStatus.Cancelled));
        }

        [Test]
        public void LifecycleOperation_FromArchived_ReturnsInvalidTransitionAndKeepsArchived()
        {
            var order = CreateOrderWithPancake();
            AssertTransition(order.Confirm(),             order, OrderStatus.Confirmed);
            AssertTransition(order.StartPreparation(),    order, OrderStatus.Preparing);
            AssertTransition(order.CompletePreparation(), order, OrderStatus.Prepared);
            AssertTransition(order.StartDelivery(),       order, OrderStatus.OutForDelivery);
            AssertTransition(order.CompleteDelivery(),    order, OrderStatus.Delivered);
            AssertTransition(order.Archive(),             order, OrderStatus.Archived);

            var result = order.Cancel();

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error, Is.EqualTo(ErrorCode.InvalidTransition));
            Assert.That(order.Status, Is.EqualTo(OrderStatus.Archived));
        }

        [TestCase((int)OrderStatus.Draft,          (int)OrderAction.Confirm,             (int)OrderStatus.Confirmed)]
        [TestCase((int)OrderStatus.Draft,          (int)OrderAction.Cancel,              (int)OrderStatus.Cancelled)]
        [TestCase((int)OrderStatus.Confirmed,      (int)OrderAction.Cancel,              (int)OrderStatus.Cancelled)]
        [TestCase((int)OrderStatus.Confirmed,      (int)OrderAction.StartPreparation,    (int)OrderStatus.Preparing)]
        [TestCase((int)OrderStatus.Preparing,      (int)OrderAction.CompletePreparation, (int)OrderStatus.Prepared)]
        [TestCase((int)OrderStatus.Prepared,       (int)OrderAction.StartDelivery,       (int)OrderStatus.OutForDelivery)]
        [TestCase((int)OrderStatus.OutForDelivery, (int)OrderAction.CompleteDelivery,    (int)OrderStatus.Delivered)]
        [TestCase((int)OrderStatus.Delivered,      (int)OrderAction.Archive,             (int)OrderStatus.Archived)]
        public void TransitionRules_ValidPairsResolveExpectedTarget(
            int status,
            int action,
            int expectedTargetStatus)
        {
            var resolved = OrderStateTransitions.TryResolve((OrderStatus)status, (OrderAction)action, out var nextState);

            Assert.That(resolved, Is.True);
            Assert.That(nextState.Status, Is.EqualTo((OrderStatus)expectedTargetStatus));
        }

        [TestCase((int)OrderStatus.Draft,     (int)OrderAction.StartPreparation)]
        [TestCase((int)OrderStatus.Confirmed, (int)OrderAction.Confirm)]
        [TestCase((int)OrderStatus.Cancelled, (int)OrderAction.Confirm)]
        [TestCase((int)OrderStatus.Archived,  (int)OrderAction.Cancel)]
        public void TransitionRules_InvalidPairsDoNotResolve(int status, int action)
        {
            var resolved = OrderStateTransitions.TryResolve((OrderStatus)status, (OrderAction)action, out _);

            Assert.That(resolved, Is.False);
        }

        private static Order CreateOrderWithPancake()
        {
            var order = CreateOrder();
            var addPancakeResult = order.AddPancake(Ingredient.Honey);

            Assert.That(addPancakeResult.IsSuccess, Is.True);
            return order;
        }

        private static Order CreateOrder()
        {
            var result = Order.Create(new DeliveryAddress("Main Street", "Tel Aviv", "Israel"));

            Assert.That(result.IsSuccess, Is.True);
            return result.Value!;
        }

        private static void AssertTransition(Result result, Order order, OrderStatus expectedStatus)
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(order.Status, Is.EqualTo(expectedStatus));
        }
    }
}
