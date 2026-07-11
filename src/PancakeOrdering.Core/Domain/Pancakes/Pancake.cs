using PancakeOrdering.Core.Common.Results;
using PancakeOrdering.Core.Domain.Enums;

namespace PancakeOrdering.Core.Domain.Pancakes
{
    internal class Pancake
    {
        private readonly List<Ingredient> _ingredients;

        public int Id { get; private set; }
        public Ingredient[] Ingredients => _ingredients.ToArray();


        public Pancake(int id) : this(id, null) { }

        public Pancake(int id, HashSet<Ingredient>? ingredients)
        {
            Id = id;
            _ingredients = ingredients?.ToList() ?? [];
        }

        internal Result AddIngredient(Ingredient ingredient)
        {
            if (_ingredients.Contains(ingredient))
                return Result.Failure(ErrorCode.DuplicateIngredientAdded);

            _ingredients.Add(ingredient);
            return Result.Success();
        }

        internal Result RemoveIngredient(Ingredient ingredient)
        {
            if (!_ingredients.Contains(ingredient))
                return Result.Failure(ErrorCode.IngredientNotFound);

            _ingredients.Remove(ingredient);
            return Result.Success();
        }
    }
}
