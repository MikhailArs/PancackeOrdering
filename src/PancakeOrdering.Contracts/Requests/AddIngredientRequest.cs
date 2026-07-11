using PancakeOrdering.Contracts.Dtos;

namespace PancakeOrdering.Contracts.Requests;

public sealed record AddIngredientRequest(
    Guid OrderId,
    int PancakeId,
    IngredientTypeDto Ingredient);
