using PancakeOrdering.Core.Domain.Enums;

namespace PancakeOrdering.Core.Domain.States
{
    internal sealed class DraftState : StateBase
    {
        public static DraftState Instance { get; } = new();

        private DraftState() { }

        public override OrderStatus Status => OrderStatus.Draft;

        public override bool CanChangeAddress => true;
        public override bool CanModifyPancakes => true;
    }
}
