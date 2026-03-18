using System;
using Gamelab.Config;

namespace Gamelab.Map.Train.State;

public class TrainState
{
    public event Action<TrainSpeedSetting> OnSpeedChanged;
    private TrainSpeedSetting currentSpeed = TrainSpeedSetting.Default;
    public float ActualSpeed;
    public float AccelerationRate { get; set; }

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

    public TrainState(GameplayConfig config)
    {
        currentSpeed = TrainSpeedSetting.Default;
        ActualSpeed = currentSpeed.TargetSpeed;
        AccelerationRate = config.TrainAccelerationRate;
        CoalAmount = config.TrainInitialCoalAmount;
    }

    public void Update(float deltaTime)
    {
        if (ActualSpeed < currentSpeed.TargetSpeed)
        {
            ActualSpeed = Math.Min(ActualSpeed + AccelerationRate * deltaTime, currentSpeed.TargetSpeed);
        }
        else if (ActualSpeed > currentSpeed.TargetSpeed)
        {
            ActualSpeed = Math.Max(ActualSpeed - AccelerationRate * deltaTime, currentSpeed.TargetSpeed);
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