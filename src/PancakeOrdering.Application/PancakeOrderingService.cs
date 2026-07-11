using ContractResults = PancakeOrdering.Contracts.Results;
using CoreResults = PancakeOrdering.Core.Common.Results;
using PancakeOrdering.Application.Orders.Snapshots;
using PancakeOrdering.Application.Ports;
using PancakeOrdering.Contracts.Dtos;
using PancakeOrdering.Contracts.Requests;
using PancakeOrdering.Contracts.Services;
using PancakeOrdering.Core.Domain.Orders;

namespace PancakeOrdering.Application
{
    public sealed class PancakeOrderingService : IPancakeOrderingService
    {
        private readonly OrderApplicationService _applicationService;

        public PancakeOrderingService(
            IKitchenGateway kitchenGateway,
            IDeliveryGateway deliveryGateway,
            IArchiveGateway archiveGateway)
            : this(new OrderApplicationService(kitchenGateway, deliveryGateway, archiveGateway))
        {
        }

        internal PancakeOrderingService(OrderApplicationService applicationService)
        {
            _applicationService = applicationService;
        }

        public ContractResults.OperationResult<OrderDto> CreateOrder(CreateOrderRequest request)
        {
            if (request == null)
                return ContractResults.OperationResult<OrderDto>.Failure(ContractResults.OperationErrorCode.InvalidRequest);

            var result = _applicationService.CreateOrder(ToDomainAddress(request.DeliveryAddress));
            return result.IsSuccess
                ? MapResult(_applicationService.GetOrderSnapshot(result.Value))
                : ContractResults.OperationResult<OrderDto>.Failure(MapError(result.Error!.Value));
        }

        public async Task<ContractResults.OperationResult<OrderDto>> AddPancakeAsync(AddPancakeRequest request)
        {
            if (request == null)
                return ContractResults.OperationResult<OrderDto>.Failure(ContractResults.OperationErrorCode.InvalidRequest);

            var ingredientsResult = OrderDtoMapper.ToDomainIngredients(request.Ingredients);
            if (!ingredientsResult.IsSuccess)
                return ContractResults.OperationResult<OrderDto>.Failure(ingredientsResult.Error!.Value);

            return await ExecuteAndProjectAsync(
                request.OrderId,
                _applicationService.AddPancakeAsync(request.OrderId, ingredientsResult.Value!));
        }

        public async Task<ContractResults.OperationResult<OrderDto>> RemovePancakeAsync(RemovePancakeRequest request)
        {
            if (request == null)
                return ContractResults.OperationResult<OrderDto>.Failure(ContractResults.OperationErrorCode.InvalidRequest);

            return await ExecuteAndProjectAsync(
                request.OrderId,
                _applicationService.RemovePancakeAsync(request.OrderId, request.PancakeId));
        }

        public async Task<ContractResults.OperationResult<OrderDto>> ChangeDeliveryAddressAsync(ChangeDeliveryAddressRequest request)
        {
            if (request == null)
                return ContractResults.OperationResult<OrderDto>.Failure(ContractResults.OperationErrorCode.InvalidRequest);

            return await ExecuteAndProjectAsync(
                request.OrderId,
                _applicationService.ChangeDeliveryAddressAsync(
                    request.OrderId,
                    ToDomainAddress(request.DeliveryAddress)));
        }

        public async Task<ContractResults.OperationResult<OrderDto>> AddIngredientAsync(AddIngredientRequest request)
        {
            if (request == null)
                return ContractResults.OperationResult<OrderDto>.Failure(ContractResults.OperationErrorCode.InvalidRequest);

            var ingredientResult = OrderDtoMapper.ToDomainIngredient(request.Ingredient);
            if (!ingredientResult.IsSuccess)
                return ContractResults.OperationResult<OrderDto>.Failure(ingredientResult.Error!.Value);

            return await ExecuteAndProjectAsync(
                request.OrderId,
                _applicationService.AddIngredientAsync(
                    request.OrderId,
                    request.PancakeId,
                    ingredientResult.Value));
        }

        public async Task<ContractResults.OperationResult<OrderDto>> RemoveIngredientAsync(RemoveIngredientRequest request)
        {
            if (request == null)
                return ContractResults.OperationResult<OrderDto>.Failure(ContractResults.OperationErrorCode.InvalidRequest);

            var ingredientResult = OrderDtoMapper.ToDomainIngredient(request.Ingredient);
            if (!ingredientResult.IsSuccess)
                return ContractResults.OperationResult<OrderDto>.Failure(ingredientResult.Error!.Value);

            return await ExecuteAndProjectAsync(
                request.OrderId,
                _applicationService.RemoveIngredientAsync(
                    request.OrderId,
                    request.PancakeId,
                    ingredientResult.Value));
        }

        public async Task<ContractResults.OperationResult<OrderDto>> ConfirmOrderAsync(ConfirmOrderRequest request)
        {
            if (request == null)
                return ContractResults.OperationResult<OrderDto>.Failure(ContractResults.OperationErrorCode.InvalidRequest);

            return await ExecuteAndProjectAsync(
                request.OrderId,
                _applicationService.ConfirmAsync(request.OrderId));
        }

        public async Task<ContractResults.OperationResult<OrderDto>> CancelOrderAsync(CancelOrderRequest request)
        {
            if (request == null)
                return ContractResults.OperationResult<OrderDto>.Failure(ContractResults.OperationErrorCode.InvalidRequest);

            return await ExecuteAndProjectAsync(
                request.OrderId,
                _applicationService.CancelAsync(request.OrderId));
        }

        public ContractResults.OperationResult<OrderDto> GetOrder(GetOrderRequest request)
        {
            return request == null
                ? ContractResults.OperationResult<OrderDto>.Failure(ContractResults.OperationErrorCode.InvalidRequest)
                : MapResult(_applicationService.GetOrderSnapshot(request.OrderId));
        }

        private async Task<ContractResults.OperationResult<OrderDto>> ExecuteAndProjectAsync(Guid orderId, Task<CoreResults.Result> operation)
        {
            var result = await operation;
            return result.IsSuccess
                ? MapResult(_applicationService.GetOrderSnapshot(orderId))
                : ContractResults.OperationResult<OrderDto>.Failure(MapError(result.Error!.Value));
        }

        private async Task<ContractResults.OperationResult<OrderDto>> ExecuteAndProjectAsync(Guid orderId, Task<CoreResults.Result<int>> operation)
        {
            var result = await operation;
            return result.IsSuccess
                ? MapResult(_applicationService.GetOrderSnapshot(orderId))
                : ContractResults.OperationResult<OrderDto>.Failure(MapError(result.Error!.Value));
        }

        private static ContractResults.OperationResult<OrderDto> MapResult(CoreResults.Result<OrderSnapshot> result)
        {
            return result.IsSuccess
                ? OrderDtoMapper.ToDto(result.Value!)
                : ContractResults.OperationResult<OrderDto>.Failure(MapError(result.Error!.Value));
        }

        private static ContractResults.OperationErrorCode MapError(CoreResults.ErrorCode error)
        {
            return error switch
            {
                CoreResults.ErrorCode.InternalError => ContractResults.OperationErrorCode.InternalError,
                CoreResults.ErrorCode.InvalidTransition => ContractResults.OperationErrorCode.InvalidTransition,
                CoreResults.ErrorCode.OrderMustContainPancake => ContractResults.OperationErrorCode.OrderMustContainPancake,
                CoreResults.ErrorCode.NoPancakesToRemove => ContractResults.OperationErrorCode.NoPancakesToRemove,
                CoreResults.ErrorCode.PancakeNotFound => ContractResults.OperationErrorCode.PancakeNotFound,
                CoreResults.ErrorCode.CannotAddOrRemovePancakeInCurrentState => ContractResults.OperationErrorCode.CannotAddOrRemovePancakeInCurrentState,
                CoreResults.ErrorCode.DuplicateIngredientAdded => ContractResults.OperationErrorCode.DuplicateIngredientAdded,
                CoreResults.ErrorCode.IngredientNotFound => ContractResults.OperationErrorCode.IngredientNotFound,
                CoreResults.ErrorCode.InvalidDeliveryAddress => ContractResults.OperationErrorCode.InvalidDeliveryAddress,
                CoreResults.ErrorCode.CannotChangeAddressInCurrentState => ContractResults.OperationErrorCode.CannotChangeAddressInCurrentState,
                CoreResults.ErrorCode.OrderNotFound => ContractResults.OperationErrorCode.OrderNotFound,
                CoreResults.ErrorCode.KitchenDeclined => ContractResults.OperationErrorCode.KitchenDeclined,
                _ => ContractResults.OperationErrorCode.InternalError
            };
        }

        private static DeliveryAddress ToDomainAddress(DeliveryAddressDto? address) =>
            new(address?.Street, address?.City, address?.Country);
    }
}
