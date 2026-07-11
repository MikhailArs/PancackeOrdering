using PancakeOrdering.Core.Common.Results;

namespace PancakeOrdering.Core.Application.Ports
{
    internal interface IDeliveryGateway
    {
        Task<Result> SubmitOrderAsync(Guid orderId);
    }
}
