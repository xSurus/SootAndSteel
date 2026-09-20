using Gamelab.Items.Bullets;
using Gamelab.Services.Random;
using UnityEngine;
using NVector2 = System.Numerics.Vector2;

namespace Gamelab.PhysicalEntities.Bullets.Propellants
{
    [CreateAssetMenu(menuName = "Gamelab/Bullets/Propellants/Homing Propellant", fileName = "HomingPropellant")]
    public class HomingPropellantAsset : BulletComponentAsset
    {
        public override EComponentType Type => EComponentType.Propellant;
        public override string ComponentId => ComponentIds.HomingPropellant;

        public class HomingState
        {
            public float Timer;
            public float InitialLockOnDelay = 0.2f;
            public Rigidbody2D LockedTarget;
            public Vector2 LockOnOffset; // meters
            public bool HasSpedUp;
        }

        private const float LockOnInterval = 0.3f;
        private const float MaxRotation = 2.8f;
        private static readonly IRandomService RandomService = Gamelab.Services.Random.RandomService.Shared;

        public override void OnCreate(BulletRuntime bullet)
        {
            bullet.Stats.Speed *= 0.5f;
            bullet.Stats.Spread *= 1.5f;
            var state = bullet.GetState<HomingState>(this);
            state.InitialLockOnDelay += RandomService.NextSingle() / 2f;
            state.LockOnOffset = new Vector2(
                WorldUnits.ToMeters(RandomService.NextSingle() * 50f - 25f),
                WorldUnits.ToMeters(RandomService.NextSingle() * 50f - 25f));
        }

        public override void OnUpdate(BulletRuntime bullet, float deltaTime)
        {
            var state = bullet.GetState<HomingState>(this);
            state.Timer += deltaTime;

            if (state.Timer >= state.InitialLockOnDelay + LockOnInterval)
            {
                state.LockedTarget = LockOnTarget(bullet);
                state.Timer -= LockOnInterval;
            }

            Rigidbody2D body = bullet.PhysicsBody;
            Vector2 velocity = body.linearVelocity;
            float angle = Mathf.Atan2(velocity.y, velocity.x);
            float maxRotationThisFrame = MaxRotation * deltaTime;
            float speed = velocity.magnitude;
            if (state.LockedTarget != null) // Unity null: also false once the target is destroyed
            {
                Vector2 desired = state.LockedTarget.position + state.LockOnOffset - body.position;
                float targetAngle = Mathf.Atan2(desired.y, desired.x);
                float diff = WrapAngle(targetAngle - angle);
                angle += Mathf.Clamp(diff, -maxRotationThisFrame, maxRotationThisFrame);

                if (!state.HasSpedUp)
                {
                    speed *= 1.7f;
                    state.HasSpedUp = true;
                }
            }

            body.linearVelocity = new Vector2(Mathf.Cos(angle) * speed, Mathf.Sin(angle) * speed);
            // Src OnDraw draws a debug indicator (two lines showing lock state). Draw-only, dropped.
        }

        private static float WrapAngle(float a) => Mathf.Repeat(a + Mathf.PI, 2f * Mathf.PI) - Mathf.PI;

        private static Rigidbody2D LockOnTarget(BulletRuntime bullet)
        {
            Vector2 pos = bullet.PhysicsBody.position;
            Vector2 velocity = bullet.PhysicsBody.linearVelocity;
            if (velocity.sqrMagnitude < 0.01f) return null;
            Vector2 forward = velocity.normalized;

            var inArc = new System.Collections.Generic.List<Rigidbody2D>();
            foreach (Rigidbody2D target in BulletTargeting.GetHostileTargets(bullet))
            {
                if (IsWithinHomingArc(bullet, pos, forward, target)) inArc.Add(target);
            }

            if (inArc.Count == 0) return null;
            Vector2 end = pos + forward * 1000f;
            return BulletTargeting.GetClosestHomingTargetToLine(
                bullet, inArc, new NVector2(pos.x, pos.y), new NVector2(end.x, end.y));
        }

        private static bool IsWithinHomingArc(BulletRuntime bullet, Vector2 position, Vector2 forward, Rigidbody2D target)
        {
            Vector2 toTarget = target.position - position;
            if (toTarget.sqrMagnitude < 0.0001f) return true;
            float minDot = BulletTargeting.GetHomingTargetDotThreshold(bullet.Faction, BulletTargeting.TagOf(target));
            return Vector2.Dot(forward, toTarget.normalized) >= minDot;
        }
    }
}
