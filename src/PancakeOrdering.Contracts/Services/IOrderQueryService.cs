using PancakeOrdering.Contracts.Dtos;
using PancakeOrdering.Contracts.Requests;
using PancakeOrdering.Contracts.Results;

namespace PancakeOrdering.Contracts.Services;

public interface IOrderQueryService
{
    OperationResult<OrderDto> GetOrder(GetOrderRequest request);
}
