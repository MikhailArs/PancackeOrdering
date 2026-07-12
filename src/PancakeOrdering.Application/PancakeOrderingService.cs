using ContractResults = PancakeOrdering.Contracts.Results;
using CoreResults = PancakeOrdering.Core.Common.Results;
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
        private readonly IOrderQueryService _orderQueryService;

        public PancakeOrderingService(
            IKitchenGateway kitchenGateway,
            IIngredientAvailability ingredientAvailability,
            IDeliveryGateway deliveryGateway,
            IArchiveGateway archiveGateway)
            : this(CreateComposition(
                kitchenGateway,
                ingredientAvailability,
                deliveryGateway,
                archiveGateway))
        {
        }

        private PancakeOrderingService(ServiceComposition composition)
            : this(composition.ApplicationService, composition.OrderQueryService)
        {
        }

        internal PancakeOrderingService(OrderApplicationService applicationService)
            : this(applicationService, new OrderQueryService(applicationService.SnapshotStore))
        {
        }

        internal PancakeOrderingService(
            OrderApplicationService applicationService,
            IOrderQueryService orderQueryService)
        {
            _applicationService = applicationService;
            _orderQueryService = orderQueryService;
        }

        public ContractResults.OperationResult<OrderDto> CreateOrder(CreateOrderRequest request)
        {
            if (request == null)
                return ContractResults.OperationResult<OrderDto>.Failure(ContractResults.OperationErrorCode.InvalidRequest);

            var result = _applicationService.CreateOrder(ToDomainAddress(request.DeliveryAddress));
            return result.IsSuccess
                ? _orderQueryService.GetOrder(new GetOrderRequest(result.Value))
                : ContractResults.OperationResult<OrderDto>.Failure(OperationErrorMapper.ToOperationError(result.Error!.Value));
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
                : _orderQueryService.GetOrder(request);
        }

        private async Task<ContractResults.OperationResult<OrderDto>> ExecuteAndProjectAsync(Guid orderId, Task<CoreResults.Result> operation)
        {
            var result = await operation;
            return result.IsSuccess
                ? _orderQueryService.GetOrder(new GetOrderRequest(orderId))
                : ContractResults.OperationResult<OrderDto>.Failure(OperationErrorMapper.ToOperationError(result.Error!.Value));
        }

        private async Task<ContractResults.OperationResult<OrderDto>> ExecuteAndProjectAsync(Guid orderId, Task<CoreResults.Result<int>> operation)
        {
            var result = await operation;
            return result.IsSuccess
                ? _orderQueryService.GetOrder(new GetOrderRequest(orderId))
                : ContractResults.OperationResult<OrderDto>.Failure(OperationErrorMapper.ToOperationError(result.Error!.Value));
        }

        private static ServiceComposition CreateComposition(
            IKitchenGateway kitchenGateway,
            IIngredientAvailability ingredientAvailability,
            IDeliveryGateway deliveryGateway,
            IArchiveGateway archiveGateway)
        {
            var snapshotStore = new Orders.Snapshots.OrderSnapshotStore();
            var queryService = new OrderQueryService(snapshotStore);

            var applicationService = new OrderApplicationService(
                kitchenGateway,
                deliveryGateway,
                archiveGateway,
                ingredientAvailability,
                snapshotStore);

            return new ServiceComposition(applicationService, queryService);
        }

        private static DeliveryAddress ToDomainAddress(DeliveryAddressDto? address) =>
            new(address?.Street, address?.City, address?.Country);

        private sealed class ServiceComposition
        {
            public ServiceComposition(
                OrderApplicationService applicationService,
                IOrderQueryService orderQueryService)
            {
                ApplicationService = applicationService;
                OrderQueryService = orderQueryService;
            }

            public OrderApplicationService ApplicationService { get; }

            public IOrderQueryService OrderQueryService { get; }
        }
    }
}
