using ContractResults = PancakeOrdering.Contracts.Results;
using PancakeOrdering.Application.Orders.Snapshots;
using PancakeOrdering.Contracts.Dtos;
using PancakeOrdering.Core.Domain.Enums;

namespace PancakeOrdering.Application
{
    internal static class OrderDtoMapper
    {
        public static ContractResults.OperationResult<OrderDto> ToDto(OrderSnapshot snapshot)
        {
            var statusResult = MapStatus(snapshot.Status);
            if (!statusResult.IsSuccess)
                return ContractResults.OperationResult<OrderDto>.Failure(statusResult.Error!.Value);

            var pancakes = new List<PancakeDto>();
            foreach (var pancake in snapshot.Pancakes)
            {
                var pancakeResult = ToDto(pancake);
                if (!pancakeResult.IsSuccess)
                    return ContractResults.OperationResult<OrderDto>.Failure(pancakeResult.Error!.Value);

                pancakes.Add(pancakeResult.Value!);
            }

            return ContractResults.OperationResult<OrderDto>.Success(
                new OrderDto(
                    snapshot.OrderId,
                    statusResult.Value,
                    new DeliveryAddressDto(
                        snapshot.DeliveryAddress.Street,
                        snapshot.DeliveryAddress.City,
                        snapshot.DeliveryAddress.Country),
                    Array.AsReadOnly(pancakes.ToArray())));
        }

        public static ContractResults.OperationResult<Ingredient> ToDomainIngredient(IngredientTypeDto ingredient)
        {
            return ingredient switch
            {
                IngredientTypeDto.Honey => ContractResults.OperationResult<Ingredient>.Success(Ingredient.Honey),
                IngredientTypeDto.Jam => ContractResults.OperationResult<Ingredient>.Success(Ingredient.Jam),
                IngredientTypeDto.Chocolate => ContractResults.OperationResult<Ingredient>.Success(Ingredient.Chocolate),
                _ => ContractResults.OperationResult<Ingredient>.Failure(ContractResults.OperationErrorCode.InvalidIngredient)
            };
        }

        public static ContractResults.OperationResult<HashSet<Ingredient>> ToDomainIngredients(IReadOnlyCollection<IngredientTypeDto>? ingredients)
        {
            var result = new HashSet<Ingredient>();

            if (ingredients == null)
                return ContractResults.OperationResult<HashSet<Ingredient>>.Success(result);

            foreach (var ingredient in ingredients)
            {
                var domainIngredient = ToDomainIngredient(ingredient);
                if (!domainIngredient.IsSuccess)
                    return ContractResults.OperationResult<HashSet<Ingredient>>.Failure(domainIngredient.Error!.Value);

                result.Add(domainIngredient.Value);
            }

            return ContractResults.OperationResult<HashSet<Ingredient>>.Success(result);
        }

        private static ContractResults.OperationResult<PancakeDto> ToDto(PancakeSnapshot pancake)
        {
            var ingredients = new List<IngredientTypeDto>();
            foreach (var ingredient in pancake.Ingredients)
            {
                var ingredientResult = MapIngredient(ingredient);
                if (!ingredientResult.IsSuccess)
                    return ContractResults.OperationResult<PancakeDto>.Failure(ingredientResult.Error!.Value);

                ingredients.Add(ingredientResult.Value);
            }

            return ContractResults.OperationResult<PancakeDto>.Success(
                new PancakeDto(
                    pancake.PancakeId,
                    Array.AsReadOnly(ingredients.ToArray())));
        }

        private static ContractResults.OperationResult<IngredientTypeDto> MapIngredient(Ingredient ingredient)
        {
            return ingredient switch
            {
                Ingredient.Honey => ContractResults.OperationResult<IngredientTypeDto>.Success(IngredientTypeDto.Honey),
                Ingredient.Jam => ContractResults.OperationResult<IngredientTypeDto>.Success(IngredientTypeDto.Jam),
                Ingredient.Chocolate => ContractResults.OperationResult<IngredientTypeDto>.Success(IngredientTypeDto.Chocolate),
                _ => ContractResults.OperationResult<IngredientTypeDto>.Failure(ContractResults.OperationErrorCode.InternalError)
            };
        }

        private static ContractResults.OperationResult<OrderStatusDto> MapStatus(OrderStatus status)
        {
            return status switch
            {
                OrderStatus.Draft => ContractResults.OperationResult<OrderStatusDto>.Success(OrderStatusDto.Draft),
                OrderStatus.Confirmed => ContractResults.OperationResult<OrderStatusDto>.Success(OrderStatusDto.Confirmed),
                OrderStatus.Preparing => ContractResults.OperationResult<OrderStatusDto>.Success(OrderStatusDto.Preparing),
                OrderStatus.Prepared => ContractResults.OperationResult<OrderStatusDto>.Success(OrderStatusDto.Prepared),
                OrderStatus.OutForDelivery => ContractResults.OperationResult<OrderStatusDto>.Success(OrderStatusDto.OutForDelivery),
                OrderStatus.Delivered => ContractResults.OperationResult<OrderStatusDto>.Success(OrderStatusDto.Delivered),
                OrderStatus.Archived => ContractResults.OperationResult<OrderStatusDto>.Success(OrderStatusDto.Archived),
                OrderStatus.Cancelled => ContractResults.OperationResult<OrderStatusDto>.Success(OrderStatusDto.Cancelled),
                _ => ContractResults.OperationResult<OrderStatusDto>.Failure(ContractResults.OperationErrorCode.InternalError)
            };
        }
    }
}
