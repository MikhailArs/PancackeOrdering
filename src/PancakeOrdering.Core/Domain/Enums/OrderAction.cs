namespace PancakeOrdering.Core.Domain.Enums
{
    internal enum OrderAction
    {
        Confirm,
        Cancel,
        StartPreparation,
        CompletePreparation,
        StartDelivery,
        CompleteDelivery,
        Archive
    }
}
