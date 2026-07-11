using PancakeOrdering.Core.Common.Results;
using PancakeOrdering.Core.Domain.States;

namespace PancakeOrdering.Core.Domain.Orders
{
    internal class Order
    {
        public IOrderState CurrentState { get; private set; }
        public Pancake[] Pancakes { get; private set; }


        public Result Confirm()
        {
            return Result.Success();
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
            throw new NotImplementedException();
        }

        public Result Cancel()
        {
            return Result.Success();
        }


    }
}
