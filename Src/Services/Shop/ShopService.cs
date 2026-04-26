using System.Collections.Generic;
using System.Linq;
using Gamelab.Data;
using Gamelab.Items.Bullets;
using Gamelab.PhysicalEntities.Configurable;
using Gamelab.PhysicalEntities.Stations;

namespace Gamelab.Services.Shop;

public class ShopManager : IShopService
{
    private readonly StationRegistry stationRegistry = GamelabGame.Instance.StationRegistry;
    private readonly ComponentRegistry componentRegistry = GamelabGame.Instance.ComponentRegistry;

    public List<CatalogItem> GenerateCatalog()
    {
        var fullCatalog = new List<CatalogItem>();
        var stationItems = stationRegistry.GetAll()
            .Where(kvp => kvp.Value.AppearsInShop)
            .Select(kvp => new CatalogItem
            {
                ItemId = kvp.Key,
                Name = kvp.Value.Name,
                Description = kvp.Value.Description,
                Price = kvp.Value.ShopPrice
            });

        fullCatalog.AddRange(stationItems);

        var componentItems = componentRegistry.GetAll()
            .Where(kvp => kvp.Value.AppearsInShop)
            .Select(kvp => new CatalogItem
            {
                ItemId = StationIds.GetResourceStationId(StationIds.GetComponentResourceId(kvp.Key)),
                Name = kvp.Value.Name,
                Description = kvp.Value.Description,
                Price = kvp.Value.ShopPrice
            });

        fullCatalog.AddRange(componentItems);
        return fullCatalog;
    }

    public CatalogItem GetCatalogItem(string itemId)
    {
        return GenerateCatalog().FirstOrDefault(c => c.ItemId == itemId);
    }
}