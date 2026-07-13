using PancakeOrdering.Contracts.Dtos;
using PancakeOrdering.Contracts.Requests;
using PancakeOrdering.Contracts.Results;
using PancakeOrdering.Contracts.Services;
using PancakeOrdering.Core.Common.Results;
using PancakeOrdering.Core.Domain.Orders;

namespace PancakeOrdering.Application
{
    public sealed class PancakeOrderingService : IPancakeOrderingService
    {
        private readonly OrderApplicationService _applicationService;
        private readonly IOrderQueryService _orderQueryService;

        internal PancakeOrderingService(
            OrderApplicationService applicationService,
            IOrderQueryService orderQueryService)
        {
            _applicationService = applicationService;
            _orderQueryService = orderQueryService;
        }

        public OperationResult<OrderDto> CreateOrder(CreateOrderRequest request)
        {
            if (request == null)
                return OperationResult<OrderDto>.Failure(OperationErrorCode.InvalidRequest);

            var result = _applicationService.CreateOrder(ToDomainAddress(request.DeliveryAddress));
            return result.IsSuccess
                ? _orderQueryService.GetOrder(new GetOrderRequest(result.Value))
                : OperationResult<OrderDto>.Failure(OperationErrorMapper.ToOperationError(result.Error!.Value));
        }

        public async Task<OperationResult<OrderDto>> AddPancakeAsync(AddPancakeRequest request)
        {
            if (request == null)
                return OperationResult<OrderDto>.Failure(OperationErrorCode.InvalidRequest);

            var ingredientsResult = OrderDtoMapper.ToDomainIngredients(request.Ingredients);
            if (!ingredientsResult.IsSuccess)
                return OperationResult<OrderDto>.Failure(ingredientsResult.Error!.Value);

            return await ExecuteAndProjectAsync(
                request.OrderId,
                _applicationService.AddPancakeAsync(request.OrderId, ingredientsResult.Value!));
        }

        public async Task<OperationResult<OrderDto>> RemovePancakeAsync(RemovePancakeRequest request)
        {
            if (request == null)
                return OperationResult<OrderDto>.Failure(OperationErrorCode.InvalidRequest);

            return await ExecuteAndProjectAsync(
                request.OrderId,
                _applicationService.RemovePancakeAsync(request.OrderId, request.PancakeId));
        }

        public async Task<OperationResult<OrderDto>> ChangeDeliveryAddressAsync(ChangeDeliveryAddressRequest request)
        {
            if (request == null)
                return OperationResult<OrderDto>.Failure(OperationErrorCode.InvalidRequest);

            return await ExecuteAndProjectAsync(
                request.OrderId,
                _applicationService.ChangeDeliveryAddressAsync(
                    request.OrderId,
                    ToDomainAddress(request.DeliveryAddress)));
        }

        public async Task<OperationResult<OrderDto>> AddIngredientAsync(AddIngredientRequest request)
        {
            if (request == null)
                return OperationResult<OrderDto>.Failure(OperationErrorCode.InvalidRequest);

            var ingredientResult = OrderDtoMapper.ToDomainIngredient(request.Ingredient);
            if (!ingredientResult.IsSuccess)
                return OperationResult<OrderDto>.Failure(ingredientResult.Error!.Value);

            return await ExecuteAndProjectAsync(
                request.OrderId,
                _applicationService.AddIngredientAsync(
                    request.OrderId,
                    request.PancakeId,
                    ingredientResult.Value));
        }

        public async Task<OperationResult<OrderDto>> RemoveIngredientAsync(RemoveIngredientRequest request)
        {
            if (request == null)
                return OperationResult<OrderDto>.Failure(OperationErrorCode.InvalidRequest);

            var ingredientResult = OrderDtoMapper.ToDomainIngredient(request.Ingredient);
            if (!ingredientResult.IsSuccess)
                return OperationResult<OrderDto>.Failure(ingredientResult.Error!.Value);

            return await ExecuteAndProjectAsync(
                request.OrderId,
                _applicationService.RemoveIngredientAsync(
                    request.OrderId,
                    request.PancakeId,
                    ingredientResult.Value));
        }

        public async Task<OperationResult<OrderDto>> ConfirmOrderAsync(ConfirmOrderRequest request)
        {
            if (request == null)
                return OperationResult<OrderDto>.Failure(OperationErrorCode.InvalidRequest);

            return await ExecuteAndProjectAsync(
                request.OrderId,
                _applicationService.ConfirmAsync(request.OrderId));
        }

        public async Task<OperationResult<OrderDto>> StartPreparationAsync(Guid orderId) =>
            await ExecuteAndProjectAsync(
                orderId,
                _applicationService.StartPreparationAsync(orderId));

        public async Task<OperationResult<OrderDto>> CompletePreparationAsync(Guid orderId) =>
            await ExecuteAndProjectAsync(
                orderId,
                _applicationService.CompletePreparationAsync(orderId));

        public async Task<OperationResult<OrderDto>> StartDeliveryAsync(Guid orderId) =>
            await ExecuteAndProjectAsync(
                orderId,
                _applicationService.StartDeliveryAsync(orderId));

        public async Task<OperationResult<OrderDto>> CompleteDeliveryAsync(Guid orderId) =>
            await ExecuteAndProjectAsync(
                orderId,
                _applicationService.CompleteDeliveryAsync(orderId));

        public async Task<OperationResult<OrderDto>> ArchiveAsync(Guid orderId) =>
            await ExecuteAndProjectAsync(
                orderId,
                _applicationService.ArchiveAsync(orderId));

        public async Task<OperationResult<OrderDto>> CancelOrderAsync(CancelOrderRequest request)
        {
            if (request == null)
                return OperationResult<OrderDto>.Failure(OperationErrorCode.InvalidRequest);

            return await ExecuteAndProjectAsync(
                request.OrderId,
                _applicationService.CancelAsync(request.OrderId));
        }

        public OperationResult<OrderDto> GetOrder(GetOrderRequest request)
        {
            return request == null
                ? OperationResult<OrderDto>.Failure(OperationErrorCode.InvalidRequest)
                : _orderQueryService.GetOrder(request);
        }

        private async Task<OperationResult<OrderDto>> ExecuteAndProjectAsync(Guid orderId, Task<Result> operation)
        {
            var result = await operation;
            return result.IsSuccess
                ? _orderQueryService.GetOrder(new GetOrderRequest(orderId))
                : OperationResult<OrderDto>.Failure(OperationErrorMapper.ToOperationError(result.Error!.Value));
        }

        private async Task<OperationResult<OrderDto>> ExecuteAndProjectAsync(Guid orderId, Task<Result<int>> operation)
        {
            var result = await operation;
            return result.IsSuccess
                ? _orderQueryService.GetOrder(new GetOrderRequest(orderId))
                : OperationResult<OrderDto>.Failure(OperationErrorMapper.ToOperationError(result.Error!.Value));
        }

        private static DeliveryAddress ToDomainAddress(DeliveryAddressDto? address) =>
            new(address?.Street, address?.City, address?.Country);
    }
}
