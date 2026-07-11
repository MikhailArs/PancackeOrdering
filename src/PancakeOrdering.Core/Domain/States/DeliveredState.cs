using PancakeOrdering.Core.Common.Results;
using PancakeOrdering.Core.Domain.Enums;
using PancakeOrdering.Core.Domain.Orders;

namespace PancakeOrdering.Core.Domain.States
{
    internal class DeliveredState : StateBase
    {
        public static DeliveredState Instance { get; } = new();

        private DeliveredState()
        {
        }

        public override OrderStatus Status => OrderStatus.Delivered;

        public override bool CanChangeAddress => false;
        public override bool CanModifyPancakes => false;
        public override bool CanCancel => false;
        public override bool CanArchive => true;

        public override Result ValidateEntry(Order order)
            => Result.Success();
    }
}
