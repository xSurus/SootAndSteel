namespace Gamelab.Enemies.Core
{
    public enum HorseState
    {
        ApproachingSideAttackSlot,
        HoldingSideAttackSlot,
        Fleeing
    }

    public enum RiderState
    {
        Idle,
        Aiming,
        Recoil,
        Dead
    }

    /// <summary>
    /// Pure state and timers of Src/Enemies/Types/Enemy.cs. Movement, firing, VFX and
    /// sound stay in the runtime wrapper. Left out as presentation: attackAngle,
    /// attackPoseTimer, animation names, neck bleed emitter.
    /// </summary>
    public class RifleEnemyBrain
    {
        public const float AimDurationSeconds = 1f;
        public const float RecoilDurationSeconds = 0.4f;

        private readonly float shootCooldown;
        private readonly float fleeDelay;
        private float timeSinceLastShot;
        private float aimTimer;
        private float recoilTimer;
        private float fleeTimer;

        public RifleEnemyBrain(float shootCooldown, float fleeDelay, float initialTimeSinceLastShot)
        {
            this.shootCooldown = shootCooldown;
            this.fleeDelay = fleeDelay;
            timeSinceLastShot = initialTimeSinceLastShot;
        }

        public HorseState Horse { get; private set; } = HorseState.ApproachingSideAttackSlot;
        public RiderState Rider { get; private set; } = RiderState.Idle;
        public float FleeDirection { get; private set; }
        public bool WasNeutralized => Rider == RiderState.Dead;

        public void Tick(float dt) => timeSinceLastShot += dt;

        public void ArriveAtApproach()
        {
            if (Horse == HorseState.ApproachingSideAttackSlot)
                Horse = HorseState.HoldingSideAttackSlot;
        }

        public bool TryShoot()
        {
            if (Rider == RiderState.Dead) return false;
            if (Horse != HorseState.HoldingSideAttackSlot ||
                timeSinceLastShot < shootCooldown ||
                Rider != RiderState.Idle)
                return false;

            Rider = RiderState.Aiming;
            aimTimer = AimDurationSeconds;
            timeSinceLastShot = 0f;
            return true;
        }

        /// <summary>Returns true on the frame the shot must be fired.</summary>
        public bool UpdateRider(float dt)
        {
            switch (Rider)
            {
                case RiderState.Aiming:
                    aimTimer -= dt;
                    if (aimTimer <= 0)
                    {
                        Rider = RiderState.Recoil;
                        recoilTimer = RecoilDurationSeconds;
                        return true;
                    }
                    break;
                case RiderState.Recoil:
                    recoilTimer -= dt;
                    if (recoilTimer <= 0) Rider = RiderState.Idle;
                    break;
                case RiderState.Dead:
                    fleeTimer -= dt;
                    if (fleeTimer <= 0)
                    {
                        FleeDirection = 1f;
                        Horse = HorseState.Fleeing;
                    }
                    break;
            }
            return false;
        }

        public bool TryStartFleeingDeath()
        {
            if (Rider == RiderState.Dead) return false;
            Rider = RiderState.Dead;
            fleeTimer = fleeDelay;
            return true;
        }
    }
}
