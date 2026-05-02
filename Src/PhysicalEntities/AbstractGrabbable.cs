using System.Collections.Generic;
using Gamelab.PhysicalEntities.Interfaces;
using Gamelab.Players;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using nkast.Aether.Physics2D.Dynamics;
using nkast.Aether.Physics2D.Dynamics.Joints;

namespace Gamelab.PhysicalEntities;

public abstract class AbstractGrabbable : AbstractPhysicalEntity, IGrabbable
{
    protected Dictionary<Player, WeldJoint> GrabJoints { get; } = new();
    public bool IsBeingHeld => GrabJoints.Count > 0;

    protected abstract bool AllowPlayerRotation { get; }

    public virtual bool OnGrab(Player interactingPlayer, Vector2 grabPointWorldMeters)
    {
        if (GrabJoints.ContainsKey(interactingPlayer)) return false;
        if (GrabJoints.Count == 0) OnFirstGrab(interactingPlayer);

        interactingPlayer.PhysicsBody.FixedRotation = false;
        Vector2 playerLocalAnchor = interactingPlayer.PhysicsBody.GetLocalPoint(grabPointWorldMeters);
        Vector2 stationLocalAnchor = PhysicsBody.GetLocalPoint(grabPointWorldMeters);

        WeldJoint joint = JointFactory.CreateWeldJoint(gameplayContext.PhysicsWorld, interactingPlayer.PhysicsBody,
            PhysicsBody, playerLocalAnchor, stationLocalAnchor);
        GrabJoints.Add(interactingPlayer, joint);

        interactingPlayer.PhysicsBody.FixedRotation = !AllowPlayerRotation;

        return true;
    }

    protected virtual void OnFirstGrab(Player interactingPlayer)
    {
        // make object dynamic
        PhysicsBody.BodyType = BodyType.Dynamic;
        PhysicsBody.LinearDamping = GamelabGame.Instance.GameplayConfig.GrabbableLinearDamping;
        PhysicsBody.FixedRotation = false;
    }

    public virtual void OnRelease(Player interactingPlayer)
    {
        if (!GrabJoints.TryGetValue(interactingPlayer, out WeldJoint joint)) return;
        GrabJoints.Remove(interactingPlayer);
        interactingPlayer.PhysicsBody.FixedRotation = true;
        bool wasLastHolder = GrabJoints.Count == 0;
        gameplayContext.DeferOrExecutePhysicsAction(() =>
        {
            gameplayContext.PhysicsWorld.Remove(joint);
            if (wasLastHolder) OnLastRelease(interactingPlayer);
        });
    }

    protected virtual void OnLastRelease(Player interactingPlayer)
    {
        PhysicsBody.BodyType = BodyType.Static;
    }

    public abstract override void Draw(SpriteBatch spriteBatch);
}