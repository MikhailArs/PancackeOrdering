using PancakeOrdering.Core.Common.Results;

namespace PancakeOrdering.Core.Application.Ports
{
    internal interface IKitchenGateway
    {
        Task<Result> AcceptOrderAsync(Guid orderId);
    }
}
