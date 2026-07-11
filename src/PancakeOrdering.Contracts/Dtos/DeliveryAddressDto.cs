namespace PancakeOrdering.Contracts.Dtos;

public sealed record DeliveryAddressDto(
    string City,
    string Street,
    string Country);