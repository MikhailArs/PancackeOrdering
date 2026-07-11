using PancakeOrdering.Core.Domain.Orders;
using PancakeOrdering.Core.Domain.Pancakes;

namespace PancakeOrdering.Application.Orders.Snapshots
{
    internal static class OrderSnapshotFactory
    {
        public static OrderSnapshot Create(Order order)
        {
            var pancakes = order.Pancakes
                .Select(CreatePancakeSnapshot)
                .ToArray();

            return new OrderSnapshot(
                order.OrderId,
                order.Status,
                new DeliveryAddressSnapshot(
                    order.DeliveryAddress.Street,
                    order.DeliveryAddress.City,
                    order.DeliveryAddress.Country),
                Array.AsReadOnly(pancakes));
        }

        private static PancakeSnapshot CreatePancakeSnapshot(Pancake pancake)
        {
            var ingredients = pancake.Ingredients.ToArray();

            return new PancakeSnapshot(
                pancake.Id,
                Array.AsReadOnly(ingredients));
        }
    }
}
