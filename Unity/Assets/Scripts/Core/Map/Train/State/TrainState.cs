using System;
using Gamelab.Config;

namespace Gamelab.Map.Train.State
{
    // Port of Src/Map/Train/State/TrainState.cs. Sound is dropped (Src registers the FMOD
    // "Temperature" global parameter and a looping freeze sound, and disposes them).
    // Deferred to the audio hookup: the seam is the ratio Temperature / MaxTemperature.
    public class TrainState : ITrainState
    {
        private readonly TrainSpeedSetting.Set speeds;
        private readonly TrainStateTuning tuning;
        private TrainSpeedSetting currentSpeed;

        public event Action<TrainSpeedSetting> OnSpeedChanged;
        public event Action OnTrainFrozen;

        public float actualSpeed;
        public int numberBreachedWalls;

        public TrainState(TrainSpeedSetting.Set speeds, TrainStateTuning tuning)
        {
            this.speeds = speeds;
            this.tuning = tuning;
            currentSpeed = speeds.Default;
            actualSpeed = currentSpeed.TargetSpeed;
            Temperature = tuning.MaxTemperature;
        }

        public TrainSpeedSetting CurrentSpeed
        {
            get => currentSpeed;
            set
            {
                if (currentSpeed != value)
                {
                    currentSpeed = value;
                    OnSpeedChanged?.Invoke(currentSpeed);
                }
            }
        }

        public float Temperature { get; set; }
        public float MaxTemperature => tuning.MaxTemperature;
        public bool IsCoalOvenBurning { get; set; } = true;
        public bool FuelBurningEnabled { get; set; } = true;
        public bool VictoryLapActive { get; set; }
        public float DistanceTraveled { get; private set; }
        public float MaintenanceScale { get; private set; } = 1f;
        public float TemperatureDecreaseScale { get; set; } = 1f;

        public void Update(float deltaTime)
        {
            float targetSpeed = currentSpeed.TargetSpeed;
            if (actualSpeed < targetSpeed)
            {
                actualSpeed = Math.Min(actualSpeed + tuning.AccelerationRate * deltaTime, targetSpeed);
            }
            else if (actualSpeed > targetSpeed)
            {
                actualSpeed = Math.Max(actualSpeed - tuning.AccelerationRate * deltaTime, targetSpeed);
            }

            if (numberBreachedWalls > 0)
            {
                DecreaseTemperature(tuning.DecreasePerSecondPerBreachedWall * MaintenanceScale *
                                    TemperatureDecreaseScale * numberBreachedWalls * deltaTime);
            }

            if (!IsCoalOvenBurning)
            {
                DecreaseTemperature(tuning.DecreasePerSecondEngineOff * MaintenanceScale *
                                    TemperatureDecreaseScale * deltaTime);
            }
            else if (numberBreachedWalls == 0)
            {
                IncreaseTemperature(tuning.IncreasePerSecond * deltaTime);
            }

            DistanceTraveled += actualSpeed * deltaTime;
        }

        public void ConfigurePlayerScaling(float maintenanceScale)
        {
            MaintenanceScale = maintenanceScale;
        }

        public void SlowDownIfRunning()
        {
            if (CurrentSpeed != speeds.Stopped)
                CurrentSpeed = speeds.Slow;
        }

        public void DecreaseTemperature(float amount)
        {
            Temperature = Math.Max(0, Math.Min(tuning.MaxTemperature, Temperature - amount));
            if (Temperature <= 0)
            {
                OnTrainFrozen?.Invoke();
            }
        }

        public void IncreaseTemperature(float amount)
        {
            Temperature = Math.Min(tuning.MaxTemperature, Math.Max(0, Temperature + amount));
        }
    }
}
