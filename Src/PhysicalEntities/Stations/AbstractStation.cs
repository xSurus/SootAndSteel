using Gamelab.Assets;
using Gamelab.Items;
using Gamelab.Players;
using Gamelab.Utils;
using Gamelab.Utils.Logging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gamelab.PhysicalEntities.Stations;

public abstract class AbstractStation : AbstractGrabbable, IInteractable, IPickable, IUpdatable
{
    
    public string Type { get; protected set; }

    // TODO swap to a texture instead of display color at some point
    public Color DisplayColor { get; protected set; }
    public Item HeldItem { get; set; }
    public Vector2 DrawPosition => Position - new Vector2(GamelabGame.Instance.GameplayConfig.TrainTileSize / 2f);
    protected override bool AllowPlayerRotation { get; } = false;

    protected AbstractStation(string type, Color displayColor, Vector2 position)
    {
        Type = type;
        DisplayColor = displayColor;
        float collisionSizePixels = GamelabGame.Instance.GameplayConfig.TrainTileSize * 0.90f;
        float simSize = collisionSizePixels.ToMeters();
        PhysicsBody = gameplayContext.PhysicsWorld.CreateRectangle(simSize, simSize, 1f, position.ToMeters());
    }

    public virtual void Update(float dt)
    {
    }

    public virtual void OnInteract(Player interactingPlayer)
    {
    }

    public virtual void OnInteractHeld(Player interactingPlayer, float dt)
    {
    }

    public virtual void OnPickup(Player interactingPlayer)
    {
    }

    public virtual void OnPickupHeld(Player interactingPlayer, float dt)
    {
    }

    protected override void OnLastRelease(Player interactingPlayer)
    {
        base.OnLastRelease(interactingPlayer);
        gameplayContext.Map?.SnapToNearestValidCell(this);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        Texture2D tex = AssetManager.GetStationTexture(Type);
        
        

        if (tex == AssetManager.BlankTexture){
            int tileSize = GamelabGame.Instance.GameplayConfig.TrainTileSize;
            int drawSize = tileSize - 10;

            Vector2 origin = new Vector2(drawSize / 2f, drawSize / 2f);
            Rectangle sourceRect = new Rectangle(0, 0, drawSize, drawSize);

            spriteBatch.Draw(
                texture: AssetManager.BlankTexture,
                position: Position,
                sourceRectangle: sourceRect,
                color: DisplayColor,
                rotation: PhysicsBody.Rotation,
                origin: origin,
                scale: 1f,
                effects: SpriteEffects.None,
                layerDepth: 0f
            );

            HeldItem?.Draw(spriteBatch, Position, tileSize / 2);
        } else {
            
            int tileSize = GamelabGame.Instance.GameplayConfig.TrainTileSize;
            float originalSize = tex.Width;
            float scale = tileSize / originalSize;

            Vector2 drawingPos = Position + new Vector2(-tileSize * 0.5f, -tileSize * 1.5f);
            spriteBatch.Draw(tex, drawingPos, null, Color.White,
                                    0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        }
    }
}