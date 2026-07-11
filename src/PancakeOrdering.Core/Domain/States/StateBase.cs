using PancakeOrdering.Core.Common.Results;
using PancakeOrdering.Core.Domain.Enums;
using PancakeOrdering.Core.Domain.Orders;

namespace PancakeOrdering.Core.Domain.States
{
    internal abstract class StateBase : IOrderState
    {
        public abstract OrderStatus Status { get; }

        public virtual bool CanChangeAddress => false;
        public virtual bool CanModifyPancakes => false;

        public virtual Result ValidateEntry(Order order) => Result.Success();

        public virtual void OnExit(Order order)
        {
        }

        public virtual void OnEnter(Order order)
        {
        }
    }
}
