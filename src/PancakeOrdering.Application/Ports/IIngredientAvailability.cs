using PancakeOrdering.Contracts.Dtos;
using PancakeOrdering.Core.Common.Results;

namespace PancakeOrdering.Application.Ports
{
    public interface IIngredientAvailability
    {
        Task<Result> CheckAvailabilityAsync(IReadOnlyCollection<IngredientTypeDto> ingredients);
    }
}
