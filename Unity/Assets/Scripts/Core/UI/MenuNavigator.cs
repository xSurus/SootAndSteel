using System;
using System.Collections.Generic;
using Gamelab.Input;
using Gamelab.UI.ViewModels;

namespace Gamelab.UI
{
    /// <summary>Drives the menu view models from input. Mirrors the Src panels' Update loops.</summary>
    public class MenuNavigator
    {
        private readonly Func<IReadOnlyList<IInputActions>> players;
        private readonly Action playSelect;

        public MenuNavigator(Func<IReadOnlyList<IInputActions>> players, Action playSelect)
        {
            this.players = players;
            this.playSelect = playSelect;
        }

        public void TickMainMenu(MainMenuViewModel vm, OptionsViewModel options)
        {
            if (options.IsOpen)
            {
                TickOptions(options);
                return;
            }

            IReadOnlyList<IInputActions> list = players();
            for (int i = 0; i < list.Count; i++)
            {
                IInputActions input = list[i];
                vm.EnsurePlayer(i);
                if (input.IsUpJustPressed())
                {
                    vm.MoveSelectionUp(i);
                    playSelect();
                }
                else if (input.IsDownJustPressed())
                {
                    vm.MoveSelectionDown(i);
                    playSelect();
                }

                if (input.IsPickupJustPressed())
                {
                    playSelect();
                    vm.Confirm(i);
                    if (options.IsOpen) return;
                }
            }
        }

        public void TickOptions(OptionsViewModel options)
        {
            if (!options.IsOpen) return;

            foreach (IInputActions input in players())
            {
                if (input.IsPauseJustPressed() || input.IsPickupJustPressed())
                {
                    options.Close();
                    return;
                }

                if (input.IsUpJustPressed())
                {
                    options.MoveSelectionUp();
                    playSelect();
                }
                else if (input.IsDownJustPressed())
                {
                    options.MoveSelectionDown();
                    playSelect();
                }

                if (input.IsLeftJustPressed()) options.DecreaseSelected();
                else if (input.IsRightJustPressed()) options.IncreaseSelected();
            }
        }

        public void TickPause(PauseMenuModel pause)
        {
            IReadOnlyList<IInputActions> list = players();

            bool anyPause = false;
            foreach (IInputActions input in list) anyPause |= input.IsPauseJustPressed();
            if (pause.IsToggleRequested(anyPause)) pause.Toggle();

            if (!pause.IsPaused) return;

            if (pause.Options.IsOpen)
            {
                TickOptions(pause.Options);
                return;
            }

            if (pause.ControlsOpen)
            {
                foreach (IInputActions input in list)
                {
                    if (input.IsPickupJustPressed() || input.IsPauseJustPressed())
                    {
                        playSelect();
                        pause.CloseControls();
                        return;
                    }
                }
                return;
            }

            bool up = false, down = false, pickup = false;
            foreach (IInputActions input in list)
            {
                up |= input.IsUpJustPressed();
                down |= input.IsDownJustPressed();
                pickup |= input.IsPickupJustPressed();
            }

            if (up)
            {
                pause.MoveUp();
                playSelect();
            }
            else if (down)
            {
                pause.MoveDown();
                playSelect();
            }

            if (pickup)
            {
                playSelect();
                pause.Confirm();
            }
        }
    }
}
