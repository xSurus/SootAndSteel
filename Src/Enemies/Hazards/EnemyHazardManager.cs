using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace Gamelab.Enemies.Hazards;

public class EnemyHazardManager
{
    private readonly List<IEnemyHazard> hazards = [];

    public bool HasActiveThreats
    {
        get
        {
            foreach (IEnemyHazard hazard in hazards)
            {
                if (hazard.CountsAsActiveThreat)
                {
                    return true;
                }
            }

            return false;
        }
    }

    public void Add(IEnemyHazard hazard)
    {
        if (hazard == null)
        {
            return;
        }

        hazards.Add(hazard);
    }

    public void Update(float deltaTime)
    {
        for (int i = hazards.Count - 1; i >= 0; i--)
        {
            IEnemyHazard hazard = hazards[i];
            hazard.Update(deltaTime);

            IEnemyHazard spawnedHazard = hazard.TryCreateHazard();
            if (spawnedHazard != null)
            {
                hazards.Add(spawnedHazard);
            }

            if (!hazard.ShouldRemove)
            {
                continue;
            }

            hazard.RemovePhysicsBody();
            hazards.RemoveAt(i);
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        foreach (IEnemyHazard hazard in hazards)
        {
            hazard.Draw(spriteBatch);
        }
    }

    public void Clear()
    {
        foreach (IEnemyHazard hazard in hazards)
        {
            hazard.RemovePhysicsBody();
        }

        hazards.Clear();
    }
}
