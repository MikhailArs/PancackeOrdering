using System;
using System.Collections.Generic;
using System.Text;

namespace PancakeOrdering.Contracts.Dtos
{
    public sealed record PancakeDto(
        int PancakeId,
        IReadOnlyList<IngredientDto> Ingredients);
}
