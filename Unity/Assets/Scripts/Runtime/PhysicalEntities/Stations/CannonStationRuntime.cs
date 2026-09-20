using System;
using Gamelab.Config;
using Gamelab.Items.Bullets;
using Gamelab.PhysicalEntities.Bullets;
using Gamelab.PhysicalEntities.Interfaces;
using Gamelab.Utils;
using UnityEngine;

namespace Gamelab.PhysicalEntities.Stations
{
    // Port of Src Cannon/CannonStation.cs: Update, OnInteract, ReloadCannon, FireCannon,
    // UpdateAim, AimDirection.
    //
    // DEFERRED (seat logic): OnGrab, OnRelease, FindEjectPosition, IsTileFree, SeatedPlayer and
    // SeatPosition. They need the player type (SeatedPlayer, PlayerConfiguration.Input,
    // PhysicsBody), Player.SeatAt/UnseatFrom, map tile queries (TileSize, GetTileIndexFromPixels,
    // Width/Height) and MapObjects. Until then AimSource stands in for the seated player's input.
    // Src CannonSlot (Structures/CannonSlot.cs) is not ported.
    //
    // Dropped presentation: load and fire sounds, muzzle flash, the aim line in Draw
    // (white unseated, orange-red seated), station Draw. AimAngle lives on the station because
    // Src stores it in PhysicsBody.Rotation while the Unity station body is static.
    // Re-declares IInteractable so interface-typed calls reach OnInteract.
    public class CannonStationRuntime : StationRuntime, IInteractable
    {
        public IStationGrid Grid { get; set; }
        public IBulletItemSpawner Spawner { get; set; }
        public ICannonAimSource AimSource { get; set; }
        public CannonTuning Tuning { get; set; } = CannonTuning.Default;

        public float CooldownTimer { get; private set; }
        public float AimAngle { get; set; }

        // Src Events.FireCannonFired.
        public event Action Fired;

        public override void Initialize(StationCatalogEntry catalogEntry)
        {
            base.Initialize(catalogEntry);
            if (string.IsNullOrEmpty(StationId)) StationId = StationIds.Cannon;
        }

        public System.Numerics.Vector2 AimDirection =>
            new System.Numerics.Vector2((float)Math.Cos(AimAngle), (float)Math.Sin(AimAngle));

        public override void Update(float dt)
        {
            base.Update(dt);
            if (CooldownTimer > 0f) CooldownTimer -= dt;
            UpdateAim(dt);
        }

        private void UpdateAim(float dt)
        {
            if (AimSource == null) return;
            AimAngle = CannonAim.Step(AimAngle, AimSource.GetMovement(), dt,
                Tuning.CannonRotationSpeed, Tuning.InputMovementDeadzoneSquared);
        }

        public void OnInteract(IPlayerActor interactingPlayer)
        {
            if (CooldownTimer > 0f) return;
            ReloadCannon();
            FireCannon();
        }

        private void ReloadCannon()
        {
            if (Grid == null) return;
            foreach (GridDirection dir in new[] { GridDirection.Up, GridDirection.Down, GridDirection.Left, GridDirection.Right })
            {
                if (Grid.GetAdjacentStation(Position, dir) is BulletRackRuntime rack
                    && rack.TryProvideItem(out var bullet))
                {
                    // Src overwrites HeldItem per rack in enum order (Up, Down, Left, Right), so a
                    // bullet loaded from an earlier rack is discarded. Replicated on purpose.
                    HeldItem = bullet;
                }
            }
        }

        public void FireCannon()
        {
            if (HeldItem == null) return;
            if (Spawner == null) throw new InvalidOperationException("Spawner must be set before firing.");

            System.Numerics.Vector2 direction = AimDirection; // unit length already (Src normalizes after use)
            // Src: DrawPosition + tile/2 == Position (DrawPosition = Position - tile/2), in pixels.
            System.Numerics.Vector2 centerPx = new System.Numerics.Vector2(Position.x, Position.y) * WorldUnits.PixelsPerMeter;
            System.Numerics.Vector2 posPx = centerPx + direction * (Tuning.TrainTileSize * 0.5f);
            System.Numerics.Vector2 posM = WorldUnits.ToMeters(posPx);

            Spawner.Emit(((BulletItem)HeldItem).ToRecipe(), new Vector2(posM.X, posM.Y),
                new Vector2(direction.X, direction.Y), BulletFaction.Player);
            CooldownTimer = Tuning.CannonCooldown;
            HeldItem = null;
            Fired?.Invoke();
        }
    }
}
