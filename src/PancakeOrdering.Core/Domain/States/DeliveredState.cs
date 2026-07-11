using PancakeOrdering.Core.Domain.Enums;

namespace PancakeOrdering.Core.Domain.States
{
    internal class DeliveredState : StateBase
    {
        public static DeliveredState Instance { get; } = new();

        private DeliveredState() { }

        public override OrderStatus Status => OrderStatus.Delivered;
    }
}
