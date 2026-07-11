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
    }
}
