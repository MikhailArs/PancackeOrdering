using PancakeOrdering.Core.Common.Results;
using PancakeOrdering.Core.Domain.Enums;
using PancakeOrdering.Core.Domain.Orders;

namespace PancakeOrdering.Core.Domain.States
{
    internal abstract class StateBase : IOrderState
    {
        public virtual OrderStatus Status { get; }
        public virtual bool CanChangeAddress { get; }
        public virtual bool CanModifyPancakes { get; }
        public virtual bool CanCancel { get; }
        public virtual bool CanArchive { get; }

        public abstract Result ValidateEntry(Order order);

        public virtual void OnExit(Order order)
        {
        }

        public virtual void OnEnter(Order order)
        {
        }
    }
}
