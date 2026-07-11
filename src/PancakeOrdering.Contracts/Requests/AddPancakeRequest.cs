using PancakeOrdering.Contracts.Dtos;

namespace PancakeOrdering.Contracts.Requests;

public sealed record AddPancakeRequest(
    Guid OrderId,
    IReadOnlyCollection<IngredientTypeDto> Ingredients);
