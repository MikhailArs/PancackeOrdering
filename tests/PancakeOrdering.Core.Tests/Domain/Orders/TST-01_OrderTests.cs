using PancakeOrdering.Core.Common.Results;
using PancakeOrdering.Core.Domain.Enums;
using PancakeOrdering.Core.Domain.Orders;

namespace PancakeOrdering.Core.Tests.Domain.Orders
{
    public sealed class OrderTests
    {
        [Test]
        public void Confirm_WithOnePancake_ChangesStatusToConfirmed()
        {
            var order = CreateOrder();
            order.AddPancake(Ingredient.Chocolate);

            Assert.That(order.Status, Is.EqualTo(OrderStatus.Draft));

            var result = order.Confirm();

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(order.Status, Is.EqualTo(OrderStatus.Confirmed));
        }

        [Test]
        public void Confirm_WithoutPancakes_ReturnsFailureAndKeepsDraftStatus()
        {
            var order = CreateOrder();

            Assert.That(order.Status, Is.EqualTo(OrderStatus.Draft));

            var result = order.Confirm();

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error, Is.EqualTo(ErrorCode.OrderMustContainPancake));
            Assert.That(order.Status, Is.EqualTo(OrderStatus.Draft));
        }
        
        [Test]
        public void AddPancake_OnDraftState_PancakeAdded()
        {
            var order = CreateOrder();

            Assert.That(order.Status, Is.EqualTo(OrderStatus.Draft));

            var result = order.AddPancake(Ingredient.Honey);

            Assert.That(result.IsSuccess, Is.True);
        }
        
        [Test]
        public void RemovePancake_OnDraftState_PancakeRemoved()
        {
            var order = CreateOrder();
            Assert.That(order.Status, Is.EqualTo(OrderStatus.Draft));

            var resultInt = order.AddPancake(Ingredient.Honey);
            Assert.That(resultInt.IsSuccess, Is.True);

            var pancakeToRemove = order.Pancakes.FirstOrDefault();
            Assert.That(pancakeToRemove != null, Is.True);

            var result = order.RemovePancake(pancakeToRemove!.Id);
            Assert.That(result.IsSuccess, Is.True);
        }

        [Test]
        public void RemoveNonExistingPancake_OnDraftState_PancakeAdded()
        {
            var order = CreateOrder();
            Assert.That(order.Status, Is.EqualTo(OrderStatus.Draft));

            var resultInt = order.AddPancake(Ingredient.Honey);
            Assert.That(resultInt.IsSuccess, Is.True);

            var pancakeToRemove = order.Pancakes.FirstOrDefault();
            Assert.That(pancakeToRemove != null, Is.True);

            var result = order.RemovePancake(pancakeToRemove!.Id);
            Assert.That(result.IsSuccess, Is.True);
            
            result = order.RemovePancake(pancakeToRemove!.Id);
            Assert.That(result.IsSuccess, Is.False);
        }

        [Test]
        public void RemovePancake_OnConfirmedState_ReturnsFailure()
        {
            var order = CreateOrder();
            Assert.That(order.Status, Is.EqualTo(OrderStatus.Draft));

            var resultInt = order.AddPancake(Ingredient.Honey);
            Assert.That(resultInt.IsSuccess, Is.True);

            var result = order.Confirm();
            Assert.That(result.IsSuccess, Is.True);

            var pancakeToRemove = order.Pancakes.FirstOrDefault();
            Assert.That(pancakeToRemove != null, Is.True);

            result = order.RemovePancake(pancakeToRemove!.Id);
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error, Is.EqualTo(ErrorCode.CannotAddOrRemovePancakeInCurrentState));
        }

        [Test]
        public void AddPancake_OnConfirmedState_ReturnsFailure()
        {
            var order = CreateOrder();
            order.AddPancake(Ingredient.Chocolate);

            Assert.That(order.Status, Is.EqualTo(OrderStatus.Draft));

            var result = order.Confirm();
            Assert.That(result.IsSuccess, Is.True);

            result = order.AddPancake(Ingredient.Honey);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error, Is.EqualTo(ErrorCode.CannotAddOrRemovePancakeInCurrentState));
        }

        [Test]
        public void AddIngredient_OnDraftState_AddsIngredient()
        {
            var order = CreateOrder();
            var pancakeId = AddEmptyPancake(order);

            var result = order.AddIngredient(pancakeId, Ingredient.Honey);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(order.Pancakes.Single().Ingredients, Does.Contain(Ingredient.Honey));
        }

        [Test]
        public void AddIngredient_WhenAlreadyExists_ReturnsFailure()
        {
            var order = CreateOrder();
            var pancakeId = AddEmptyPancake(order);
            order.AddIngredient(pancakeId, Ingredient.Honey);

            var result = order.AddIngredient(pancakeId, Ingredient.Honey);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error, Is.EqualTo(ErrorCode.DuplicateIngredientAdded));
        }

        [Test]
        public void RemoveIngredient_OnDraftState_RemovesIngredient()
        {
            var order = CreateOrder();
            var pancakeId = AddEmptyPancake(order);
            order.AddIngredient(pancakeId, Ingredient.Jam);

            var result = order.RemoveIngredient(pancakeId, Ingredient.Jam);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(order.Pancakes.Single().Ingredients, Does.Not.Contain(Ingredient.Jam));
        }

        [Test]
        public void RemoveIngredient_WhenIngredientDoesNotExist_ReturnsFailure()
        {
            var order = CreateOrder();
            var pancakeId = AddEmptyPancake(order);

            var result = order.RemoveIngredient(pancakeId, Ingredient.Chocolate);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error, Is.EqualTo(ErrorCode.IngredientNotFound));
        }

        [Test]
        public void ModifyIngredients_OnConfirmedOrder_ReturnsFailure()
        {
            var order = CreateOrder();
            var addPancakeResult = order.AddPancake(Ingredient.Honey);
            Assert.That(addPancakeResult.IsSuccess, Is.True);
            var pancakeId = (int)addPancakeResult.Value!;
            var confirmResult = order.Confirm();
            Assert.That(confirmResult.IsSuccess, Is.True);

            var addIngredientResult = order.AddIngredient(pancakeId, Ingredient.Jam);
            var removeIngredientResult = order.RemoveIngredient(pancakeId, Ingredient.Honey);

            Assert.That(addIngredientResult.IsSuccess, Is.False);
            Assert.That(addIngredientResult.Error, Is.EqualTo(ErrorCode.CannotAddOrRemovePancakeInCurrentState));
            Assert.That(removeIngredientResult.IsSuccess, Is.False);
            Assert.That(removeIngredientResult.Error, Is.EqualTo(ErrorCode.CannotAddOrRemovePancakeInCurrentState));
        }

        [Test]
        public void CreateOrder_WithValidAddress_AddressStored()
        {
            var address = new DeliveryAddress("Main Street", "Tel Aviv", "Israel");

            var result = Order.Create(address);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value!.DeliveryAddress.Street, Is.EqualTo("Main Street"));
            Assert.That(result.Value.DeliveryAddress.City, Is.EqualTo("Tel Aviv"));
            Assert.That(result.Value.DeliveryAddress.Country, Is.EqualTo("Israel"));
        }

        [Test]
        public void ChangeAddress_OnDraftState_AddressChanged()
        {
            var order = CreateOrder();
            var newAddress = new DeliveryAddress("Second Street", "Jerusalem", "Israel");

            var result = order.ChangeDeliveryAddress(newAddress);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(order.DeliveryAddress.Street, Is.EqualTo("Second Street"));
            Assert.That(order.DeliveryAddress.City, Is.EqualTo("Jerusalem"));
            Assert.That(order.DeliveryAddress.Country, Is.EqualTo("Israel"));
        }

        [Test]
        public void ChangeAddress_OnConfirmedOrder_ReturnsFailure()
        {
            var order = CreateOrder();
            var originalAddress = order.DeliveryAddress;
            order.AddPancake(Ingredient.Honey);
            var confirmResult = order.Confirm();
            Assert.That(confirmResult.IsSuccess, Is.True);

            var result = order.ChangeDeliveryAddress(new DeliveryAddress("Second Street", "Jerusalem", "Israel"));

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error, Is.EqualTo(ErrorCode.CannotChangeAddressInCurrentState));
            Assert.That(order.DeliveryAddress, Is.EqualTo(originalAddress));
        }

        [Test]
        public void CreateAddress_WithNullField_ReturnsFailure()
        {
            var address = new DeliveryAddress(null, "Tel Aviv", "Israel");

            var result = Order.Create(address);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error, Is.EqualTo(ErrorCode.InvalidDeliveryAddress));
        }

        [Test]
        public void ChangeAddress_WithInvalidAddress_ReturnsFailureAndKeepsCurrentAddress()
        {
            var order = CreateOrder();
            var originalAddress = order.DeliveryAddress;

            var result = order.ChangeDeliveryAddress(
                new DeliveryAddress(null, "Tel Aviv", "Israel"));

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error, Is.EqualTo(ErrorCode.InvalidDeliveryAddress));
            Assert.That(order.DeliveryAddress, Is.EqualTo(originalAddress));
        }

        private static int AddEmptyPancake(Order order)
        {
            var result = order.AddPancake(null);

            Assert.That(result.IsSuccess, Is.True);
            return (int)result.Value!;
        }

        private static Order CreateOrder()
        {
            var result = Order.Create(new DeliveryAddress("Main Street", "Tel Aviv", "Israel"));

            Assert.That(result.IsSuccess, Is.True);
            return result.Value!;
        }
    }
}
