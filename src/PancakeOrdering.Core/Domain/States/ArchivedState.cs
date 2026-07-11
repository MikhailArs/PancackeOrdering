using PancakeOrdering.Core.Common.Results;
using PancakeOrdering.Core.Domain.Enums;
using PancakeOrdering.Core.Domain.Orders;

namespace PancakeOrdering.Core.Domain.States
{
    internal class ArchivedState : StateBase
    {
        public static ArchivedState Instance { get; } = new();

        private ArchivedState()
        {
        }

        public override OrderStatus Status => OrderStatus.Archived;

        public override bool CanChangeAddress => false;
        public override bool CanModifyPancakes => false;
        public override bool CanCancel => false;
        public override bool CanArchive => false;

        public override Result ValidateEntry(Order order)
        {
            if (order.CurrentState.Status == OrderStatus.Delivered)
                return Result.Success();
            else
            {
                // TODO[Mik]: Log to be added (order.CurrentState.Status)
                return Result.Failure(ErrorCode.InvalidTransition);
            }
        }
    }
}
