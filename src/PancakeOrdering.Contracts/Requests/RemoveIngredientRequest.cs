using PancakeOrdering.Contracts.Dtos;

namespace PancakeOrdering.Contracts.Requests;

public sealed record RemoveIngredientRequest(
    Guid OrderId,
    int PancakeId,
    IngredientTypeDto Ingredient);
