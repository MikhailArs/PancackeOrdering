namespace PancakeOrdering.Application.Orders.Snapshots;

internal sealed record DeliveryAddressSnapshot(
    string Street,
    string City,
    string Country);
