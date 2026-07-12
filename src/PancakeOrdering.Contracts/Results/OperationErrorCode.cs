namespace PancakeOrdering.Contracts.Results;

public enum OperationErrorCode
{
    InternalError = 0,
    InvalidRequest,
    InvalidIngredient,
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
    IngredientUnavailable
}
