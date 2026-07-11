using PancakeOrdering.Core.Domain.Enums;

namespace PancakeOrdering.Core.Domain.States
{
    internal sealed class DeliveringState : StateBase
    {
        public static DeliveringState Instance { get; } = new();

        private DeliveringState() { }

        public override OrderStatus Status => OrderStatus.OutForDelivery;
    }
}
