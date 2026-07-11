using PancakeOrdering.Core.Domain.Enums;

namespace PancakeOrdering.Core.Domain.States
{
    internal sealed class PreparingState : StateBase
    {
        public static PreparingState Instance { get; } = new();

        private PreparingState() { }

        public override OrderStatus Status => OrderStatus.Preparing;
    }
}
