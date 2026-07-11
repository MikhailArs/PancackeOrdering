using PancakeOrdering.Core.Common.Results;
using PancakeOrdering.Core.Domain.Enums;
using PancakeOrdering.Core.Domain.Orders;

namespace PancakeOrdering.Core.Domain.States
{
    internal interface IOrderState
    {
        OrderStatus Status { get; }

        bool CanChangeAddress { get; }
        bool CanModifyPancakes { get; }

        Result ValidateEntry(Order order);

        void OnExit(Order order);

        void OnEnter(Order order);
    }
}
