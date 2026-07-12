using PancakeOrdering.Application.Orders.Snapshots;
using PancakeOrdering.Contracts.Dtos;
using PancakeOrdering.Contracts.Requests;
using PancakeOrdering.Contracts.Results;
using PancakeOrdering.Contracts.Services;

namespace PancakeOrdering.Application
{
    internal sealed class OrderQueryService : IOrderQueryService
    {
        private readonly OrderSnapshotStore _snapshotStore;

        public OrderQueryService(OrderSnapshotStore snapshotStore)
        {
            _snapshotStore = snapshotStore;
        }

        public OperationResult<OrderDto> GetOrder(GetOrderRequest request)
        {
            if (request == null)
                return OperationResult<OrderDto>.Failure(OperationErrorCode.InvalidRequest);

            var snapshotResult = _snapshotStore.GetSnapshot(request.OrderId);
            return snapshotResult.IsSuccess
                ? OrderDtoMapper.ToDto(snapshotResult.Value!)
                : OperationResult<OrderDto>.Failure(OperationErrorMapper.ToOperationError(snapshotResult.Error!.Value));
        }
    }
}
