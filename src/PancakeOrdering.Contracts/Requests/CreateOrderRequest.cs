using PancakeOrdering.Contracts.Dtos;

namespace PancakeOrdering.Contracts.Requests;

public sealed record CreateOrderRequest(DeliveryAddressDto DeliveryAddress);
