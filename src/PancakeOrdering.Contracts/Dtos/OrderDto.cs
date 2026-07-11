namespace PancakeOrdering.Contracts.Dtos;

public sealed record OrderDto(
    Guid OrderId,
    OrderStatusDto Status,
    DeliveryAddressDto DeliveryAddress,
    IReadOnlyList<PancakeDto> Pancakes);
