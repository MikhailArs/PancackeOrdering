using System.Collections.Concurrent;
using PancakeOrdering.Core.Common.Results;
using PancakeOrdering.Core.Domain.Orders;

namespace PancakeOrdering.Application.Orders.Snapshots
{
    internal sealed class OrderSnapshotStore
    {
        private readonly ConcurrentDictionary<Guid, OrderSnapshot> _snapshots = new();

        public void Publish(Guid orderId, Order order)
        {
            _snapshots[orderId] = OrderSnapshotFactory.Create(order);
        }

        public Result<OrderSnapshot> GetSnapshot(Guid orderId)
        {
            return _snapshots.TryGetValue(orderId, out var snapshot)
                ? Result.Success(snapshot)
                : Result.Failure<OrderSnapshot>(ErrorCode.OrderNotFound);
        }
    }
}
