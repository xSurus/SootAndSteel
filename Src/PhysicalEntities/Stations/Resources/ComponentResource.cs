using Gamelab.Assets;
using System.Linq;
using Gamelab.Items.Bullets;
using Gamelab.Players;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gamelab.PhysicalEntities.Stations.Resources;

public class ComponentResource(Vector2 position, string componentId)
    : AbstractResource(
        ComponentRegistry.GetSprite(componentId),
        ComponentRegistry.GetColor(componentId),
        "Bullet",
        position)
{

    public string componentId = componentId;
    
    public override void OnPickup(Player interactingPlayer)
    {
        if (interactingPlayer.HeldItem == null)
        {
            interactingPlayer.HeldItem = new BulletItem(componentId);
        }
        else if (interactingPlayer.HeldItem.Id == ResourceId
                 && ((BulletItem)interactingPlayer.HeldItem).ComponentIds.SequenceEqual([componentId]))
        {
            interactingPlayer.HeldItem = null;
        }
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        Texture2D tex = AssetManager.GetStationTexture(componentId);
        int tileSize = GamelabGame.Instance.GameplayConfig.TrainTileSize;
        float originalSize = tex.Width;
        float scale = tileSize / originalSize;

        Vector2 drawingPos = Position + new Vector2(-tileSize * 0.5f, -tileSize * 1.5f);
        spriteBatch.Draw(tex, drawingPos, null, Color.White,
                                0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
    }
}

