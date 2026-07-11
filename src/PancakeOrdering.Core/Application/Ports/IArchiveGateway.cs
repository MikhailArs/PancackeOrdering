using PancakeOrdering.Core.Common.Results;

namespace PancakeOrdering.Core.Application.Ports
{
    internal interface IArchiveGateway
    {
        Task<Result> ArchiveOrderAsync(Guid orderId);
    }
}
