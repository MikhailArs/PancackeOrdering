using PancakeOrdering.Core.Common.Results;
using PancakeOrdering.Core.Domain.Enums;
using PancakeOrdering.Core.Domain.Pancakes;
using PancakeOrdering.Core.Domain.States;

namespace PancakeOrdering.Core.Domain.Orders
{
    internal sealed class Order
    {
        private int _pancakesCounter;
        private readonly List<Pancake> _pancakes;

        private Order()
        {
            CurrentState = DraftState.Instance;
            _pancakes = new List<Pancake>();
        }

        public IOrderState CurrentState { get; private set; }

        public OrderStatus Status => CurrentState.Status;

        public Pancake[] Pancakes => _pancakes.ToArray();

        public static Order Create()
        {
            return new Order();
        }

        public Result<int> AddPancake(Ingredient ingredient) => AddPancake([ingredient]);

        public Result<int> AddPancake(HashSet<Ingredient>? ingredients)
        {
            if (!CurrentState.CanModifyPancakes)
                return Result.Failure(ErrorCode.CannotAddOrRemovePancakeInCurrentState);

            var pancake = ingredients == null ? new Pancake(GetNextPancakeId()) : new Pancake(GetNextPancakeId(), ingredients);

            _pancakes.Add(pancake);

            return Result.Success(pancake.Id);
        }
        
        public Result RemovePancake(int pancakeId)
        {
            if (!CurrentState.CanModifyPancakes)
                return Result.Failure(ErrorCode.CannotAddOrRemovePancakeInCurrentState);

            if (!_pancakes.Any())
                return Result.Failure(ErrorCode.NoPancakesToRemove);

            var pancakeToRemove = _pancakes.FirstOrDefault(a => a.Id == pancakeId);
            if (pancakeToRemove == null)
                return Result.Failure(ErrorCode.NoPancakeFound);

            _pancakes.Remove(pancakeToRemove);

            return Result.Success();
        }




        public Result Confirm()
        {
            return CurrentState.Status == OrderStatus.Draft
                ? TransitionTo(ConfirmedState.Instance)
                : Result.Failure(ErrorCode.InvalidTransition);
        }

        public Result UpdateAddress()
        {
            return Result.Success();
        }

        public Result UpdateIngredients()
        {
            return Result.Success();
        }
        public Result MarkPreparationStarted()
        {
            return Result.Success();
        }
        public Result MarkPrepared()
        {
            return Result.Success();
        }
        public Result MarkDelivered()
        {
            return Result.Success();
        }
        public Result MarkArchived()
        {
            return Result.Success();
        }
        public Result MarkConfirmed()
        {
            return Confirm();
        }
        public Result Cancel()
        {
            return Result.Success();
        }

        private Result TransitionTo(IOrderState nextState)
        {
            var entryResult = nextState.ValidateEntry(this);
            if (!entryResult.IsSuccess)
            {
                return entryResult;
            }

            CurrentState.OnExit(this);
            CurrentState = nextState;
            CurrentState.OnEnter(this);

            return Result.Success();
        }

        private int GetNextPancakeId() => ++_pancakesCounter;
    }
}
