using System.Collections.Generic;
using System.Linq;
using Gamelab.Input; // Ensure this matches where you put IInputProvider
using Microsoft.Xna.Framework.Input;

namespace Gamelab.Players;

public class PlayerConfiguration
{
    public int PlayerIndex { get; set; }
    public IInputProvider Input { get; init; }
}

public class PlayerManager
{
    private readonly List<PlayerConfiguration> configs = [];
    public IReadOnlyList<PlayerConfiguration> Configs => configs;
    public bool IsControllerJoined(int controllerIndex)
    {
        return configs.Any(c => c.Input is GamePadInputProvider gp && gp.ControllerIndex == controllerIndex);
    }
    
    public bool JoinPlayer(IInputProvider input)
    {
        if (configs.Count >= 4) return false;

        PlayerConfiguration config = new PlayerConfiguration
        {
            PlayerIndex = configs.Count,
            Input = input
        };

        configs.Add(config);
        return true;
    }

    public void Reset()
    {
        configs.Clear();
    }
}