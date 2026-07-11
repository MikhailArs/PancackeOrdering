namespace PancakeOrdering.Contracts.Dtos;

public sealed record PancakeDto(
    int PancakeId,
    IReadOnlyList<IngredientTypeDto> Ingredients);
