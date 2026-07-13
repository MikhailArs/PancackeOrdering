using PancakeOrdering.Contracts.Dtos;
using PancakeOrdering.Contracts.Requests;
using PancakeOrdering.Contracts.Results;
using PancakeOrdering.Contracts.Services;
using PancakeOrdering.Infrastructure;
using System.Text;

var stock = new Dictionary<IngredientTypeDto, int>
{
    [IngredientTypeDto.Jam] = 10,
    [IngredientTypeDto.Chocolate] = 10, 
    [IngredientTypeDto.Honey] = 10
};

IPancakeOrderingService pancakeService = InMemoryPancakeOrderingComposition.Create(stock);

Console.WriteLine("Pancake Ordering Service Demo");
Console.WriteLine();

var orderId = CreateNewOrder(pancakeService, "Israel", "Tel Aviv", "Main Street");
await AddPancake(pancakeService, orderId, [IngredientTypeDto.Chocolate]);
await AddPancake(pancakeService, orderId, [IngredientTypeDto.Honey]);
await AddPancake(pancakeService, orderId, [IngredientTypeDto.Chocolate, IngredientTypeDto.Jam]);
await ConfirmOrder(pancakeService, orderId);

await RunOrderOperation("Start preparation", pancakeService.StartPreparationAsync(orderId));
await RunOrderOperation("Complete preparation", pancakeService.CompletePreparationAsync(orderId));
await RunOrderOperation("Start delivery", pancakeService.StartDeliveryAsync(orderId));
await RunOrderOperation("Complete delivery", pancakeService.CompleteDeliveryAsync(orderId));
await RunOrderOperation("Archive order", pancakeService.ArchiveAsync(orderId));

Console.WriteLine("Press any key to exit");
Console.ReadLine();


static void Terminate()
{
    Console.WriteLine("Application terminated");
    Environment.Exit(1);
}


static Guid CreateNewOrder(IPancakeOrderingService service, string country, string city, string street)
{
    var address = new DeliveryAddressDto(street, city, country);
    Console.WriteLine($"Creating new Order for address Country: {address.Country}, City: {address.City}, Street: {address.Street}, ");

    OperationResult<OrderDto> create = service.CreateOrder(new CreateOrderRequest(address));

    Console.WriteLine($"Order creations status - Is Success: {create.IsSuccess}\n");
    if (create.IsFailure)
        Terminate();

    return create.Value!.OrderId;
}

static async Task AddPancake(IPancakeOrderingService service, Guid orderId, IngredientTypeDto[] ingredients)
{
    var addPancakeRequest = new AddPancakeRequest(orderId, ingredients);
    Console.WriteLine($"Adding Pancake with {string.Join(",", addPancakeRequest.Ingredients)}");
    
    OperationResult<OrderDto> addPancakeResult = await service.AddPancakeAsync(addPancakeRequest);
    
    Console.WriteLine($"Add Pancake order status - Is Success: {addPancakeResult.IsSuccess}\n");
    if (addPancakeResult.IsFailure)
        Terminate();
}

static async Task ConfirmOrder(IPancakeOrderingService service, Guid orderId)
{
    OperationResult<OrderDto> confirm = await service.ConfirmOrderAsync(new ConfirmOrderRequest(orderId));
    Console.WriteLine($"Order confirmation status - Is Success: {confirm.IsSuccess}\n");
    if (confirm.IsFailure)
        Terminate();

    Console.WriteLine($"Order ({confirm.Value!.OrderId})");
    Console.WriteLine("Contains:");
    Console.WriteLine($"{PrintPancakes(confirm.Value!.Pancakes.ToArray())}");
    Console.WriteLine($"Order status: {confirm.Value.Status}");
}

static async Task RunOrderOperation(string operation, Task<OperationResult<OrderDto>> operationTask)
{
    OperationResult<OrderDto> result = await operationTask;

    Console.WriteLine($"{operation} status - Is Success: {result.IsSuccess}");
    if (result.IsFailure)
    {
        Console.WriteLine($"Error: {result.Error}\n");
        Terminate();
    }

    Console.WriteLine($"Order status: {result.Value!.Status}\n");
}

static string PrintPancakes(PancakeDto[] pancakes)
{
    var sb = new StringBuilder();
    foreach (var item in pancakes)
    {
        sb.Append($"Pancake No: {item.PancakeId}. Ingredients: {string.Join(",", item.Ingredients)}\n");
    }

    return sb.ToString();
}
