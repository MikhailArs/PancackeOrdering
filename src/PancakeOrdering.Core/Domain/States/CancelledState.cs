using PancakeOrdering.Core.Common.Results;
using PancakeOrdering.Core.Domain.Enums;
using PancakeOrdering.Core.Domain.Orders;

namespace PancakeOrdering.Core.Domain.States
{
    internal class CancelledState : StateBase
    {
        public static CancelledState Instance { get; } = new();

        private CancelledState()
        {
        }

        public override OrderStatus Status => OrderStatus.Cancelled;

        public override bool CanChangeAddress => false;
        public override bool CanModifyPancakes => false;
        public override bool CanCancel => false;
        public override bool CanArchive => false;


        public override Result ValidateEntry(Order order)
        {
            if (order.CurrentState.Status == OrderStatus.Draft || order.CurrentState.Status == OrderStatus.Confirmed)
                return Result.Success();
            else
            {
                // TODO[Mik]: Log to be added (order.CurrentState.Status)
                return Result.Failure(ErrorCode.InvalidTransition);
            }
        }
    }
}
