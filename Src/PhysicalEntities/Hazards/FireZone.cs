using System.Collections.Generic;
using Gamelab.Assets;
using Gamelab.Enemies;
using Gamelab.Enemies.Hazards;
using Gamelab.Players;
using Gamelab.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using nkast.Aether.Physics2D.Dynamics;

namespace Gamelab.PhysicalEntities.Hazards;

public class FireZone : AbstractPhysicalEntity, IEnemyHazard
{
    private readonly float radius;
    private readonly float durationSeconds;
    private readonly Dictionary<Player, float> exposureTimes = [];
    private float remainingTime;

    public bool ShouldRemove { get; private set; }
    public bool CountsAsActiveThreat => false;

    public FireZone(Vector2 position, float radius, float durationSeconds)
    {
        this.radius = radius;
        this.durationSeconds = durationSeconds;
        remainingTime = durationSeconds;

        PhysicsBody =
            gameplayContext.PhysicsWorld.CreateCircle((radius / 2f).ToMeters(), 1f, position.ToMeters(),
                BodyType.Static);
        foreach (var fixture in PhysicsBody.FixtureList)
        {
            fixture.IsSensor = true;
        }
    }

    public void Update(float dt)
    {
        if (ShouldRemove)
        {
            return;
        }

        remainingTime -= dt;
        if (remainingTime <= 0f)
        {
            ShouldRemove = true;
            return;
        }

        HashSet<Player> insidePlayers = [];
        foreach (Body body in gameplayContext.PhysicsWorld.BodyList)
        {
            if (body.Tag is not Player player)
            {
                continue;
            }

            float distanceSquared = Vector2.DistanceSquared(player.Position, Position);
            if (distanceSquared > radius * radius)
            {
                continue;
            }

            insidePlayers.Add(player);
            float exposure = exposureTimes.TryGetValue(player, out float current) ? current + dt : dt;
            exposureTimes[player] = exposure;
            if (exposure >= GamelabGame.Instance.GameplayConfig.FireStunDurationSeconds)
            {
                player.Stun(GamelabGame.Instance.GameplayConfig.PlayerStunDurationSeconds);
            }
        }

        List<Player> toReset = [];
        foreach ((Player player, _) in exposureTimes)
        {
            if (!insidePlayers.Contains(player))
            {
                toReset.Add(player);
            }
        }

        foreach (Player player in toReset)
        {
            exposureTimes.Remove(player);
        }
    }

    public IEnemyHazard TryCreateHazard()
    {
        return null;
    }

    public void RemovePhysicsBody()
    {
        if (PhysicsBody == null)
        {
            return;
        }

        PhysicsBody.World.Remove(PhysicsBody);
        PhysicsBody = null;
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        float alpha = MathHelper.Clamp(remainingTime / durationSeconds, 0.25f, 1f);
        int diameter = (int)(radius * 2f);

        Vector2 feetPosition = Position + new Vector2(0, radius);
        float depth = RenderUtility.CalculateDepth(feetPosition.Y);

        Vector2 origin = new Vector2(0.5f, 1f);

        spriteBatch.Draw(
            AssetManager.BlankTexture,
            feetPosition,
            null,
            Color.OrangeRed * alpha,
            0f,
            origin,
            new Vector2(diameter, diameter),
            SpriteEffects.None,
            depth
        );
    }
}