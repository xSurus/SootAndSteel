using Gamelab.Assets;
using Gamelab.Enemies;
using Gamelab.Map.Train.State;
using Gamelab.PhysicalEntities;
using Gamelab.Players;
using Gamelab.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using nkast.Aether.Physics2D.Dynamics;

namespace Gamelab.PhysicalEntities.Hazards;

public class TarPatch : AbstractPhysicalEntity, IEnemyHazard, IInteractable
{
    private readonly GameplayContext gameplayContext;
    private readonly EnemySlotSide side;
    private readonly float cleanDurationSeconds;
    private float cleanProgress;

    public bool ShouldRemove { get; private set; }
    public bool CountsAsActiveThreat => false;

    public TarPatch(GameplayContext gameplayContext, EnemySlotSide side, float cleanDurationSeconds)
    {
        this.gameplayContext = gameplayContext;
        this.side = side;
        this.cleanDurationSeconds = cleanDurationSeconds;

        Vector2 position = GetPatchCenter();
        PhysicsBody = gameplayContext.PhysicsWorld.CreateRectangle(0.5f, 0.5f, 1f, position.ToMeters(), 0f, BodyType.Static);
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

        cleanProgress += dt / System.Math.Max(0.01f, cleanDurationSeconds);
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
        spriteBatch.Draw(AssetManager.BlankTexture, overlay, new Color(20, 20, 20, 220));

        Vector2 center = GetPatchCenter();
        Rectangle blob = new((int)(center.X - 30f), (int)(center.Y - 20f), 60, 40);
        spriteBatch.Draw(AssetManager.BlankTexture, blob, Color.Black);

        if (cleanProgress > 0f)
        {
            Rectangle bg = new((int)(center.X - 20f), (int)(center.Y + 24f), 40, 6);
            Rectangle fill = new(bg.X, bg.Y, (int)(40 * System.Math.Clamp(cleanProgress, 0f, 1f)), 6);
            spriteBatch.Draw(AssetManager.BlankTexture, bg, Color.DarkGray);
            spriteBatch.Draw(AssetManager.BlankTexture, fill, Color.LightBlue);
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
