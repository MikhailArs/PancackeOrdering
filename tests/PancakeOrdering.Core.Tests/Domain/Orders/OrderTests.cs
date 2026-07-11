using PancakeOrdering.Core.Common.Results;
using PancakeOrdering.Core.Domain.Enums;
using PancakeOrdering.Core.Domain.Orders;

namespace PancakeOrdering.Core.Tests.Domain.Orders
{
    public sealed class OrderTests
    {
        [Test]
        public void Confirm_WithOnePancake_ChangesStatusToConfirmed()
        {
            var order = Order.Create();
            order.AddPancake(Ingredient.Chocolate);

            Assert.That(order.Status, Is.EqualTo(OrderStatus.Draft));

            var result = order.Confirm();

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(order.Status, Is.EqualTo(OrderStatus.Confirmed));
        }

        [Test]
        public void Confirm_WithoutPancakes_ReturnsFailureAndKeepsDraftStatus()
        {
            var order = Order.Create();

            Assert.That(order.Status, Is.EqualTo(OrderStatus.Draft));

            var result = order.Confirm();

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error, Is.EqualTo(ErrorCode.OrderMustContainPancake));
            Assert.That(order.Status, Is.EqualTo(OrderStatus.Draft));
        }
        
        [Test]
        public void AddPancake_OnDraftState_PancakeAdded()
        {
            var order = Order.Create();

            Assert.That(order.Status, Is.EqualTo(OrderStatus.Draft));

            var result = order.AddPancake(Ingredient.Honey);

            Assert.That(result.IsSuccess, Is.True);
        }
        
        [Test]
        public void RemovePancake_OnDraftState_PancakeRemoved()
        {
            var order = Order.Create();
            Assert.That(order.Status, Is.EqualTo(OrderStatus.Draft));

            var resultInt = order.AddPancake(Ingredient.Honey);
            Assert.That(resultInt.IsSuccess, Is.True);

            var pancakeToRemove = order.Pancakes.FirstOrDefault();
            Assert.That(pancakeToRemove != null, Is.True);

            var result = order.RemovePancake(pancakeToRemove!.Id);
            Assert.That(result.IsSuccess, Is.True);
        }

        [Test]
        public void RemoveNonExistingPancake_OnDraftState_PancakeAdded()
        {
            var order = Order.Create();
            Assert.That(order.Status, Is.EqualTo(OrderStatus.Draft));

            var resultInt = order.AddPancake(Ingredient.Honey);
            Assert.That(resultInt.IsSuccess, Is.True);

            var pancakeToRemove = order.Pancakes.FirstOrDefault();
            Assert.That(pancakeToRemove != null, Is.True);

            var result = order.RemovePancake(pancakeToRemove!.Id);
            Assert.That(result.IsSuccess, Is.True);
            
            result = order.RemovePancake(pancakeToRemove!.Id);
            Assert.That(result.IsSuccess, Is.False);
        }

        [Test]
        public void RemovePancake_OnConfirmedState_ReturnsFailure()
        {
            var order = Order.Create();
            Assert.That(order.Status, Is.EqualTo(OrderStatus.Draft));

            var resultInt = order.AddPancake(Ingredient.Honey);
            Assert.That(resultInt.IsSuccess, Is.True);

            var result = order.Confirm();
            Assert.That(result.IsSuccess, Is.True);

            var pancakeToRemove = order.Pancakes.FirstOrDefault();
            Assert.That(pancakeToRemove != null, Is.True);

            result = order.RemovePancake(pancakeToRemove!.Id);
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error, Is.EqualTo(ErrorCode.CannotAddOrRemovePancakeInCurrentState));
        }

        [Test]
        public void AddPancake_OnConfirmedState_ReturnsFailure()
        {
            var order = Order.Create();
            order.AddPancake(Ingredient.Chocolate);

            Assert.That(order.Status, Is.EqualTo(OrderStatus.Draft));

            var result = order.Confirm();
            Assert.That(result.IsSuccess, Is.True);

            result = order.AddPancake(Ingredient.Honey);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error, Is.EqualTo(ErrorCode.CannotAddOrRemovePancakeInCurrentState));
        }
    }
}
