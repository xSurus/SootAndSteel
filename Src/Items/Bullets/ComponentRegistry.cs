using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Gamelab.PhysicalEntities.Bullets;
using Gamelab.PhysicalEntities.Bullets.Components.Casings;
using Gamelab.PhysicalEntities.Bullets.Components.Projectiles;
using Gamelab.PhysicalEntities.Bullets.Components.Propellants;
using Gamelab.Utils.Logging;
using Microsoft.Xna.Framework;
using JsonSerializerOptions = System.Text.Json.JsonSerializerOptions;

namespace Gamelab.Items.Bullets;

public static class ComponentRegistry
{
    private static readonly Logger logger = new ("ComponentRegistry");

    
    private class ComponentDescription {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Sprite { get; set; }

        [JsonIgnore]
        public Color Color { get; set; }
        
        [JsonPropertyName("color")]
        public string StringColor
        {
            private get => "";
            set
            {
                int r = int.Parse(value.Substring(0, 2), NumberStyles.HexNumber);
                int g = int.Parse(value.Substring(2, 2), NumberStyles.HexNumber);
                int b = int.Parse(value.Substring(4, 2), NumberStyles.HexNumber);
            
                Color = new Color(r, g, b);
            }
        }
    }
    
    private static readonly Dictionary<string, ComponentDefinition> Definitions = new();
    private static Dictionary<string, ComponentDescription> _texts;
    
    public static void Initialize()
    {
        Register(new ComponentDefinition(EComponentType.Casing, "BasicCasing", new BasicCasing()));
        Register(new ComponentDefinition(EComponentType.Projectile, "BasicProjectile", new BasicProjectile()));
        Register(new ComponentDefinition(EComponentType.Propellant, "BasicPropellant", new BasicPropellant()));
        Register(new ComponentDefinition(EComponentType.Projectile, "ScatterProjectile", new ScatterProjectile()));
        Register(new ComponentDefinition(EComponentType.Projectile, "EnemyProjectile", new EnemyProjectile()));
        Register(new ComponentDefinition(EComponentType.Casing, "HomingCasing", new HomingCasing()));
        LoadDescriptions();
    }

    private static void LoadDescriptions()
    {
        using Stream stream = TitleContainer.OpenStream("Config/ComponentConfig.json");
        using StreamReader reader = new StreamReader(stream);
        string json = reader.ReadToEnd();
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        _texts = JsonSerializer.Deserialize<Dictionary<string, ComponentDescription>>(json, options);
        logger.Info($"Loaded {_texts.Count} component descriptions");
        foreach (var (componentId, description) in _texts)
        {
            logger.Info($"Loading description for '{componentId}': Name='{description.Name}', Description='{description.Description}', Color='{description.Color}'");
        }
    }

    private static void Register(ComponentDefinition definition)
    {
        Definitions[definition.ComponentId] = definition;
    }
    
    public static ComponentDefinition GetDefinition(string componentId)
    {
        return Definitions.TryGetValue(componentId, out var def) ? def 
            : throw new Exception($"Component '{componentId}' does not exist in the registry.");
    }

    public static string GetName(string componentId)
    {
        return _texts.TryGetValue(componentId, out var value) ? value.Name
            : throw new Exception($"Component '{componentId}' does not exist in the registry.");
    }
    
    public static string GetDescription(string componentId)
    {
        return _texts.TryGetValue(componentId, out var value) ? value.Description
            : throw new Exception($"Component '{componentId}' does not exist in the registry.");
    }

    public static Color GetColor(string componentId)
    {
        return _texts.TryGetValue(componentId, out var value) ? value.Color 
            : throw new Exception($"Component '{componentId}' does not exist in the registry."); 
    }

    public static string GetSprite(string componentId)
    {
        return _texts.TryGetValue(componentId, out var value) && value.Sprite != null
            ? value.Sprite
            : "ComponentResource";
    }
}