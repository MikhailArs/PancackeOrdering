namespace PancakeOrdering.Core.Common.Results
{
    public enum ErrorCode
    {
        InternalError = 0,
        InvalidTransition,
        OrderMustContainPancake,
        NoPancakesToRemove,
        PancakeNotFound,
        CannotAddOrRemovePancakeInCurrentState,
        DuplicateIngredientAdded,
        IngredientNotFound,
        InvalidDeliveryAddress,
        CannotChangeAddressInCurrentState,
        OrderNotFound,
        KitchenDeclined,
        DeliveryFailed,
        ArchiveFailed
    }
}
