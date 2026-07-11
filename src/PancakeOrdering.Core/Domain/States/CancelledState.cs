using PancakeOrdering.Core.Domain.Enums;

namespace PancakeOrdering.Core.Domain.States
{
    internal sealed class CancelledState : StateBase
    {
        public static CancelledState Instance { get; } = new();

        private CancelledState() { }

        public override OrderStatus Status => OrderStatus.Cancelled;
    }
}
