using PancakeOrdering.Core.Common.Results;
using PancakeOrdering.Core.Domain.Enums;
using PancakeOrdering.Core.Domain.Pancakes;
using PancakeOrdering.Core.Domain.States;

namespace PancakeOrdering.Core.Domain.Orders
{
    internal sealed class Order
    {
        private Order(Pancake[] pancakes)
        {
            CurrentState = DraftState.Instance;
            Pancakes = pancakes;
        }

        public IOrderState CurrentState { get; private set; }

        public OrderStatus Status => CurrentState.Status;

        public Pancake[] Pancakes { get; private set; }

        public static Order Create(params Pancake[]? pancakes)
        {
            pancakes = pancakes is null ? [] : pancakes.ToArray();
            return new Order(pancakes);
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
        public Result UpdatePancakes()
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
    }
}
