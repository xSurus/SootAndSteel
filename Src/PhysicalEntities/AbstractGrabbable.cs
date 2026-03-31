using System.Collections.Generic;
using Gamelab.Map.Train.State;
using Gamelab.Players;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using nkast.Aether.Physics2D.Dynamics;
using nkast.Aether.Physics2D.Dynamics.Joints;

namespace Gamelab.PhysicalEntities;

public abstract class AbstractGrabbable : AbstractPhysicalEntity, IGrabbable
{
    protected Dictionary<Player, WeldJoint> GrabJoints { get; } = new();

    protected abstract bool AllowPlayerRotation { get; }

    public virtual bool OnGrab(Player interactingPlayer, GameplayContext gameplayContext, Vector2 grabPointWorldMeters)
    {
        if (GrabJoints.ContainsKey(interactingPlayer)) return false;
        if (GrabJoints.Count == 0) OnFirstGrab(interactingPlayer, gameplayContext);

        interactingPlayer.PhysicsBody.FixedRotation = false;
        Vector2 playerLocalAnchor = interactingPlayer.PhysicsBody.GetLocalPoint(grabPointWorldMeters);
        Vector2 stationLocalAnchor = PhysicsBody.GetLocalPoint(grabPointWorldMeters);

        WeldJoint joint = JointFactory.CreateWeldJoint(gameplayContext.Map.PhysicsWorld, interactingPlayer.PhysicsBody,
            PhysicsBody, playerLocalAnchor, stationLocalAnchor);
        GrabJoints.Add(interactingPlayer, joint);

        interactingPlayer.PhysicsBody.FixedRotation = !AllowPlayerRotation;

        return true;
    }

    protected virtual void OnFirstGrab(Player interactingPlayer, GameplayContext gameplayContext)
    {
        // make object dynamic
        PhysicsBody.BodyType = BodyType.Dynamic;
        PhysicsBody.LinearDamping = GamelabGame.Instance.GameplayConfig.GrabbableLinearDamping;
        PhysicsBody.FixedRotation = false;
    }

    public virtual void OnRelease(Player interactingPlayer, GameplayContext gameplayContext)
    {
        if (!GrabJoints.TryGetValue(interactingPlayer, out WeldJoint joint)) return;
        gameplayContext.Map.PhysicsWorld.Remove(joint);
        GrabJoints.Remove(interactingPlayer);
        interactingPlayer.PhysicsBody.FixedRotation = true;
        if (GrabJoints.Count == 0) OnLastRelease(interactingPlayer, gameplayContext);
    }

    protected virtual void OnLastRelease(Player interactingPlayer, GameplayContext gameplayContext)
    {
        PhysicsBody.BodyType = BodyType.Static;
    }

    public abstract override void Draw(SpriteBatch spriteBatch);
}