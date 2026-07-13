using PancakeOrdering.Application.Ports;
using PancakeOrdering.Core.Common.Results;
using System.Collections.Concurrent;

namespace PancakeOrdering.Infrastructure.Delivery
{
    public sealed class InMemoryDelivery : IDeliveryGateway
    {
        private readonly ConcurrentQueue<Guid> _orderIds = new();

        public IReadOnlyCollection<Guid> OrderIds => _orderIds.ToArray();

        public Task<Result> SubmitOrderAsync(Guid orderId)
        {
            _orderIds.Enqueue(orderId);
            return Task.FromResult(Result.Success());
        }
    }
}
