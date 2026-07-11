namespace PancakeOrdering.Contracts.Dtos;

public sealed record DeliveryAddressDto(
    string Street,
    string City,
    string Country);
