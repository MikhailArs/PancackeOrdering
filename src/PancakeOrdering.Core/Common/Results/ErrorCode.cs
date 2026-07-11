namespace PancakeOrdering.Core.Common.Results
{
    public enum ErrorCode
    {
        InternalError = 0,
        InvalidTransition,
        OrderMustContainPancake,
        NoPancakesToRemove,
        NoPancakeFound,
        CannotAddOrRemovePancakeInCurrentState,
        DuplicateIngredientAdded,
        IngredientNotFound,
        InvalidDeliveryAddress,
        CannotChangeAddressInCurrentState
    }
}
