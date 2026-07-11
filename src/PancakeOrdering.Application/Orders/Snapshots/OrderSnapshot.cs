using PancakeOrdering.Core.Domain.Enums;

namespace PancakeOrdering.Application.Orders.Snapshots;

internal sealed record OrderSnapshot(
    Guid OrderId,
    OrderStatus Status,
    DeliveryAddressSnapshot DeliveryAddress,
    IReadOnlyList<PancakeSnapshot> Pancakes);
