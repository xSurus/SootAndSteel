using Gamelab.Assets;
using Gamelab.Items;
using Gamelab.PhysicalEntities.Interfaces;
using Gamelab.Players;
using Gamelab.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gamelab.PhysicalEntities.Structures;

public class CoalWagon : AbstractPhysicalEntity, IPickable
{
    private readonly float heightPixels;
    private readonly float widthPixels;

    private static readonly Logger logger = new("CoalWagon");

    public CoalWagon(Vector2 position)
    {
        heightPixels = GamelabGame.Instance.GameplayConfig.TrainTileSize *
                       GamelabGame.Instance.GameplayConfig.TrainHeight;
        widthPixels = GamelabGame.Instance.GameplayConfig.TrainTileSize * 6f;

        PhysicsBody = gameplayContext.PhysicsWorld.CreateRectangle(widthPixels.ToMeters(), heightPixels.ToMeters(), 1f,
            position.ToMeters());
        PhysicsBody.Tag = this;
    }

    public void OnPickup(Player interactingPlayer)
    {
        if (interactingPlayer.HeldItem == null && gameplayContext.State.CoalAmount > 0)
        {
            interactingPlayer.HeldItem = new Item("Coal");
            gameplayContext.State.ConsumeCoal(1);
        }
        else if (interactingPlayer.HeldItem != null && interactingPlayer.HeldItem.Id == "Coal")
        {
            interactingPlayer.HeldItem = null;
            gameplayContext.State.AddCoal(1);
        }
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        // logger.Info($"Drawing texture for Wagon");
        int tileSize = GamelabGame.Instance.GameplayConfig.TrainTileSize;
        // float scale = tileSize / AssetManager.TileTexture.Width;

        Vector2 drawPos = Position + new Vector2(-widthPixels / 2f + tileSize, -heightPixels / 2f - tileSize * 2.25f);
        spriteBatch.Draw(AssetManager.GetStructureTexture("CoalWagon"),
            drawPos, null, Color.White,
            0f, Vector2.Zero, 0.85f, SpriteEffects.None, 0f);


        // Rectangle destRect = new Rectangle((int)(Position.X - widthPixels / 2f), (int)(Position.Y - heightPixels / 2f),
        //     (int)widthPixels, (int)heightPixels);
        // spriteBatch.Draw(AssetManager., destRect, Color.Black);
    }
}