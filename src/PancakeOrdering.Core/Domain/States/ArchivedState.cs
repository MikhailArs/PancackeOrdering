using PancakeOrdering.Core.Common.Results;
using PancakeOrdering.Core.Domain.Enums;
using PancakeOrdering.Core.Domain.Orders;

namespace PancakeOrdering.Core.Domain.States
{
    internal class ArchivedState : StateBase
    {
        public static ArchivedState Instance { get; } = new();

        private ArchivedState() { }

        public override OrderStatus Status => OrderStatus.Archived;

        public override bool CanChangeAddress => false;
        public override bool CanModifyPancakes => false;

        public override Result ValidateEntry(Order order) => Result.Success();
    }
}
