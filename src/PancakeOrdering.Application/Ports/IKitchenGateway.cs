using PancakeOrdering.Core.Common.Results;

namespace PancakeOrdering.Application.Ports
{
    public interface IKitchenGateway
    {
        Task<Result> AcceptOrderAsync(Guid orderId);
    }
}
