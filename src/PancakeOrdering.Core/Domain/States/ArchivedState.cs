using PancakeOrdering.Core.Domain.Enums;

namespace PancakeOrdering.Core.Domain.States
{
    internal sealed class ArchivedState : StateBase
    {
        public static ArchivedState Instance { get; } = new();

        private ArchivedState() { }

        public override OrderStatus Status => OrderStatus.Archived;
    }
}
