using PancakeOrdering.Contracts.Dtos;
using PancakeOrdering.Contracts.Requests;
using PancakeOrdering.Contracts.Results;

namespace PancakeOrdering.Contracts.Services;

public interface IPancakeOrderingService
{
    OperationResult<OrderDto> CreateOrder(CreateOrderRequest request);

    Task<OperationResult<OrderDto>> AddPancakeAsync(AddPancakeRequest request);

    Task<OperationResult<OrderDto>> RemovePancakeAsync(RemovePancakeRequest request);

    Task<OperationResult<OrderDto>> ChangeDeliveryAddressAsync(ChangeDeliveryAddressRequest request);

    Task<OperationResult<OrderDto>> AddIngredientAsync(AddIngredientRequest request);

    Task<OperationResult<OrderDto>> RemoveIngredientAsync(RemoveIngredientRequest request);

    Task<OperationResult<OrderDto>> ConfirmOrderAsync(ConfirmOrderRequest request);

    Task<OperationResult<OrderDto>> CancelOrderAsync(CancelOrderRequest request);

    OperationResult<OrderDto> GetOrder(GetOrderRequest request);
}
