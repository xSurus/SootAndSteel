using System;
using Gamelab.Assets;
using Gamelab.Enemies;
using Gamelab.Enemies.Hazards;
using Gamelab.Enemies.Slots;
using Gamelab.PhysicalEntities.Interfaces;
using Gamelab.Players;
using Gamelab.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using nkast.Aether.Physics2D.Dynamics;

namespace Gamelab.PhysicalEntities.Hazards;

public class TarPatch : AbstractPhysicalEntity, IEnemyHazard, IInteractable
{
    private readonly EnemySlotSide side;
    private readonly float cleanDurationSeconds;
    private float cleanProgress;

    public bool ShouldRemove { get; private set; }
    public bool CountsAsActiveThreat => false;

    public TarPatch(EnemySlotSide side, float cleanDurationSeconds)
    {
        this.side = side;
        this.cleanDurationSeconds = cleanDurationSeconds;

        Vector2 position = GetPatchCenter();
        PhysicsBody =
            gameplayContext.PhysicsWorld.CreateRectangle(0.5f, 0.5f, 1f, position.ToMeters(), 0f, BodyType.Static);
        foreach (var fixture in PhysicsBody.FixtureList)
        {
            fixture.IsSensor = true;
        }
    }

    public void Update(float dt)
    {
        Position = GetPatchCenter();
    }

    public void OnInteract(Player interactingPlayer)
    {
    }

    public void OnInteractHeld(Player interactingPlayer, float dt)
    {
        if (ShouldRemove)
        {
            return;
        }

        cleanProgress += dt / Math.Max(0.01f, cleanDurationSeconds);
        if (cleanProgress >= 1f)
        {
            ShouldRemove = true;
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
        Rectangle bounds = gameplayContext.Map.GetBounds();
        Rectangle overlay = side == EnemySlotSide.Top
            ? new Rectangle(bounds.Left, bounds.Top - 80, bounds.Width, bounds.Height / 2)
            : new Rectangle(bounds.Left, bounds.Center.Y, bounds.Width, bounds.Height / 2 + 80);

        spriteBatch.Draw(AssetManager.BlankTexture, overlay, null, new Color(20, 20, 20, 220), 0f, Vector2.Zero,
            SpriteEffects.None, 0.01f);

        Vector2 center = GetPatchCenter();
        Vector2 centerBottom = center + new Vector2(0, 20f);
        float depth = RenderUtility.CalculateDepth(centerBottom.Y);

        Vector2 origin = new Vector2(0.5f, 1f);
        spriteBatch.Draw(AssetManager.BlankTexture, centerBottom, null, Color.Black, 0f, origin, new Vector2(60f, 40f),
            SpriteEffects.None, depth);
        if (cleanProgress > 0f)
        {
            Rectangle bg = new((int)(center.X - 20f), (int)(center.Y + 24f), 40, 6);
            Rectangle fill = new(bg.X, bg.Y, (int)(40 * Math.Clamp(cleanProgress, 0f, 1f)), 6);

            spriteBatch.Draw(AssetManager.BlankTexture, bg, null, Color.DarkGray, 0f, Vector2.Zero, SpriteEffects.None,
                depth + RenderUtility.Eps);
            spriteBatch.Draw(AssetManager.BlankTexture, fill, null, Color.LightBlue, 0f, Vector2.Zero,
                SpriteEffects.None, depth + 2 * RenderUtility.Eps);
        }
    }

    private Vector2 GetPatchCenter()
    {
        Rectangle bounds = gameplayContext.Map.GetBounds();
        return side == EnemySlotSide.Top
            ? new Vector2(bounds.Center.X, bounds.Top - 30f)
            : new Vector2(bounds.Center.X, bounds.Bottom + 30f);
    }
}