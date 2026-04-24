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

public class AnchorCable : AbstractPhysicalEntity, IEnemyHazard, IInteractable
{
    private readonly EnemyTrainSlot slot;
    private readonly float distanceFromTrain;
    private readonly float cutDurationSeconds;
    private bool anchorApplied;
    private float cutProgress;

    public bool ShouldRemove { get; private set; }
    public bool CountsAsActiveThreat => !ShouldRemove;

    public AnchorCable(EnemyTrainSlot slot, float distanceFromTrain,
        float cutDurationSeconds)
    {
        this.slot = slot;
        this.distanceFromTrain = distanceFromTrain;
        this.cutDurationSeconds = cutDurationSeconds;

        Vector2 anchorPosition = slot.GetAnchor(distanceFromTrain);
        PhysicsBody =
            gameplayContext.PhysicsWorld.CreateRectangle(0.5f, 0.5f, 1f, anchorPosition.ToMeters(), 0f,
                BodyType.Static);
        foreach (var fixture in PhysicsBody.FixtureList)
        {
            fixture.IsSensor = true;
        }

        ApplyAnchorEffect();
    }

    public void Update(float dt)
    {
        Position = slot.GetAnchor(distanceFromTrain);
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

        cutProgress += dt / Math.Max(0.01f, cutDurationSeconds);
        if (cutProgress >= 1f)
        {
            RemoveAnchorEffect();
            ShouldRemove = true;
        }
    }

    public IEnemyHazard TryCreateHazard()
    {
        return null;
    }

    public void RemovePhysicsBody()
    {
        RemoveAnchorEffect();
        if (PhysicsBody == null)
        {
            return;
        }

        PhysicsBody.World.Remove(PhysicsBody);
        PhysicsBody = null;
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        Vector2 anchor = Position;
        Vector2 groundPoint = slot.Side == EnemySlotSide.Top
            ? new Vector2(anchor.X - 80f, 0f)
            : new Vector2(anchor.X - 80f, gameplayContext.ScreenHeight);

        Vector2 feetPosition = anchor + new Vector2(0, 14f);
        float depth = RenderUtility.CalculateDepth(feetPosition.Y);
        DrawLine(spriteBatch, groundPoint, anchor, Color.SaddleBrown, 4, depth);
        Vector2 boxOrigin = new Vector2(0.5f, 1f);
        spriteBatch.Draw(AssetManager.BlankTexture, feetPosition, null, Color.Brown, 0f, boxOrigin,
            new Vector2(28f, 28f), SpriteEffects.None, depth + RenderUtility.Eps);

        if (cutProgress > 0f)
        {
            Rectangle bg = new((int)(anchor.X - 20f), (int)(anchor.Y + 20f), 40, 6);
            Rectangle fill = new(bg.X, bg.Y, (int)(40 * Math.Clamp(cutProgress, 0f, 1f)), 6);

            spriteBatch.Draw(AssetManager.BlankTexture, bg, null, Color.Black, 0f, Vector2.Zero, SpriteEffects.None,
                depth + 2 * RenderUtility.Eps);
            spriteBatch.Draw(AssetManager.BlankTexture, fill, null, Color.LightGreen, 0f, Vector2.Zero,
                SpriteEffects.None, depth + 3 * RenderUtility.Eps);
        }
    }

    private static void DrawLine(SpriteBatch spriteBatch, Vector2 start, Vector2 end, Color color, int thickness,
        float depth)
    {
        Vector2 edge = end - start;
        float angle = (float)Math.Atan2(edge.Y, edge.X);
        spriteBatch.Draw(
            AssetManager.BlankTexture,
            new Rectangle((int)start.X, (int)start.Y, (int)edge.Length(), thickness),
            null,
            color,
            angle,
            new Vector2(0f, 0.5f),
            SpriteEffects.None,
            depth
        );
    }

    private void ApplyAnchorEffect()
    {
        if (anchorApplied)
        {
            return;
        }

        gameplayContext.State.AddAnchor();
        anchorApplied = true;
    }

    private void RemoveAnchorEffect()
    {
        if (!anchorApplied)
        {
            return;
        }

        gameplayContext.State.RemoveAnchor();
        anchorApplied = false;
    }
}