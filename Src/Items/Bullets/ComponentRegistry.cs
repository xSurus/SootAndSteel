using System;
using System.Collections.Generic;
using System.Globalization;
using Gamelab.Utils;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace Gamelab.Items.Bullets;

public class ComponentConfig
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string Sprite { get; set; }
    public bool AppearsInShop { get; set; }
    public int ShopPrice { get; set; }

    [JsonIgnore] public Color Color { get; set; }

    private string stringColor = "";

    [JsonProperty("color")]
    public string StringColor
    {
        get => stringColor;
        set
        {
            stringColor = value;

            string hex = value.TrimStart('#');
            if (hex.Length >= 6)
            {
                int r = int.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
                int g = int.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
                int b = int.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);

                Color = new Color(r, g, b);
            }
        }
    }
}

public class ComponentRegistry
{
    private readonly Logger logger = new("ComponentRegistry");

    private Dictionary<string, ComponentConfig> configs = new();

    public void Load(JsonLoader loader)
    {
        var loadedData = loader.LoadJson<Dictionary<string, ComponentConfig>>("ComponentConfig.json");
        if (loadedData != null)
        {
            configs = loadedData;
            logger.Info($"Loaded {configs.Count} component configs");
            foreach ((string componentId, ComponentConfig config) in configs)
            {
                logger.Info(
                    $"Loading config for '{componentId}': Name='{config.Name}', Description='{config.Description}', Color='{config.Color}', InShop='{config.AppearsInShop}', Price='{config.ShopPrice}'");
            }
        }
        else
        {
            logger.Error("Failed to load component configs from 'Data/ComponentConfig.json'.");
        }
    }

    public ComponentConfig Get(string componentId)
    {
        return configs.TryGetValue(componentId, out var config)
            ? config
            : throw new Exception($"Component config '{componentId}' does not exist in the registry.");
    }

    public IEnumerable<KeyValuePair<string, ComponentConfig>> GetAll()
    {
        return configs;
    }
}