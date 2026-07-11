using PancakeOrdering.Core.Common.Results;
using PancakeOrdering.Core.Domain.Enums;
using PancakeOrdering.Core.Domain.Orders;

namespace PancakeOrdering.Core.Domain.States
{
    internal class PreparedState : StateBase
    {
        public static PreparedState Instance { get; } = new();

        private PreparedState()
        {
        }

        public override OrderStatus Status => OrderStatus.Prepared;

        public override bool CanChangeAddress => false;
        public override bool CanModifyPancakes => false;
        public override bool CanCancel => false;
        public override bool CanArchive => false;

        public override Result ValidateEntry(Order order)
            => Result.Success();
    }
}
