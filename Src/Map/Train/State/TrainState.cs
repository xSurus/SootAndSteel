using System;

namespace Gamelab.Map.Train.State;

public class TrainState
{
    public event Action<TrainSpeedSetting> OnSpeedChanged;
    private TrainSpeedSetting currentSpeed = TrainSpeedSetting.Default;
    public float actualSpeed;
    private float AccelerationRate => GamelabGame.Instance.GameplayConfig.TrainAccelerationRate;

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
    public float Temperature { get; set; } = 20f;
    public bool IsCoalOvenBurning { get; set; } = true;

    public TrainState()
    {
        actualSpeed = currentSpeed.TargetSpeed;
        CoalAmount = GamelabGame.Instance.GameplayConfig.TrainInitialCoalAmount;
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
    }

    public void ConsumeCoal(int amount)
    {
        CoalAmount = Math.Max(0, CoalAmount - amount);
    }

    public void AddCoal(int amount)
    {
        CoalAmount += amount;
    }
}