using PancakeOrdering.Core.Common.Results;

namespace PancakeOrdering.Application.Ports
{
    public interface IDeliveryGateway
    {
        Task<Result> SubmitOrderAsync(Guid orderId);
    }
}
