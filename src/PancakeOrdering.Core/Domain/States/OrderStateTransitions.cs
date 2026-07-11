using PancakeOrdering.Core.Domain.Enums;

namespace PancakeOrdering.Core.Domain.States
{
    internal static class OrderStateTransitions
    {
        private static readonly IReadOnlyDictionary<
            (OrderStatus Status, OrderAction Action),
            IOrderState> Transitions =
            new Dictionary<(OrderStatus, OrderAction), IOrderState>
            {
                [(OrderStatus.Draft, OrderAction.Confirm)]                   = ConfirmedState.Instance,
                [(OrderStatus.Draft, OrderAction.Cancel)]                    = CancelledState.Instance,
                [(OrderStatus.Confirmed, OrderAction.Cancel)]                = CancelledState.Instance,
                [(OrderStatus.Confirmed, OrderAction.StartPreparation)]      = PreparingState.Instance,
                [(OrderStatus.Preparing, OrderAction.CompletePreparation)]   = PreparedState.Instance,
                [(OrderStatus.Prepared, OrderAction.StartDelivery)]          = DeliveringState.Instance,
                [(OrderStatus.OutForDelivery, OrderAction.CompleteDelivery)] = DeliveredState.Instance,
                [(OrderStatus.Delivered, OrderAction.Archive)]               = ArchivedState.Instance
            };

        public static bool TryResolve(
            OrderStatus currentStatus,
            OrderAction action,
            out IOrderState nextState) => Transitions.TryGetValue((currentStatus, action), out nextState!);
    }
}
