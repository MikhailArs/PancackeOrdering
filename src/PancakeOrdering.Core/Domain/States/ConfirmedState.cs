using PancakeOrdering.Core.Common.Results;
using PancakeOrdering.Core.Domain.Enums;
using PancakeOrdering.Core.Domain.Orders;

namespace PancakeOrdering.Core.Domain.States
{
    internal class ConfirmedState : StateBase
    {
        public static ConfirmedState Instance { get; } = new();

        private ConfirmedState() { }

        public override OrderStatus Status => OrderStatus.Confirmed;

        public override bool CanChangeAddress => false;
        public override bool CanModifyPancakes => false;

        public override Result ValidateEntry(Order order) =>
            order.PancakeCount > 0 
                ? Result.Success()
                : Result.Failure(ErrorCode.OrderMustContainPancake);
        
    }
}
