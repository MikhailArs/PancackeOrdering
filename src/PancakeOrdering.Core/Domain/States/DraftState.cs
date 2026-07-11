using PancakeOrdering.Core.Common.Results;
using PancakeOrdering.Core.Domain.Enums;
using PancakeOrdering.Core.Domain.Orders;

namespace PancakeOrdering.Core.Domain.States
{
    internal class DraftState : StateBase
    {
        public static DraftState Instance { get; } = new();

        private DraftState()
        {
        }

        public override OrderStatus Status => OrderStatus.Draft;

        public override bool CanChangeAddress => true;
        public override bool CanModifyPancakes => true;
        public override bool CanCancel => true;
        public override bool CanArchive => false;

        public override Result ValidateEntry(Order order)
            => Result.Success();
    }
}
