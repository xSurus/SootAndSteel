using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace Gamelab.PhysicalEntities.Projectiles;

public class ProjectileManager
{
    private readonly List<AbstractProjectile> projectiles = [];

    public void Add(AbstractProjectile projectile)
    {
        if (projectile == null)
        {
            return;
        }

        projectiles.Add(projectile);
    }

    public void Update(float deltaTime)
    {
        for (int i = projectiles.Count - 1; i >= 0; i--)
        {
            AbstractProjectile projectile = projectiles[i];
            projectile.Update(deltaTime);

            if (!projectile.IsActive)
            {
                projectile.RemovePhysicsBody();
                projectiles.RemoveAt(i);
            }
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        foreach (AbstractProjectile projectile in projectiles)
        {
            projectile.Draw(spriteBatch);
        }
    }

    public void Clear()
    {
        foreach (AbstractProjectile projectile in projectiles)
        {
            projectile.Deactivate();
            projectile.RemovePhysicsBody();
        }

        projectiles.Clear();
    }
}
