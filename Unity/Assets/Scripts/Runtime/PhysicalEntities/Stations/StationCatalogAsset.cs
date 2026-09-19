// Unity/Assets/Scripts/Runtime/PhysicalEntities/Stations/StationCatalogAsset.cs
using System;
using System.Collections.Generic;
using Gamelab.PhysicalEntities.Stations;
using UnityEngine;

namespace Gamelab.PhysicalEntities.Stations
{
    [Serializable]
    public class StationCatalogEntry
    {
        public string stationId;
        public string displayName;
        public string description;
        public StationRuntime prefab;
    }

    [CreateAssetMenu(menuName = "Gamelab/Stations/Station Catalog", fileName = "StationCatalog")]
    public class StationCatalogAsset : ScriptableObject
    {
        [SerializeField] private List<StationCatalogEntry> entries = new List<StationCatalogEntry>();

        public StationCatalogEntry Get(string stationId)
        {
            foreach (StationCatalogEntry entry in entries)
            {
                if (entry.stationId == stationId)
                {
                    return entry;
                }
            }

            throw new ArgumentOutOfRangeException(nameof(stationId), stationId, "Unknown station id in catalog.");
        }
    }
}
