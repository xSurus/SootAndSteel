using System;
using System.Collections.Generic;

namespace Gamelab.Items.Bullets
{
    // Src also has Color and Sprite; both are draw-only and stay out of Core.
    public class ComponentConfig
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool AppearsInShop { get; set; }
        public int ShopPrice { get; set; }
    }

    public class ComponentRegistry
    {
        private readonly List<KeyValuePair<string, ComponentConfig>> ordered = new List<KeyValuePair<string, ComponentConfig>>();
        private readonly Dictionary<string, ComponentConfig> byId = new Dictionary<string, ComponentConfig>();

        public ComponentRegistry(IEnumerable<KeyValuePair<string, ComponentConfig>> configs)
        {
            foreach (var kvp in configs)
            {
                ordered.Add(kvp);
                byId[kvp.Key] = kvp.Value;
            }
        }

        public ComponentConfig Get(string componentId)
        {
            if (byId.TryGetValue(componentId, out var config)) return config;
            throw new Exception($"Component config '{componentId}' does not exist in the registry.");
        }

        public IEnumerable<KeyValuePair<string, ComponentConfig>> GetAll()
        {
            return ordered;
        }
    }
}
