using PancakeOrdering.Contracts.Dtos;

namespace PancakeOrdering.Contracts.Requests;

public sealed record ChangeDeliveryAddressRequest(
    Guid OrderId,
    DeliveryAddressDto DeliveryAddress);
