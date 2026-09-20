using System.Collections.Generic;
using Gamelab.Enemies;
using Gamelab.PhysicalEntities.Interfaces;
using UnityEngine;
using NVector2 = System.Numerics.Vector2;

namespace Gamelab.PhysicalEntities.Bullets
{
    /// <summary>
    /// Port of Src BulletTargetingHelper. Src scans the physics World body list; Unity has none, so
    /// live IDamageable MonoBehaviours register themselves here (OnEnable/OnDisable).
    /// Targets are returned as their Rigidbody2D, positions are in meters.
    /// </summary>
    public static class BulletTargeting
    {
        private static readonly List<MonoBehaviour> registry = new List<MonoBehaviour>();

        public static void Register<T>(T target) where T : MonoBehaviour, IDamageable
        {
            if (!registry.Contains(target)) registry.Add(target);
        }

        public static void Unregister<T>(T target) where T : MonoBehaviour, IDamageable => registry.Remove(target);

        public static List<Rigidbody2D> GetHostileTargets(BulletRuntime bullet, IDamageable excludedTarget = null)
        {
            var result = new List<Rigidbody2D>();
            foreach (MonoBehaviour mb in registry)
            {
                if (mb == null || !mb.isActiveAndEnabled) continue;
                if (excludedTarget != null && ReferenceEquals(mb, excludedTarget)) continue;
                if (!IsHostile(bullet.Faction, mb)) continue;
                Rigidbody2D body = mb.GetComponent<Rigidbody2D>();
                if (body == null || !body.simulated || body == bullet.PhysicsBody) continue;
                result.Add(body);
            }

            return result;
        }

        public static bool IsHostile(BulletFaction faction, object target)
        {
            switch (faction)
            {
                case BulletFaction.Player: return target is EnemyRuntime;
                case BulletFaction.Enemy: return target is IDamageable && !(target is EnemyRuntime);
                default: return false;
            }
        }

        public static Rigidbody2D GetClosestHostileTarget(BulletRuntime bullet, IDamageable excludedTarget = null)
        {
            Rigidbody2D best = null;
            float bestDist = float.MaxValue;
            foreach (Rigidbody2D body in GetHostileTargets(bullet, excludedTarget))
            {
                float d = (body.position - bullet.PhysicsBody.position).sqrMagnitude;
                if (d < bestDist)
                {
                    bestDist = d;
                    best = body;
                }
            }

            return best;
        }

        // Src: Player faction -> enemies 0; Enemy faction -> Player 0, ShootHoleWall 1, other IDamageable 2.
        // Unity has no Player or ShootHoleWall damageable yet (later waves). When they exist, add
        // their cases (Player 0, ShootHoleWall 1) here and in GetHomingTargetDotThreshold.
        public static int GetHomingTargetPriority(BulletFaction faction, object target)
        {
            if (faction == BulletFaction.Player && target is EnemyRuntime) return 0;
            if (faction == BulletFaction.Enemy && target is IDamageable && !(target is EnemyRuntime)) return 2;
            return int.MaxValue;
        }

        // Src: -0.15 when an Enemy bullet homes on the Player, else 0.5. Player does not exist yet.
        public static float GetHomingTargetDotThreshold(BulletFaction faction, object target) => 0.5f;

        /// <summary>The IDamageable behind a target body (Src body.Tag).</summary>
        public static object TagOf(Rigidbody2D body) => body.GetComponentInParent<IDamageable>();

        public static Rigidbody2D GetClosestHomingTargetToLine(
            BulletRuntime bullet, IEnumerable<Rigidbody2D> candidates, NVector2 lineStart, NVector2 lineEnd)
        {
            Rigidbody2D best = null;
            int bestPriority = int.MaxValue;
            float bestDist = float.MaxValue;
            foreach (Rigidbody2D body in candidates)
            {
                int priority = GetHomingTargetPriority(bullet.Faction, TagOf(body));
                float dist = BulletTargetingMath.GetDistanceToLine(
                    lineStart, lineEnd, new NVector2(body.position.x, body.position.y));
                if (best == null || priority < bestPriority || (priority == bestPriority && dist < bestDist))
                {
                    best = body;
                    bestPriority = priority;
                    bestDist = dist;
                }
            }

            return best;
        }
    }
}
