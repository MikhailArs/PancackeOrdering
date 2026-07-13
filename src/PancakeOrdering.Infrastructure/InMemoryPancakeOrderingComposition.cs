using PancakeOrdering.Application;
using PancakeOrdering.Application.Orders.Snapshots;
using PancakeOrdering.Contracts.Dtos;
using PancakeOrdering.Contracts.Services;
using PancakeOrdering.Infrastructure.Archive;
using PancakeOrdering.Infrastructure.Delivery;
using PancakeOrdering.Infrastructure.Kitchen;

namespace PancakeOrdering.Infrastructure
{
    public static class InMemoryPancakeOrderingComposition
    {
        public static IPancakeOrderingService Create(
            IReadOnlyDictionary<IngredientTypeDto, int> stock)
        {
            var snapshotStore = new OrderSnapshotStore();
            var queryService = new OrderQueryService(snapshotStore);

            var kitchen = new InMemoryKitchen(queryService, stock);

            var orderService = new OrderApplicationService(
                kitchen,
                new InMemoryDelivery(),
                new InMemoryArchive(),
                kitchen,
                snapshotStore);

            return new PancakeOrderingService(orderService, queryService);
        }
    }
}
