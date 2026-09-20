using System;
using Random = System.Random;
using Gamelab.PhysicalEntities.Bullets;
using Gamelab.PhysicalEntities.Interfaces;
using UnityEngine;

namespace Gamelab.Map.Train
{
    /// <summary>
    /// Src ShootHoleWall: health, damage from enemy bullets, repair, and the damage sprites.
    /// Deferred, all at this one seam: sounds (WallHit, WallBreak, WallFix, WallFixed), wall smoke VFX,
    /// the breach screen shake (GameplayScreen.OnWallBreached), highlight, and the IInteractable /
    /// IPickable members OnPickup and OnInteractHeld(Player, dt), which need the Player type. When the
    /// player wave lands, OnInteractHeld should call Repair(dt) and OnInteractReleased stop the fix sound.
    /// The bottom light overlay (DrawLightBatch) is also not ported.
    /// </summary>
    public class ShootHoleWallRuntime : MonoBehaviour, IDamageable
    {
        private WallSpec spec;
        private float feetYPx;
        private Random rng;

        public bool IsTop { get; set; }
        public WallHealth Health { get; } = new WallHealth();
        public int Variation { get; private set; } = 1;
        public SpriteRenderer Visual { get; private set; }
        public string SpriteName { get; private set; }

        public event Action Breached
        {
            add => Health.Breached += value;
            remove => Health.Breached -= value;
        }

        public event Action Repaired
        {
            add => Health.Repaired += value;
            remove => Health.Repaired -= value;
        }

        internal void Configure(WallSpec spec, float feetYPx, SpriteRenderer visual, Random rng)
        {
            this.spec = spec;
            this.feetYPx = feetYPx;
            Visual = visual;
            this.rng = rng;
            IsTop = spec.IsTop;
            Variation = rng.Next(1, 4);
            RefreshSprite();
        }

        public void TakeDamage(float damageAmount)
        {
            Health.TakeDamage(damageAmount);
            RefreshSprite();
        }

        public void Repair(float dt)
        {
            bool wasBreached = Health.IsBreached;
            Health.Repair(dt);
            if (wasBreached && !Health.IsBreached) Variation = rng.Next(1, 4);
            RefreshSprite();
        }

        public bool OnHit(BulletRuntime bullet)
        {
            if (bullet.Faction != BulletFaction.Enemy || Health.IsBroken) return false;
            TakeDamage(bullet.Stats.Damage);
            return true;
        }

        private void RefreshSprite()
        {
            if (Visual == null) return;
            string name = WallSpriteNames.Select(IsTop, Health.DamagePercent, Health.IsBreached, Variation);
            if (name == SpriteName) return;
            SpriteName = name;
            Visual.sprite = MapSprites.Get("Walls/" + name);
            TrainMapRuntime.PlaceSprite(Visual, spec, feetYPx, 0);
        }
    }
}
