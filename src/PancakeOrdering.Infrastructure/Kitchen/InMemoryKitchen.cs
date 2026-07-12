using PancakeOrdering.Application;
using PancakeOrdering.Application.Ports;
using PancakeOrdering.Contracts.Dtos;
using PancakeOrdering.Contracts.Requests;
using PancakeOrdering.Contracts.Services;
using PancakeOrdering.Core.Common.Results;

namespace PancakeOrdering.Infrastructure.Kitchen
{
    internal sealed class InMemoryKitchen :
        IKitchenGateway,
        IIngredientAvailability
    {
        private readonly IOrderQueryService _orderQueryService;
        private readonly Dictionary<IngredientTypeDto, int> _stock;
        private readonly Lock _stockLock = new();

        public InMemoryKitchen(
            IOrderQueryService orderQueryService,
            IReadOnlyDictionary<IngredientTypeDto, int> initialStock)
        {
            _orderQueryService = orderQueryService;
            _stock = new Dictionary<IngredientTypeDto, int>(initialStock);
        }

        public Task<Result> AcceptOrderAsync(Guid orderId)
        {
            var orderResult = _orderQueryService.GetOrder(new GetOrderRequest(orderId));
            if (!orderResult.IsSuccess)
                return Task.FromResult(Result.Failure(OperationErrorMapper.ToCoreError(orderResult.Error!.Value)));

            var requiredIngredients = CountRequiredIngredients(orderResult.Value!);
            var result = TryConsumeStock(requiredIngredients)
                ? Result.Success()
                : Result.Failure(ErrorCode.KitchenDeclined);

            return Task.FromResult(result);
        }

        public Task<Result> CheckAvailabilityAsync(IReadOnlyCollection<IngredientTypeDto> ingredients)
        {
            lock (_stockLock)
            {
                foreach (var ingredient in ingredients)
                {
                    if (GetAvailableQuantityUnsafe(ingredient) <= 0)
                        return Task.FromResult(Result.Failure(ErrorCode.IngredientUnavailable));
                }
            }

            return Task.FromResult(Result.Success());
        }

        internal int GetAvailableQuantity(IngredientTypeDto ingredient)
        {
            lock (_stockLock)
            {
                return GetAvailableQuantityUnsafe(ingredient);
            }
        }

        private bool TryConsumeStock(Dictionary<IngredientTypeDto, int> requiredIngredients)
        {
            lock (_stockLock)
            {
                foreach (var requirement in requiredIngredients)
                {
                    if (GetAvailableQuantityUnsafe(requirement.Key) < requirement.Value)
                        return false;
                }

                foreach (var requirement in requiredIngredients)
                {
                    _stock[requirement.Key] = GetAvailableQuantityUnsafe(requirement.Key) - requirement.Value;
                }

                return true;
            }
        }

        private int GetAvailableQuantityUnsafe(IngredientTypeDto ingredient) =>
            _stock.GetValueOrDefault(ingredient, 0);

        private static Dictionary<IngredientTypeDto, int> CountRequiredIngredients(OrderDto order)
        {
            var requiredIngredients = new Dictionary<IngredientTypeDto, int>();

            foreach (var pancake in order.Pancakes)
            {
                foreach (var ingredient in pancake.Ingredients)
                {
                    requiredIngredients.TryGetValue(ingredient, out var count);
                    requiredIngredients[ingredient] = count + 1;
                }
            }

            return requiredIngredients;
        }
    }
}
