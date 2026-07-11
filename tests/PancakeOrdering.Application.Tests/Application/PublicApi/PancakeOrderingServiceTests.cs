using System.Collections.Concurrent;
using System.Reflection;
using System.Xml.Linq;
using ContractResults = PancakeOrdering.Contracts.Results;
using CoreResults = PancakeOrdering.Core.Common.Results;
using PancakeOrdering.Application.Ports;
using PancakeOrdering.Contracts.Dtos;
using PancakeOrdering.Contracts.Requests;
using PancakeOrdering.Contracts.Services;

namespace PancakeOrdering.Application.Tests.Application.PublicApi
{
    public sealed class PancakeOrderingServiceTests
    {
        [Test]
        [Property("Requirement", "NFR-3")]
        [Property("Requirement", "NFR-6")]
        public void ProjectReferences_FollowRequiredDependencyDirection()
        {
            var contractsReferences = GetProjectReferences("src/PancakeOrdering.Contracts/PancakeOrdering.Contracts.csproj");
            var coreReferences = GetProjectReferences("src/PancakeOrdering.Core/PancakeOrdering.Core.csproj");
            var applicationReferences = GetProjectReferences("src/PancakeOrdering.Application/PancakeOrdering.Application.csproj");

            Assert.That(contractsReferences, Is.Empty);
            Assert.That(coreReferences, Does.Not.Contain("PancakeOrdering.Contracts.csproj"));
            Assert.That(coreReferences, Does.Not.Contain("PancakeOrdering.Application.csproj"));
            Assert.That(applicationReferences, Is.EquivalentTo(new[]
            {
                "PancakeOrdering.Contracts.csproj",
                "PancakeOrdering.Core.csproj"
            }));
        }

        [Test]
        [Property("Requirement", "FR-1")]
        [Property("Requirement", "NFR-2")]
        [Property("Requirement", "NFR-3")]
        public void CreateOrder_ReturnsDraftOrderDtoAndGetOrderReturnsSameOrder()
        {
            var service = CreateService();

            var createResult = service.CreateOrder(new CreateOrderRequest(CreateAddress()));
            var getResult = service.GetOrder(new GetOrderRequest(createResult.Value!.OrderId));

            Assert.That(createResult.IsSuccess, Is.True);
            Assert.That(createResult.Value!.OrderId, Is.Not.EqualTo(Guid.Empty));
            Assert.That(createResult.Value.Status, Is.EqualTo(OrderStatusDto.Draft));
            Assert.That(createResult.Value.DeliveryAddress, Is.EqualTo(CreateAddress()));
            Assert.That(createResult.Value.Pancakes, Is.Empty);
            Assert.That(getResult.IsSuccess, Is.True);
            Assert.That(getResult.Value!.OrderId, Is.EqualTo(createResult.Value.OrderId));
            Assert.That(getResult.Value.Status, Is.EqualTo(createResult.Value.Status));
            Assert.That(getResult.Value.DeliveryAddress, Is.EqualTo(createResult.Value.DeliveryAddress));
            Assert.That(getResult.Value.Pancakes, Is.Empty);
        }

        [Test]
        [Property("Requirement", "NFR-2")]
        [Property("Requirement", "NFR-4")]
        public void GetOrder_WhenOrderDoesNotExist_ReturnsTypedOrderNotFound()
        {
            var service = CreateService();

            var result = service.GetOrder(new GetOrderRequest(Guid.NewGuid()));

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.EqualTo(ContractResults.OperationErrorCode.OrderNotFound));
        }

        [Test]
        [Property("Requirement", "FR-8")]
        [Property("Requirement", "NFR-3")]
        public async Task AddPancake_WithNoIngredients_ReturnsOrderDtoWithEmptyPancake()
        {
            var service = CreateService();
            var orderId = CreateOrder(service);

            var result = await service.AddPancakeAsync(new AddPancakeRequest(orderId, Array.Empty<IngredientTypeDto>()));

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value!.Pancakes, Has.Count.EqualTo(1));
            Assert.That(result.Value.Pancakes.Single().PancakeId, Is.GreaterThan(0));
            Assert.That(result.Value.Pancakes.Single().Ingredients, Is.Empty);
        }

        [Test]
        [Property("Requirement", "FR-8")]
        [Property("Requirement", "FR-9")]
        [Property("Requirement", "NFR-3")]
        public async Task AddPancake_WithIngredients_MapsIngredientsToDto()
        {
            var service = CreateService();
            var orderId = CreateOrder(service);

            var result = await service.AddPancakeAsync(
                new AddPancakeRequest(orderId, new[] { IngredientTypeDto.Honey, IngredientTypeDto.Jam }));

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value!.Pancakes.Single().Ingredients, Is.EquivalentTo(new[]
            {
                IngredientTypeDto.Honey,
                IngredientTypeDto.Jam
            }));
        }

        [Test]
        [Property("Requirement", "FR-8")]
        public async Task RemovePancake_ReturnsOrderDtoWithoutRemovedPancake()
        {
            var service = CreateService();
            var orderId = CreateOrder(service);
            var addResult = await service.AddPancakeAsync(new AddPancakeRequest(orderId, Array.Empty<IngredientTypeDto>()));
            var pancakeId = addResult.Value!.Pancakes.Single().PancakeId;

            var result = await service.RemovePancakeAsync(new RemovePancakeRequest(orderId, pancakeId));

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value!.Pancakes, Is.Empty);
        }

        [Test]
        [Property("Requirement", "FR-9")]
        public async Task AddAndRemoveIngredient_ReturnUpdatedOrderDto()
        {
            var service = CreateService();
            var orderId = CreateOrder(service);
            var addPancakeResult = await service.AddPancakeAsync(new AddPancakeRequest(orderId, Array.Empty<IngredientTypeDto>()));
            var pancakeId = addPancakeResult.Value!.Pancakes.Single().PancakeId;

            var addIngredientResult = await service.AddIngredientAsync(
                new AddIngredientRequest(orderId, pancakeId, IngredientTypeDto.Chocolate));
            var removeIngredientResult = await service.RemoveIngredientAsync(
                new RemoveIngredientRequest(orderId, pancakeId, IngredientTypeDto.Chocolate));

            Assert.That(addIngredientResult.IsSuccess, Is.True);
            Assert.That(addIngredientResult.Value!.Pancakes.Single().Ingredients, Is.EqualTo(new[] { IngredientTypeDto.Chocolate }));
            Assert.That(removeIngredientResult.IsSuccess, Is.True);
            Assert.That(removeIngredientResult.Value!.Pancakes.Single().Ingredients, Is.Empty);
        }

        [Test]
        [Property("Requirement", "FR-2")]
        public async Task ChangeDeliveryAddress_ReturnsUpdatedAddressAndFailsAfterConfirmation()
        {
            var service = CreateService();
            var orderId = CreateOrder(service);
            var updatedAddress = new DeliveryAddressDto("Second Street", "Jerusalem", "Israel");

            var changeResult = await service.ChangeDeliveryAddressAsync(
                new ChangeDeliveryAddressRequest(orderId, updatedAddress));
            await service.AddPancakeAsync(new AddPancakeRequest(orderId, new[] { IngredientTypeDto.Honey }));
            await service.ConfirmOrderAsync(new ConfirmOrderRequest(orderId));
            var failedChangeResult = await service.ChangeDeliveryAddressAsync(
                new ChangeDeliveryAddressRequest(orderId, CreateAddress()));

            Assert.That(changeResult.IsSuccess, Is.True);
            Assert.That(changeResult.Value!.DeliveryAddress, Is.EqualTo(updatedAddress));
            Assert.That(failedChangeResult.IsSuccess, Is.False);
            Assert.That(failedChangeResult.Error, Is.EqualTo(ContractResults.OperationErrorCode.CannotChangeAddressInCurrentState));
        }

        [Test]
        [Property("Requirement", "FR-3")]
        [Property("Requirement", "FR-4")]
        public async Task ConfirmOrder_WithAcceptingKitchen_ReturnsConfirmedOrderDto()
        {
            var kitchen = new KitchenGatewayFake();
            var service = CreateService(kitchen);
            var orderId = CreateOrder(service);
            await service.AddPancakeAsync(new AddPancakeRequest(orderId, new[] { IngredientTypeDto.Honey }));

            var result = await service.ConfirmOrderAsync(new ConfirmOrderRequest(orderId));

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value!.Status, Is.EqualTo(OrderStatusDto.Confirmed));
            Assert.That(kitchen.CallCount, Is.EqualTo(1));
        }

        [Test]
        [Property("Requirement", "FR-7")]
        public async Task CancelOrder_WhenValid_ReturnsCancelledOrderDto()
        {
            var service = CreateService();
            var orderId = CreateOrder(service);

            var result = await service.CancelOrderAsync(new CancelOrderRequest(orderId));

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value!.Status, Is.EqualTo(OrderStatusDto.Cancelled));
        }

        [Test]
        [Property("Requirement", "NFR-4")]
        public async Task AddPancake_WithInvalidPublicIngredient_ReturnsTypedFailureAndLeavesOrderUnchanged()
        {
            var service = CreateService();
            var orderId = CreateOrder(service);

            var result = await service.AddPancakeAsync(
                new AddPancakeRequest(orderId, new[] { (IngredientTypeDto)999 }));
            var orderResult = service.GetOrder(new GetOrderRequest(orderId));

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error, Is.EqualTo(ContractResults.OperationErrorCode.InvalidIngredient));
            Assert.That(orderResult.IsSuccess, Is.True);
            Assert.That(orderResult.Value!.Pancakes, Is.Empty);
        }

        [Test]
        [Property("Requirement", "FR-5")]
        [Property("Requirement", "NFR-2")]
        [Property("Requirement", "NFR-5")]
        public async Task GetOrder_DuringIncompleteCommand_ReturnsPreviousPublishedSnapshot()
        {
            var delivery = new ControlledDeliveryGateway();
            var applicationService = CreateApplicationService(deliveryGateway: delivery);
            var service = CreateService(applicationService);
            var orderId = await CreatePreparingOrderAsync(applicationService, service);
            delivery.Block(orderId);

            var completePreparationTask = applicationService.CompletePreparationAsync(orderId);
            await delivery.WaitUntilEnteredAsync(orderId);

            var duringCommandResult = service.GetOrder(new GetOrderRequest(orderId));

            Assert.That(duringCommandResult.IsSuccess, Is.True);
            Assert.That(duringCommandResult.Value!.Status, Is.EqualTo(OrderStatusDto.Preparing));

            delivery.Release(orderId, CoreResults.Result.Success());

            var commandResult = await completePreparationTask.WaitAsync(TestTimeout);
            var afterCommandResult = service.GetOrder(new GetOrderRequest(orderId));

            Assert.That(commandResult.IsSuccess, Is.True);
            Assert.That(afterCommandResult.IsSuccess, Is.True);
            Assert.That(afterCommandResult.Value!.Status, Is.EqualTo(OrderStatusDto.Prepared));
        }

        [Test]
        [Property("Requirement", "FR-5")]
        [Property("Requirement", "NFR-5")]
        public async Task GetOrder_AfterFailedCommandThatMutatedOrder_ReturnsUpdatedSnapshot()
        {
            var delivery = new DeliveryGatewayFake(_ => Task.FromResult(CoreResults.Result.Failure(CoreResults.ErrorCode.DeliveryFailed)));
            var applicationService = CreateApplicationService(deliveryGateway: delivery);
            var service = CreateService(applicationService);
            var orderId = await CreatePreparingOrderAsync(applicationService, service);

            var commandResult = await applicationService.CompletePreparationAsync(orderId);
            var getResult = service.GetOrder(new GetOrderRequest(orderId));

            Assert.That(commandResult.IsSuccess, Is.False);
            Assert.That(commandResult.Error, Is.EqualTo(CoreResults.ErrorCode.DeliveryFailed));
            Assert.That(getResult.IsSuccess, Is.True);
            Assert.That(getResult.Value!.Status, Is.EqualTo(OrderStatusDto.Prepared));
        }

        [Test]
        [Property("Requirement", "FR-9")]
        [Property("Requirement", "NFR-3")]
        [Property("Requirement", "NFR-5")]
        public async Task PreviouslyReturnedDto_DoesNotChangeAfterLaterCommand()
        {
            var service = CreateService();
            var orderId = CreateOrder(service);
            var addPancakeResult = await service.AddPancakeAsync(
                new AddPancakeRequest(orderId, new[] { IngredientTypeDto.Honey }));
            var pancakeId = addPancakeResult.Value!.Pancakes.Single().PancakeId;

            var firstGetResult = service.GetOrder(new GetOrderRequest(orderId));
            var addIngredientResult = await service.AddIngredientAsync(
                new AddIngredientRequest(orderId, pancakeId, IngredientTypeDto.Jam));
            var secondGetResult = service.GetOrder(new GetOrderRequest(orderId));

            Assert.That(addIngredientResult.IsSuccess, Is.True);
            Assert.That(firstGetResult.IsSuccess, Is.True);
            Assert.That(secondGetResult.IsSuccess, Is.True);
            Assert.That(firstGetResult.Value!.Pancakes.Single().Ingredients, Is.EqualTo(new[] { IngredientTypeDto.Honey }));
            Assert.That(secondGetResult.Value!.Pancakes.Single().Ingredients, Is.EquivalentTo(new[] { IngredientTypeDto.Honey, IngredientTypeDto.Jam }));
            Assert.That(secondGetResult.Value.Pancakes, Is.Not.SameAs(firstGetResult.Value.Pancakes));
            Assert.That(secondGetResult.Value.Pancakes.Single().Ingredients, Is.Not.SameAs(firstGetResult.Value.Pancakes.Single().Ingredients));
        }

        [Test]
        [Property("Requirement", "NFR-3")]
        public async Task OrderDto_CollectionsAreRuntimeReadOnlyCollections()
        {
            var service = CreateService();
            var orderId = CreateOrder(service);

            var result = await service.AddPancakeAsync(
                new AddPancakeRequest(orderId, new[] { IngredientTypeDto.Chocolate }));

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value!.Pancakes, Is.AssignableTo<ICollection<PancakeDto>>());
            Assert.That(((ICollection<PancakeDto>)result.Value.Pancakes).IsReadOnly, Is.True);

            var ingredients = result.Value.Pancakes.Single().Ingredients;
            Assert.That(ingredients, Is.AssignableTo<ICollection<IngredientTypeDto>>());
            Assert.That(((ICollection<IngredientTypeDto>)ingredients).IsReadOnly, Is.True);
        }

        [Test]
        [Property("Requirement", "NFR-3")]
        public void ApplicationSnapshotQuery_DoesNotReturnMutableOrder()
        {
            var mutableOrderTypeName = "PancakeOrdering.Core.Domain.Orders.Order";
            var queryMethods = typeof(OrderApplicationService)
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(method => method.Name.Contains("GetOrder", StringComparison.Ordinal));

            foreach (var method in queryMethods)
            {
                Assert.That(TypeGraphContains(method.ReturnType, mutableOrderTypeName), Is.False, method.ToString());
            }
        }

        [Test]
        [Property("Requirement", "NFR-3")]
        public void PublicContractsGraph_DoesNotExposeCoreTypes()
        {
            var visited = new HashSet<Type>();

            foreach (var type in typeof(IPancakeOrderingService).Assembly.GetExportedTypes())
            {
                AssertNoCoreTypes(type, visited);
            }
        }

        [Test]
        [Property("Requirement", "NFR-2")]
        public void GetOrder_IsSynchronous()
        {
            var method = typeof(IPancakeOrderingService).GetMethod(nameof(IPancakeOrderingService.GetOrder));

            Assert.That(method, Is.Not.Null);
            Assert.That(method!.ReturnType, Is.EqualTo(typeof(ContractResults.OperationResult<OrderDto>)));
            Assert.That(typeof(Task).IsAssignableFrom(method.ReturnType), Is.False);
        }

        private static readonly TimeSpan TestTimeout = TimeSpan.FromSeconds(3);

        private static OrderApplicationService CreateApplicationService(
            IKitchenGateway? kitchenGateway = null,
            IDeliveryGateway? deliveryGateway = null,
            IArchiveGateway? archiveGateway = null)
        {
            return new OrderApplicationService(
                kitchenGateway ?? new KitchenGatewayFake(),
                deliveryGateway ?? new DeliveryGatewayFake(),
                archiveGateway ?? new ArchiveGatewayFake());
        }

        private static IPancakeOrderingService CreateService(IKitchenGateway? kitchenGateway = null) =>
            CreateService(CreateApplicationService(kitchenGateway));

        private static IPancakeOrderingService CreateService(OrderApplicationService applicationService) =>
            new PancakeOrderingService(applicationService);

        private static DeliveryAddressDto CreateAddress() =>
            new("Main Street", "Tel Aviv", "Israel");

        private static Guid CreateOrder(IPancakeOrderingService service)
        {
            var result = service.CreateOrder(new CreateOrderRequest(CreateAddress()));

            Assert.That(result.IsSuccess, Is.True);
            return result.Value!.OrderId;
        }

        private static async Task<Guid> CreatePreparingOrderAsync(
            OrderApplicationService applicationService,
            IPancakeOrderingService service)
        {
            var orderId = CreateOrder(service);
            var addPancakeResult = await service.AddPancakeAsync(
                new AddPancakeRequest(orderId, new[] { IngredientTypeDto.Honey }));
            var confirmResult = await service.ConfirmOrderAsync(new ConfirmOrderRequest(orderId));
            var startPreparationResult = await applicationService.StartPreparationAsync(orderId);

            Assert.That(addPancakeResult.IsSuccess, Is.True);
            Assert.That(confirmResult.IsSuccess, Is.True);
            Assert.That(startPreparationResult.IsSuccess, Is.True);
            return orderId;
        }

        private static IReadOnlyCollection<string> GetProjectReferences(string projectRelativePath)
        {
            var projectPath = Path.Combine(FindRepositoryRoot(), projectRelativePath);
            var document = XDocument.Load(projectPath);

            return document
                .Descendants("ProjectReference")
                .Select(reference => Path.GetFileName(reference.Attribute("Include")!.Value))
                .ToArray();
        }

        private static string FindRepositoryRoot()
        {
            var directory = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
            while (directory != null && !File.Exists(Path.Combine(directory.FullName, "PancakeOrdering.sln")))
            {
                directory = directory.Parent;
            }

            Assert.That(directory, Is.Not.Null);
            return directory!.FullName;
        }

        private static void AssertNoCoreTypes(Type type, HashSet<Type> visited)
        {
            if (!visited.Add(type))
                return;

            Assert.That(type.Namespace?.StartsWith("PancakeOrdering.Core", StringComparison.Ordinal), Is.Not.True, type.FullName);

            if (type.IsArray)
            {
                AssertNoCoreTypes(type.GetElementType()!, visited);
                return;
            }

            if (type.IsGenericType)
            {
                foreach (var argument in type.GetGenericArguments())
                {
                    AssertNoCoreTypes(argument, visited);
                }
            }

            if (type.Assembly != typeof(IPancakeOrderingService).Assembly)
                return;

            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                AssertNoCoreTypes(property.PropertyType, visited);
            }

            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance))
            {
                AssertNoCoreTypes(method.ReturnType, visited);
                foreach (var parameter in method.GetParameters())
                {
                    AssertNoCoreTypes(parameter.ParameterType, visited);
                }
            }
        }

        private static bool TypeGraphContains(Type type, string fullName)
        {
            if (type.FullName == fullName)
                return true;

            if (type.IsArray)
                return TypeGraphContains(type.GetElementType()!, fullName);

            if (!type.IsGenericType)
                return false;

            return type.GetGenericArguments().Any(argument => TypeGraphContains(argument, fullName));
        }

        private sealed class KitchenGatewayFake : IKitchenGateway
        {
            private readonly ConcurrentQueue<Guid> _calls = new();

            public int CallCount => _calls.Count;

            public Task<CoreResults.Result> AcceptOrderAsync(Guid orderId)
            {
                _calls.Enqueue(orderId);
                return Task.FromResult(CoreResults.Result.Success());
            }
        }

        private sealed class DeliveryGatewayFake : IDeliveryGateway
        {
            private readonly Func<Guid, Task<CoreResults.Result>> _submitOrder;

            public DeliveryGatewayFake(Func<Guid, Task<CoreResults.Result>>? submitOrder = null)
            {
                _submitOrder = submitOrder ?? (_ => Task.FromResult(CoreResults.Result.Success()));
            }

            public Task<CoreResults.Result> SubmitOrderAsync(Guid orderId) =>
                _submitOrder(orderId);
        }

        private sealed class ArchiveGatewayFake : IArchiveGateway
        {
            public Task<CoreResults.Result> ArchiveOrderAsync(Guid orderId) =>
                Task.FromResult(CoreResults.Result.Success());
        }

        private sealed class ControlledDeliveryGateway : IDeliveryGateway
        {
            private readonly ConcurrentDictionary<Guid, TaskCompletionSource<bool>> _enteredSignals = new();
            private readonly ConcurrentDictionary<Guid, TaskCompletionSource<CoreResults.Result>> _blockedOrders = new();

            public void Block(Guid orderId)
            {
                _blockedOrders[orderId] = NewCompletionSource<CoreResults.Result>();
            }

            public async Task WaitUntilEnteredAsync(Guid orderId)
            {
                var signal = _enteredSignals.GetOrAdd(orderId, _ => NewCompletionSource<bool>());
                await signal.Task.WaitAsync(TestTimeout);
            }

            public void Release(Guid orderId, CoreResults.Result result)
            {
                _blockedOrders[orderId].SetResult(result);
            }

            public Task<CoreResults.Result> SubmitOrderAsync(Guid orderId)
            {
                _enteredSignals
                    .GetOrAdd(orderId, _ => NewCompletionSource<bool>())
                    .TrySetResult(true);

                return _blockedOrders.TryGetValue(orderId, out var release)
                    ? release.Task
                    : Task.FromResult(CoreResults.Result.Success());
            }

            private static TaskCompletionSource<T> NewCompletionSource<T>() =>
                new(TaskCreationOptions.RunContinuationsAsynchronously);
        }
    }
}
