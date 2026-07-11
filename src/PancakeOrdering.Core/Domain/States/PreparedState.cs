using PancakeOrdering.Core.Domain.Enums;

namespace PancakeOrdering.Core.Domain.States
{
    internal sealed class PreparedState : StateBase
    {
        public static PreparedState Instance { get; } = new();

        private PreparedState() { }

        public override OrderStatus Status => OrderStatus.Prepared;
    }
}
