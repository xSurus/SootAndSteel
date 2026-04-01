using Gamelab.Assets;
using Gamelab.Players;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using nkast.Aether.Physics2D.Dynamics;
using nkast.Aether.Physics2D.Dynamics.Joints;

namespace Gamelab.PhysicalEntities.Stations.Cannon;

public class CannonAimingBar : AbstractGrabbable
{
    protected override bool AllowPlayerRotation { get; } = true;

    public CannonAimingBar(Body cannonBaseBody, Vector2 anchorMeters)
    {
        // TODO only example asset
        PhysicsBody = gameplayContext.PhysicsWorld.CreateRectangle(1.5f, 0.3f, 1f, anchorMeters, 0f, BodyType.Dynamic);
        PhysicsBody.Tag = this;
        PhysicsBody.LinearDamping = GamelabGame.Instance.GameplayConfig.GrabbableLinearDamping;

        foreach (var fixture in PhysicsBody.FixtureList)
        {
            fixture.IsSensor = true;
        }

        RevoluteJoint joint = JointFactory.CreateRevoluteJoint(gameplayContext.PhysicsWorld, cannonBaseBody,
            PhysicsBody, Vector2.Zero);
        joint.MotorEnabled = true;
        joint.MotorSpeed = 0f;
        joint.MaxMotorTorque = GamelabGame.Instance.GameplayConfig.GrabbableRotationalResistance;
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        if (PhysicsBody == null) return;

        int tileSize = GamelabGame.Instance.GameplayConfig.TrainTileSize;
        Vector2 barCenterPixels = Position;

        float barPixelWidth = 1.5f * tileSize;
        float barPixelHeight = 0.3f * tileSize;

        Texture2D texture = AssetManager.BlankTexture;
        Vector2 scale = new Vector2(barPixelWidth / texture.Width, barPixelHeight / texture.Height);
        Vector2 origin = new Vector2(texture.Width / 2f, texture.Height / 2f);

        spriteBatch.Draw(
            texture: texture,
            position: barCenterPixels,
            sourceRectangle: null,
            color: Color.DarkGray,
            rotation: PhysicsBody.Rotation,
            origin: origin,
            scale: scale,
            effects: SpriteEffects.None,
            layerDepth: 0f
        );
    }

    protected override void OnLastRelease(Player interactingPlayer)
    {
        PhysicsBody.BodyType = BodyType.Dynamic;
    }
}