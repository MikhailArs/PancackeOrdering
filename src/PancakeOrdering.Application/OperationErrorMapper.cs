using ContractResults = PancakeOrdering.Contracts.Results;
using CoreResults = PancakeOrdering.Core.Common.Results;

namespace PancakeOrdering.Application
{
    internal static class OperationErrorMapper
    {
        public static ContractResults.OperationErrorCode ToOperationError(CoreResults.ErrorCode error)
        {
            return error switch
            {
                CoreResults.ErrorCode.InternalError => ContractResults.OperationErrorCode.InternalError,
                CoreResults.ErrorCode.InvalidTransition => ContractResults.OperationErrorCode.InvalidTransition,
                CoreResults.ErrorCode.OrderMustContainPancake => ContractResults.OperationErrorCode.OrderMustContainPancake,
                CoreResults.ErrorCode.NoPancakesToRemove => ContractResults.OperationErrorCode.NoPancakesToRemove,
                CoreResults.ErrorCode.PancakeNotFound => ContractResults.OperationErrorCode.PancakeNotFound,
                CoreResults.ErrorCode.CannotAddOrRemovePancakeInCurrentState => ContractResults.OperationErrorCode.CannotAddOrRemovePancakeInCurrentState,
                CoreResults.ErrorCode.DuplicateIngredientAdded => ContractResults.OperationErrorCode.DuplicateIngredientAdded,
                CoreResults.ErrorCode.IngredientNotFound => ContractResults.OperationErrorCode.IngredientNotFound,
                CoreResults.ErrorCode.InvalidDeliveryAddress => ContractResults.OperationErrorCode.InvalidDeliveryAddress,
                CoreResults.ErrorCode.CannotChangeAddressInCurrentState => ContractResults.OperationErrorCode.CannotChangeAddressInCurrentState,
                CoreResults.ErrorCode.OrderNotFound => ContractResults.OperationErrorCode.OrderNotFound,
                CoreResults.ErrorCode.KitchenDeclined => ContractResults.OperationErrorCode.KitchenDeclined,
                CoreResults.ErrorCode.IngredientUnavailable => ContractResults.OperationErrorCode.IngredientUnavailable,
                _ => ContractResults.OperationErrorCode.InternalError
            };
        }

        public static CoreResults.ErrorCode ToCoreError(ContractResults.OperationErrorCode error)
        {
            return error switch
            {
                ContractResults.OperationErrorCode.InternalError => CoreResults.ErrorCode.InternalError,
                ContractResults.OperationErrorCode.InvalidTransition => CoreResults.ErrorCode.InvalidTransition,
                ContractResults.OperationErrorCode.OrderMustContainPancake => CoreResults.ErrorCode.OrderMustContainPancake,
                ContractResults.OperationErrorCode.NoPancakesToRemove => CoreResults.ErrorCode.NoPancakesToRemove,
                ContractResults.OperationErrorCode.PancakeNotFound => CoreResults.ErrorCode.PancakeNotFound,
                ContractResults.OperationErrorCode.CannotAddOrRemovePancakeInCurrentState => CoreResults.ErrorCode.CannotAddOrRemovePancakeInCurrentState,
                ContractResults.OperationErrorCode.DuplicateIngredientAdded => CoreResults.ErrorCode.DuplicateIngredientAdded,
                ContractResults.OperationErrorCode.IngredientNotFound => CoreResults.ErrorCode.IngredientNotFound,
                ContractResults.OperationErrorCode.InvalidDeliveryAddress => CoreResults.ErrorCode.InvalidDeliveryAddress,
                ContractResults.OperationErrorCode.CannotChangeAddressInCurrentState => CoreResults.ErrorCode.CannotChangeAddressInCurrentState,
                ContractResults.OperationErrorCode.OrderNotFound => CoreResults.ErrorCode.OrderNotFound,
                ContractResults.OperationErrorCode.KitchenDeclined => CoreResults.ErrorCode.KitchenDeclined,
                ContractResults.OperationErrorCode.IngredientUnavailable => CoreResults.ErrorCode.IngredientUnavailable,
                _ => CoreResults.ErrorCode.InternalError
            };
        }
    }
}
