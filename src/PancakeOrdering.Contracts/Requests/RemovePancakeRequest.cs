namespace PancakeOrdering.Contracts.Requests;

public sealed record RemovePancakeRequest(
    Guid OrderId,
    int PancakeId);
