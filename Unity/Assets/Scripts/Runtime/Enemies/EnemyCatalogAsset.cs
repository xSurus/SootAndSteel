using System;
using System.Collections.Generic;
using Gamelab.Enemies.Core;
using UnityEngine;

namespace Gamelab.Enemies
{
    [Serializable]
    public class EnemyCatalogEntry
    {
        public EnemyType type;
        public float health = 100f; // Src/Config/GameplayConfig.cs EnemyHealth default
        public float size = 72f;    // Src/Config/GameplayConfig.cs EnemySize default
        public EnemyRuntime prefab;
    }

    [CreateAssetMenu(menuName = "Gamelab/Enemies/Enemy Catalog", fileName = "EnemyCatalog")]
    public class EnemyCatalogAsset : ScriptableObject
    {
        [SerializeField] private List<EnemyCatalogEntry> entries = new List<EnemyCatalogEntry>();

        public EnemyCatalogEntry Get(EnemyType type)
        {
            foreach (EnemyCatalogEntry entry in entries)
            {
                if (entry.type == type)
                {
                    return entry;
                }
            }

            throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown enemy type in catalog.");
        }
    }
}
