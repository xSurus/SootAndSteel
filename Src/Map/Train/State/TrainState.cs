using System;
using FmodForFoxes.Studio;
using Gamelab.Serialization;
using Gamelab.Services.Sound;

namespace Gamelab.Map.Train.State;

public class TrainState : IDisposable
{
    public event Action<TrainSpeedSetting> OnSpeedChanged;
    public event Action OnTrainFrozen;
    private TrainSpeedSetting currentSpeed = TrainSpeedSetting.Default;
    public float actualSpeed;
    private float AccelerationRate => GamelabGame.Instance.GameplayConfig.TrainAccelerationRate;
    public int numberBreachedWalls;

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

    public float Temperature { get; set; } = GamelabGame.Instance.GameplayConfig.TrainMaxTemperature;
    private readonly float maxTemperature = GamelabGame.Instance.GameplayConfig.TrainMaxTemperature;
    public bool IsCoalOvenBurning { get; set; } = true;
    public bool FuelBurningEnabled { get; set; } = true;
    public bool VictoryLapActive { get; set; }

    public float DistanceTraveled { get; private set; }
    public float MaintenanceScale { get; private set; } = 1f;
    public float TemperatureDecreaseScale { get; set; } = 1f;
    
    private ISoundService SoundService => GamelabGame.Instance.Services.GetService<ISoundService>();
    private readonly ParameterBinding fmodTemperature;
    public readonly EventInstance freezeSound;

    public TrainState(RunSession session)
    {
        actualSpeed = currentSpeed.TargetSpeed;
        fmodTemperature = SoundService?.RegisterGlobalParameter("Temperature", () => Temperature / maxTemperature);
        SoundService?.LoadSound(Sounds.Freeze);
        freezeSound = SoundService?.GetSoundInstance(Sounds.Freeze);
        SoundService?.RegisterParameter(freezeSound, "Temperature", () => Temperature / maxTemperature);
        freezeSound?.Start();
    }

    public void Update(float deltaTime)
    {
        float targetSpeed = currentSpeed.TargetSpeed;
        if (actualSpeed < targetSpeed)
        {
            actualSpeed = Math.Min(actualSpeed + AccelerationRate * deltaTime, targetSpeed);
        }
        else if (actualSpeed > targetSpeed)
        {
            actualSpeed = Math.Max(actualSpeed - AccelerationRate * deltaTime, targetSpeed);
        }

        if (numberBreachedWalls > 0)
        {
            float temperatureDecrease =
                GamelabGame.Instance.GameplayConfig.TrainTemperatureDecreasePerSecondPerBreachedWall *
                MaintenanceScale *
                TemperatureDecreaseScale *
                numberBreachedWalls *
                deltaTime;
            DecreaseTemperature(temperatureDecrease);
        }

        if (!IsCoalOvenBurning)
        {
            float temperatureDecrease = GamelabGame.Instance.GameplayConfig.TrainTemperatureDecreasePerSecondEngineOff *
                                        MaintenanceScale *
                                        TemperatureDecreaseScale *
                                        deltaTime;
            DecreaseTemperature(temperatureDecrease);
        }
        else if (numberBreachedWalls == 0)
        {
            float temperatureIncrease =
                GamelabGame.Instance.GameplayConfig.TrainTemperatureIncreasePerSecond * deltaTime;
            IncreaseTemperature(temperatureIncrease);
        }

        DistanceTraveled += actualSpeed * deltaTime;
    }

    public void ConfigurePlayerScaling(int playerCount)
    {
        MaintenanceScale = GamelabGame.Instance.GameplayConfig.GetMaintenanceScaleForPlayerCount(playerCount);
    }

    public void SlowDownIfRunning()
    {
        if (CurrentSpeed != TrainSpeedSetting.Stopped)
            CurrentSpeed = TrainSpeedSetting.Slow;
    }

    public void DecreaseTemperature(float amount)
    {
        float newTemperature = Temperature - amount;
        Temperature = Math.Max(
            0,
            Math.Min(GamelabGame.Instance.GameplayConfig.TrainMaxTemperature, newTemperature)
        );
        if (Temperature <= 0)
        {
            OnTrainFrozen?.Invoke();
        }
    }

    public void IncreaseTemperature(float amount)
    {
        float newTemperature = Temperature + amount;
        Temperature = Math.Min(
            GamelabGame.Instance.GameplayConfig.TrainMaxTemperature,
            Math.Max(0, newTemperature)
        );
    }


    public void Dispose()
    {
        fmodTemperature?.Deactivate();
        freezeSound?.Stop();
        freezeSound?.Dispose();
    }
}