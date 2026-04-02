using System;

namespace Gamelab.Map.Train.State;

public class TrainState
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

    public int CoalAmount { get; set; }
    public float Temperature { get; set; } = GamelabGame.Instance.GameplayConfig.TrainMaxTemperature;
    public bool IsCoalOvenBurning { get; set; } = true;

    /// <summary>
    /// When true (level complete outro), the train keeps moving but stations must not consume coal/fuel.
    /// </summary>
    public bool VictoryLapActive { get; set; }
    
    public bool IsFrozen => Temperature <= 0;
    public float DistanceTraveled { get; private set; }

    public TrainState()
    {
        actualSpeed = currentSpeed.TargetSpeed;
        CoalAmount = GamelabGame.Instance.GameplayConfig.TrainInitialCoalAmount;
        Temperature = GamelabGame.Instance.GameplayConfig.TrainMaxTemperature;
    }

    public void Update(float deltaTime)
    {
        if (actualSpeed < currentSpeed.TargetSpeed)
        {
            actualSpeed = Math.Min(actualSpeed + AccelerationRate * deltaTime, currentSpeed.TargetSpeed);
        }
        else if (actualSpeed > currentSpeed.TargetSpeed)
        {
            actualSpeed = Math.Max(actualSpeed - AccelerationRate * deltaTime, currentSpeed.TargetSpeed);
        }

        if (numberBreachedWalls > 0)
        {
            float temperatureDecrease =
                GamelabGame.Instance.GameplayConfig.TrainTemperatureDecreasePerSecondPerBreachedWall * numberBreachedWalls * deltaTime;
            DecreaseTemperature(temperatureDecrease);
        }

        if (IsCoalOvenBurning && numberBreachedWalls == 0)
        {
            float temperatureIncrease = GamelabGame.Instance.GameplayConfig.TrainTemperatureIncreasePerSecond * deltaTime;
            IncreaseTemperature(temperatureIncrease);
        }

        DistanceTraveled += actualSpeed * deltaTime;
    }

    public void ConsumeCoal(int amount)
    {
        CoalAmount = Math.Max(0, CoalAmount - amount);
    }

    public void AddCoal(int amount)
    {
        CoalAmount += amount;
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
}
