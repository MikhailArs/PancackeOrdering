namespace PancakeOrdering.Contracts.Dtos;

public enum OrderStatusDto
{
    Draft,
    Confirmed,
    Preparing,
    Prepared,
    OutForDelivery,
    Delivered,
    Archived,
    Cancelled
}
