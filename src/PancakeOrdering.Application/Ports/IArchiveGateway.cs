using PancakeOrdering.Core.Common.Results;

namespace PancakeOrdering.Application.Ports
{
    public interface IArchiveGateway
    {
        Task<Result> ArchiveOrderAsync(Guid orderId);
    }
}
