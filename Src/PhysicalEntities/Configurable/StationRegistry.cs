using System;
using System.Collections.Generic;
using System.Linq;
using Gamelab.Data;
using Gamelab.Items;
using Gamelab.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
namespace Gamelab.PhysicalEntities.Configurable;

public class StationConfig
{
    public string Name { get; set; }
    public string Description { get; set; }
    public bool AppearsInShop { get; set; }
    public int ShopPrice { get; set; }

    [JsonConverter(typeof(StringEnumConverter))]
    public EItemType? ItemType { get; set; }
}

public class StationRegistry
{
    private static readonly Logger Logger = new("StationRegistry");
    private Dictionary<string, StationConfig> configs = new();

    public void Load(JsonLoader loader)
    {
        var loadedData = loader.LoadJson<Dictionary<string, StationConfig>>("StationConfig.json");
        if (loadedData != null)
        {
            configs = loadedData;
            Logger.Info($"Loaded {configs.Count} station configs");
            foreach (var (stationId, config) in configs)
            {
                Logger.Info($"Loading config for '{stationId}': Name='{config.Name}', Description='{config.Description}', ItemType='{config.ItemType?.ToString() ?? "<none>"}', InShop='{config.AppearsInShop}', Price='{config.ShopPrice}'");
            }
        }
        else
        {
            Logger.Error("Failed to load station configs from 'StationConfig.json'.");
        }
    }

    public StationConfig Get(string entityId)
    {
        return configs.TryGetValue(entityId, out var config) ? config : null;
    }
    
    
    public IEnumerable<KeyValuePair<string, StationConfig>> GetAll() 
    {
        return configs;
    }
}