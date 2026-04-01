using Gamelab.Items.Crafting;
using Microsoft.Xna.Framework;

namespace Gamelab.PhysicalEntities.Stations.Workbenches;

public class Anvil : AbstractWorkbench
{
    public Anvil(Vector2 position) : base("Anvil", Color.DarkSlateGray, position)
    {
        ValidRecipes.Add(new Recipe("HammeredCopper", 2.0f, "Copper"));
        ValidRecipes.Add(new Recipe("Core", 2.0f, "HammeredCopper"));
        ValidRecipes.Add(new Recipe("CoredHammeredCopper", 3.0f, "Copper", "Core"));

        ValidRecipes.Add(new Recipe("StandardBullet", 1.5f, "HammeredCopper", "Gunpowder"));
        ValidRecipes.Add(new Recipe("HeavyBullet", 2.5f, "CoredHammeredCopper", "Gunpowder"));
    }
}