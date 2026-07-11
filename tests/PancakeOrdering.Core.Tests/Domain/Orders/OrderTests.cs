using PancakeOrdering.Core.Common.Results;
using PancakeOrdering.Core.Domain.Enums;
using PancakeOrdering.Core.Domain.Orders;
using PancakeOrdering.Core.Domain.Pancakes;

namespace PancakeOrdering.Core.Tests.Domain.Orders
{
    public sealed class OrderTests
    {
        [Test]
        public void Confirm_WithOnePancake_ChangesStatusToConfirmed()
        {
            var order = Order.Create(new Pancake());

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
    }
}
