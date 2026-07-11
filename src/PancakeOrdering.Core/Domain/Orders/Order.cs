using PancakeOrdering.Core.Common.Results;
using PancakeOrdering.Core.Domain.Enums;
using PancakeOrdering.Core.Domain.Pancakes;
using PancakeOrdering.Core.Domain.States;

namespace PancakeOrdering.Core.Domain.Orders
{
    internal sealed class Order
    {
        private int _pancakesCounter;
        private readonly List<Pancake> _pancakes;

        private Order(DeliveryAddress deliveryAddress)
        {
            OrderId = Guid.NewGuid();
            CurrentState = DraftState.Instance;
            _pancakes = new List<Pancake>();
            DeliveryAddress = deliveryAddress;
        }

        public Guid OrderId { get; }

        public IOrderState CurrentState { get; private set; }

        public OrderStatus Status => CurrentState.Status;

        public DeliveryAddress DeliveryAddress { get; private set; }

        public Pancake[] Pancakes => _pancakes.ToArray();

        internal int PancakeCount => _pancakes.Count;

        public static Result<Order> Create(DeliveryAddress? deliveryAddress)
        {
            var validationResult = DeliveryAddress.Validate(deliveryAddress);
            return validationResult.IsSuccess
                ? Result.Success(new Order(deliveryAddress!))
                : Result.Failure<Order>(validationResult.Error!.Value);
        }

        public Result<int> AddPancake(Ingredient ingredient) => AddPancake([ingredient]);

        public Result<int> AddPancake(HashSet<Ingredient>? ingredients)
        {
            if (!CurrentState.CanModifyPancakes)
                return Result.Failure<int>(ErrorCode.CannotAddOrRemovePancakeInCurrentState);

            var pancake = ingredients == null ? new Pancake(GetNextPancakeId()) : new Pancake(GetNextPancakeId(), ingredients);

            _pancakes.Add(pancake);

            return Result.Success(pancake.Id);
        }
        
        public Result RemovePancake(int pancakeId)
        {
            if (!CurrentState.CanModifyPancakes)
                return Result.Failure(ErrorCode.CannotAddOrRemovePancakeInCurrentState);

            if (!_pancakes.Any())
                return Result.Failure(ErrorCode.NoPancakesToRemove);

            var pancakeToRemove = _pancakes.FirstOrDefault(a => a.Id == pancakeId);
            if (pancakeToRemove == null)
                return Result.Failure(ErrorCode.NoPancakeFound);

            _pancakes.Remove(pancakeToRemove);

            return Result.Success();
        }

        public Result AddIngredient(int pancakeId, Ingredient ingredient)
        {
            if (!CurrentState.CanModifyPancakes)
                return Result.Failure(ErrorCode.CannotAddOrRemovePancakeInCurrentState);

            var pancake = _pancakes.FirstOrDefault(a => a.Id == pancakeId);
            if (pancake == null)
                return Result.Failure(ErrorCode.NoPancakeFound);

            return pancake.AddIngredient(ingredient);
        }

        public Result RemoveIngredient(int pancakeId, Ingredient ingredient)
        {
            if (!CurrentState.CanModifyPancakes)
                return Result.Failure(ErrorCode.CannotAddOrRemovePancakeInCurrentState);

            var pancake = _pancakes.FirstOrDefault(a => a.Id == pancakeId);
            if (pancake == null)
                return Result.Failure(ErrorCode.NoPancakeFound);

            return pancake.RemoveIngredient(ingredient);
        }

        public Result ChangeDeliveryAddress(DeliveryAddress address)
        {
            if (!CurrentState.CanChangeAddress)
                return Result.Failure(ErrorCode.CannotChangeAddressInCurrentState);

            var validationResult = DeliveryAddress.Validate(address);
            if (!validationResult.IsSuccess)
                return validationResult;

            DeliveryAddress = address;
            return Result.Success();
        }

        
        public Result Confirm()             => TryTransition(OrderAction.Confirm);
        public Result StartPreparation()    => TryTransition(OrderAction.StartPreparation);
        public Result CompletePreparation() => TryTransition(OrderAction.CompletePreparation);
        public Result StartDelivery()       => TryTransition(OrderAction.StartDelivery);
        public Result CompleteDelivery()    => TryTransition(OrderAction.CompleteDelivery);
        public Result Archive()             => TryTransition(OrderAction.Archive);
        public Result Cancel()              => TryTransition(OrderAction.Cancel);

        internal Result ValidateConfirmation() => ValidateTransition(OrderAction.Confirm, out _);

        private Result TryTransition(OrderAction action)
        {
            var validationResult = ValidateTransition(action, out var nextState);
            if (!validationResult.IsSuccess)
                return validationResult;

            CurrentState.OnExit(this);
            CurrentState = nextState!;
            CurrentState.OnEnter(this);

            return Result.Success();
        }

        private Result ValidateTransition(OrderAction action, out IOrderState? nextState)
        {
            if (!OrderStateTransitions.TryResolve(CurrentState.Status, action, out var resolvedState))
            {
                nextState = null;
                return Result.Failure(ErrorCode.InvalidTransition);
            }

            var entryResult = resolvedState.ValidateEntry(this);
            if (!entryResult.IsSuccess)
            {
                nextState = null;
                return entryResult;
            }

            nextState = resolvedState;
            return Result.Success();
        }

        private int GetNextPancakeId() => ++_pancakesCounter;
    }
}
