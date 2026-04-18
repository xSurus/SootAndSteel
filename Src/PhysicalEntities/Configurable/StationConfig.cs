using System.Collections.Generic;
using System.Linq;
using Gamelab.Utils;

namespace Gamelab.PhysicalEntities.Configurable;
// Feel free to move this namespace to Gamelab.Data or similar later!

public class StationConfig
{
    public string Title { get; set; }
    public string Description { get; set; }
    public bool AppearsInShop { get; set; }
    public int ShopPrice { get; set; }
}

public class ConfigurableStationRegistry
{
    private Dictionary<string, StationConfig> entities = new();

    public void Load(JsonLoader loader)
    {
        var loadedData = loader.LoadJson<Dictionary<string, StationConfig>>("stationConfigs.json");
        if (loadedData != null)
        {
            entities = loadedData;
        }
    }

    public StationConfig Get(string entityId)
    {
        return entities.GetValueOrDefault(entityId);
    }

    public List<KeyValuePair<string, StationConfig>> GetShopCatalog()
    {
        return entities.Where(e => e.Value.AppearsInShop).ToList();
    }
}