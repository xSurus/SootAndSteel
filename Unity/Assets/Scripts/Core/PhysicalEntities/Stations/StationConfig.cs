using System.Collections.Generic;
using Gamelab.Items;

namespace Gamelab.PhysicalEntities.Configurable
{
    public class StationConfig
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool AppearsInShop { get; set; }
        public int ShopPrice { get; set; }
        public EItemType? ItemType { get; set; }
    }

    // Insertion-ordered: the shop catalog order depends on it.
    public class StationRegistry
    {
        private readonly List<KeyValuePair<string, StationConfig>> ordered = new List<KeyValuePair<string, StationConfig>>();
        private readonly Dictionary<string, StationConfig> byId = new Dictionary<string, StationConfig>();

        public StationRegistry(IEnumerable<KeyValuePair<string, StationConfig>> configs)
        {
            foreach (var kvp in configs)
            {
                ordered.Add(kvp);
                byId[kvp.Key] = kvp.Value;
            }
        }

        public StationConfig Get(string stationId)
        {
            return byId.TryGetValue(stationId, out var config) ? config : null;
        }

        public IEnumerable<KeyValuePair<string, StationConfig>> GetAll()
        {
            return ordered;
        }
    }
}
