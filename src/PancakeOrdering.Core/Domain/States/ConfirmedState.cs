using PancakeOrdering.Core.Common.Results;
using PancakeOrdering.Core.Domain.Enums;
using PancakeOrdering.Core.Domain.Orders;

namespace PancakeOrdering.Core.Domain.States
{
    internal class ConfirmedState : StateBase
    {
        public static ConfirmedState Instance { get; } = new();

        private ConfirmedState()
        {
        }

        public override OrderStatus Status => OrderStatus.Confirmed;

        public override bool CanChangeAddress => false;
        public override bool CanModifyPancakes => false;
        public override bool CanCancel => true;
        public override bool CanArchive => false;

        public override Result ValidateEntry(Order order)
        {
            if (order.Pancakes.Length > 0)
            {
                return Result.Success();
            }
            else
            {
                // TODO[Mik]: Log to be added (order.Pancakes.Length)
                return Result.Failure(ErrorCode.OrderMustContainPancake);
            }
        }
    }
}
