using System.Collections.Generic;
using Gamelab.Input;
using Gamelab.UI;
using Gamelab.UI.ViewModels;
using NUnit.Framework;

namespace Gamelab.Tests.EditMode.UI
{
    public class MenuNavigatorTests
    {
        private FakeInputActions p0, p1;
        private FakeVolumeSettings volume;
        private OptionsViewModel options;
        private int sounds;
        private MenuNavigator nav;

        [SetUp]
        public void SetUp()
        {
            p0 = new FakeInputActions();
            p1 = new FakeInputActions();
            volume = new FakeVolumeSettings();
            options = new OptionsViewModel(volume);
            sounds = 0;
            nav = new MenuNavigator(() => new List<IInputActions> { p0, p1 }, () => sounds++);
        }

        private MainMenuViewModel Menu(System.Action onFirst = null, System.Action onSecond = null) =>
            new MainMenuViewModel(new[]
            {
                new MainMenuViewModel.Entry("A", onFirst),
                new MainMenuViewModel.Entry("B", onSecond),
            });

        [Test]
        public void MainMenu_up_down_move_per_player_with_sound()
        {
            var vm = Menu();
            p0.Down = true;
            nav.TickMainMenu(vm, options);
            Assert.AreEqual(1, vm.PlayerSelections[0]);
            Assert.AreEqual(0, vm.PlayerSelections[1]);
            Assert.AreEqual(1, sounds);
        }

        [Test]
        public void MainMenu_up_wins_over_down()
        {
            var vm = Menu();
            p0.Up = true; p0.Down = true;
            nav.TickMainMenu(vm, options);
            Assert.AreEqual(1, vm.PlayerSelections[0]); // wrapped up from 0 to 1 of 2
            Assert.AreEqual(1, sounds);
        }

        [Test]
        public void MainMenu_pickup_plays_sound_and_confirms()
        {
            int b = 0;
            var vm = Menu(null, () => b++);
            p1.Down = true;
            nav.TickMainMenu(vm, options);
            p1.Clear();
            p1.Pickup = true;
            nav.TickMainMenu(vm, options);
            Assert.AreEqual(1, b);
            Assert.AreEqual(2, sounds);
        }

        [Test]
        public void MainMenu_options_entry_opens_and_next_tick_handles_options()
        {
            var vm = Menu(() => options.Open());
            p0.Pickup = true;
            nav.TickMainMenu(vm, options);
            Assert.IsTrue(options.IsOpen);
            p0.Clear();
            p0.Right = true;
            nav.TickMainMenu(vm, options);
            Assert.AreEqual(0.55f, volume.MasterVolume, 1e-5f);
            Assert.AreEqual(0, vm.PlayerSelections[0]);
        }

        [Test]
        public void Options_up_down_play_sound_left_right_do_not()
        {
            options.Open();
            p0.Down = true;
            nav.TickOptions(options);
            Assert.AreEqual(1, options.SelectionIndex);
            Assert.AreEqual(1, sounds);
            p0.Clear();
            p0.Left = true;
            nav.TickOptions(options);
            Assert.AreEqual(0.45f, volume.MusicVolume, 1e-5f);
            Assert.AreEqual(1, sounds);
        }

        [Test]
        public void Options_close_on_pickup_or_pause()
        {
            options.Open();
            p1.Pickup = true;
            nav.TickOptions(options);
            Assert.IsFalse(options.IsOpen);

            options.Open();
            p1.Clear();
            p0.Pause = true;
            nav.TickOptions(options);
            Assert.IsFalse(options.IsOpen);
        }

        [Test]
        public void Options_ignored_when_closed()
        {
            p0.Down = true;
            nav.TickOptions(options);
            Assert.AreEqual(0, sounds);
        }

        [Test]
        public void Pause_press_toggles_and_unpause_resets()
        {
            var pause = new PauseMenuModel(options);
            p1.Pause = true;
            nav.TickPause(pause);
            Assert.IsTrue(pause.IsPaused);
            nav.TickPause(pause);
            Assert.IsFalse(pause.IsPaused);
        }

        [Test]
        public void Pause_ignores_input_while_not_paused()
        {
            var pause = new PauseMenuModel(options);
            p0.Down = true; p0.Pickup = true;
            nav.TickPause(pause);
            Assert.AreEqual(0, pause.SelectionIndex);
            Assert.AreEqual(0, sounds);
        }

        [Test]
        public void Pause_root_navigation_ors_players_up_wins_with_sound()
        {
            var pause = new PauseMenuModel(options);
            pause.Toggle();
            p0.Down = true; p1.Up = true;
            nav.TickPause(pause);
            Assert.AreEqual(3, pause.SelectionIndex);
            Assert.AreEqual(1, sounds);
        }

        [Test]
        public void Pause_confirm_plays_sound_and_opens_options_then_pause_closes_options_not_pause()
        {
            var pause = new PauseMenuModel(options);
            pause.Toggle();
            p0.Down = true;
            nav.TickPause(pause);
            p0.Clear();
            p0.Pickup = true;
            nav.TickPause(pause);
            Assert.IsTrue(options.IsOpen);
            Assert.AreEqual(2, sounds);

            p0.Clear();
            p1.Pause = true;
            nav.TickPause(pause);
            Assert.IsFalse(options.IsOpen);
            Assert.IsTrue(pause.IsPaused);
        }

        [Test]
        public void Pause_controls_close_on_pickup_or_pause_with_sound()
        {
            var pause = new PauseMenuModel(options);
            pause.Toggle();
            pause.MoveDown(); pause.MoveDown();
            pause.Confirm();
            Assert.IsTrue(pause.ControlsOpen);

            p0.Down = true; // ignored while controls open
            nav.TickPause(pause);
            Assert.IsTrue(pause.ControlsOpen);
            Assert.AreEqual(0, sounds);

            p0.Clear();
            p1.Pickup = true;
            nav.TickPause(pause);
            Assert.IsFalse(pause.ControlsOpen);
            Assert.IsTrue(pause.IsPaused);
            Assert.AreEqual(1, sounds);
        }

        [Test]
        public void Pause_press_with_controls_open_unpauses_via_toggle()
        {
            var pause = new PauseMenuModel(options);
            pause.Toggle();
            pause.MoveDown(); pause.MoveDown();
            pause.Confirm();
            p0.Pause = true;
            nav.TickPause(pause);
            Assert.IsFalse(pause.IsPaused);
            Assert.IsFalse(pause.ControlsOpen);
        }

        [Test]
        public void Pause_exit_raises_event()
        {
            var pause = new PauseMenuModel(options);
            int exits = 0;
            pause.ExitRequested += () => exits++;
            pause.Toggle();
            pause.MoveUp();
            p0.Pickup = true;
            nav.TickPause(pause);
            Assert.AreEqual(1, exits);
        }
    }
}
