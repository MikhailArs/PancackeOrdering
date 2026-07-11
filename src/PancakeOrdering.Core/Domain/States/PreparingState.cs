using PancakeOrdering.Core.Common.Results;
using PancakeOrdering.Core.Domain.Enums;
using PancakeOrdering.Core.Domain.Orders;

namespace PancakeOrdering.Core.Domain.States
{
    internal class PreparingState : StateBase
    {
        public static PreparingState Instance { get; } = new();

        private PreparingState()
        {
        }

        public override OrderStatus Status => OrderStatus.Preparing;

        public override bool CanChangeAddress => false;
        public override bool CanModifyPancakes => false;
        public override bool CanCancel => false;
        public override bool CanArchive => false;

        public override Result ValidateEntry(Order order)
            => Result.Success();
    }
}
