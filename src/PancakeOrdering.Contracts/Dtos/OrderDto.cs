namespace PancakeOrdering.Contracts.Dtos;

public sealed record OrderDto(
    Guid OrderId,
    string Status,
    DeliveryAddressDto Address,
    IReadOnlyList<PancakeDto> Pancakes);