using PancakeOrdering.Core.Domain.Enums;

namespace PancakeOrdering.Application.Orders.Snapshots;

internal sealed record PancakeSnapshot(
    int PancakeId,
    IReadOnlyList<Ingredient> Ingredients);
