using Gamelab.Assets;
using Gamelab.Items;
using Gamelab.Map.Train.State;
using Gamelab.Players;
using Gamelab.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gamelab.PhysicalEntities;

public class CoalWagon : AbstractPhysicalEntity, IPickable
{
    private readonly GameplayContext gameplayContext = GamelabGame.Instance.Services.GetService<GameplayContext>();
    private readonly float heightPixels;
    private readonly float widthPixels;

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
        Rectangle destRect = new Rectangle((int)(Position.X - widthPixels / 2f), (int)(Position.Y - heightPixels / 2f),
            (int)widthPixels, (int)heightPixels);
        spriteBatch.Draw(AssetManager.BlankTexture, destRect, Color.Black);
    }
}