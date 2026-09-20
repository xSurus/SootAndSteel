using System;
using Random = System.Random;
using Gamelab.Config;
using Gamelab.Enemies.Core;
using Gamelab.Enemies.Movement;
using Gamelab.PhysicalEntities.Bullets;
using UnityEngine;
using NVector2 = System.Numerics.Vector2;

namespace Gamelab.Enemies
{
    // Port of Src/Enemies/Types/Enemy.cs. State and timers live in RifleEnemyBrain.
    // Dropped presentation: horse/rider animation and draw, horse riding/flee/fire sounds,
    // neck bleed emitter and blood splatter, FeetPosition (footprints only), EnemyMovementController train-frame drift, and
    // the "InitialShooter is CannonStation or CannonSlot" check (Faction.Player instead, as
    // EnemyRuntime.OnHit). Y convention is Src's: pixel space is y-down, so the Top side has
    // the smaller Y (the approach anchor for Top is above the slot anchor: -tile).
    public class RifleEnemyRuntime : EnemyRuntime
    {
        private EnemyTuning tuning;
        private EnemyAmmoDefinition ammoDefinition;
        private EnemyTrainSlot slot;
        private IEnemyWorld world;
        private IBulletItemSpawner spawner;
        private Random rng;
        private RifleEnemyBrain brain;
        private EnemyMovementController movement;

        protected virtual float ShootCooldown => tuning.EnemyShootCooldown;
        protected virtual void ApplyStartingHealth() { }
        protected EnemyTuning Tuning => tuning;

        public override bool WasNeutralized => base.WasNeutralized || (brain != null && brain.WasNeutralized);
        public HorseState Horse => brain.Horse;
        public RiderState Rider => brain.Rider;

        /// <summary>Call after Initialize (needs the Rigidbody2D and size).</summary>
        // Health and size come from the EnemyCatalogEntry given to Initialize (Src reads them from
        // GameplayConfig; the catalog defaults carry the same values), so tuning.EnemyHealth and
        // tuning.EnemySize are unused. Only the tutorial health is taken from tuning.
        public void Configure(EnemyTuning tuning, EnemyAmmoDefinition ammoDefinition, EnemyTrainSlot slot,
            IEnemyWorld world, IBulletItemSpawner spawner, Random rng)
        {
            if (PhysicsBody == null) throw new InvalidOperationException("Configure must run after Initialize.");
            this.tuning = tuning ?? throw new ArgumentNullException(nameof(tuning));
            this.ammoDefinition = ammoDefinition ?? throw new ArgumentNullException(nameof(ammoDefinition));
            this.world = world ?? throw new ArgumentNullException(nameof(world));
            this.spawner = spawner ?? throw new ArgumentNullException(nameof(spawner));
            this.rng = rng ?? throw new ArgumentNullException(nameof(rng));
            this.slot = slot;
            Bounds = world;
            movement = new EnemyMovementController(PhysicsBody, EnemyMovementProfile.CreateDefault(tuning.RifleMaxSpeed));
            brain = new RifleEnemyBrain(ShootCooldown, tuning.EnemyFleeDelay, (float)(rng.NextDouble() * ShootCooldown));
            ApplyStartingHealth();
        }

        // Public only so tests can advance time without Time.deltaTime; Update calls Tick.
        public void Step(float dt) => Tick(dt);

        public bool TryShoot() => brain.TryShoot();

        protected override void Tick(float dt)
        {
            base.Tick(dt);
            if (brain == null) return; // not configured yet
            brain.Tick(dt);
            if (brain.UpdateRider(dt)) ExecuteFire();

            NVector2 slotAnchor = world.GetSlotAnchor(slot, SizePixels + tuning.RiflePreferredDistance);
            switch (brain.Horse)
            {
                case HorseState.ApproachingSideAttackSlot:
                    NVector2 approach = ApproachAnchor(slotAnchor);
                    movement.UpdateTowardPoint(ToUnity(WorldUnits.ToMeters(approach)), dt);
                    NVector2 posPx = PositionPx();
                    if (NVector2.Distance(posPx, approach) <= movement.Profile.ArrivalRadius * WorldUnits.PixelsPerMeter + 8f)
                        brain.ArriveAtApproach();
                    break;
                case HorseState.HoldingSideAttackSlot:
                    // Src UpdateHoldPosition is an alias of UpdateTowardPoint (no drift).
                    movement.UpdateTowardPoint(ToUnity(WorldUnits.ToMeters(slotAnchor)), dt);
                    break;
                case HorseState.Fleeing:
                    PhysicsBody.linearVelocity = new Vector2(
                        WorldUnits.ToMeters(brain.FleeDirection * tuning.RifleMaxSpeed * 1.1f), 0f);
                    if (EnemyCulling.IsFleeCulled(WorldUnits.ToPixels(PhysicsBody.position.x), SizePixels, world))
                        ShouldRemove = true;
                    break;
            }
        }

        private NVector2 PositionPx() =>
            WorldUnits.ToPixels(new NVector2(PhysicsBody.position.x, PhysicsBody.position.y));

        private NVector2 ApproachAnchor(NVector2 slotAnchor)
        {
            float tile = tuning.TrainTileSize;
            return new NVector2(slotAnchor.X + tile * 1.5f,
                slot.Side == EnemySlotSide.Top ? slotAnchor.Y - tile : slotAnchor.Y + tile);
        }

        private static Vector2 ToUnity(NVector2 v) => new Vector2(v.X, v.Y);

        private void ExecuteFire()
        {
            NVector2 pos = PositionPx();
            NVector2 dir = NVector2.Normalize(world.GetTargetPoint(slot.Side, pos) - pos);
            float spread = (float)((rng.NextDouble() - 0.5) * tuning.EnemyShootSpread);
            float angle = Mathf.Atan2(dir.Y, dir.X) + spread;
            spawner.Emit(ammoDefinition.BuildRecipe(), PhysicsBody.position,
                new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)), BulletFaction.Enemy);
        }

        public override bool OnHit(BulletRuntime bullet)
        {
            if (brain == null) return base.OnHit(bullet);
            if (brain.Rider == RiderState.Dead) return false;

            if (bullet.Faction == BulletFaction.Player && IsAlive && !ShouldRemove &&
                Health - bullet.Stats.Damage <= 0)
            {
                Health = 1;
                brain.TryStartFleeingDeath();
                return true;
            }

            return base.OnHit(bullet);
        }
    }
}
