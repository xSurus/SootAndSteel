using Gamelab.Input;

namespace Gamelab.Players
{
    /// <summary>Replaces Src/Players/PlayerManager.cs's PlayerConfiguration.</summary>
    public class PlayerSlot
    {
        public int PlayerIndex { get; set; }
        public IInputActions Input { get; }

        public PlayerSlot(int playerIndex, IInputActions input)
        {
            PlayerIndex = playerIndex;
            Input = input;
        }
    }
}
