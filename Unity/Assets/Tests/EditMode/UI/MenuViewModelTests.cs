using System;
using Gamelab.UI.ViewModels;
using NUnit.Framework;

namespace Gamelab.Tests.EditMode.UI
{
    public class FakeVolumeSettings : IVolumeSettings
    {
        public float MasterVolume { get; private set; } = 0.5f;
        public float MusicVolume { get; private set; } = 0.5f;
        public float SfxVolume { get; private set; } = 0.5f;
        public void SetMasterVolume(float v) => MasterVolume = Math.Max(0f, Math.Min(1f, v));
        public void SetMusicVolume(float v) => MusicVolume = Math.Max(0f, Math.Min(1f, v));
        public void SetSfxVolume(float v) => SfxVolume = Math.Max(0f, Math.Min(1f, v));
    }

    public class MainMenuViewModelTests
    {
        private static MainMenuViewModel Make(Action a = null, Action b = null) =>
            new MainMenuViewModel(new[]
            {
                new MainMenuViewModel.Entry("A", a),
                new MainMenuViewModel.Entry("B", b),
                new MainMenuViewModel.Entry("C", null),
            });

        [Test]
        public void Selection_wraps_both_ways()
        {
            var vm = Make();
            vm.MoveSelectionUp(0);
            Assert.AreEqual(2, vm.PlayerSelections[0]);
            vm.MoveSelectionDown(0);
            Assert.AreEqual(0, vm.PlayerSelections[0]);
        }

        [Test]
        public void Selection_is_per_player()
        {
            var vm = Make();
            vm.MoveSelectionDown(0);
            vm.EnsurePlayer(1);
            Assert.AreEqual(1, vm.PlayerSelections[0]);
            Assert.AreEqual(0, vm.PlayerSelections[1]);
            Assert.IsTrue(vm.IsEntrySelectedByAnyone(1));
            Assert.IsFalse(vm.IsEntrySelectedByAnyone(2));
        }

        [Test]
        public void EnsurePlayer_raises_selection_changed_once_for_new_player()
        {
            var vm = Make();
            int calls = 0, lastPlayer = -1, lastEntry = -1;
            vm.OnSelectionChanged += (p, e) => { calls++; lastPlayer = p; lastEntry = e; };
            vm.EnsurePlayer(2);
            vm.EnsurePlayer(2);
            Assert.AreEqual(1, calls);
            Assert.AreEqual(2, lastPlayer);
            Assert.AreEqual(0, lastEntry);
        }

        [Test]
        public void Confirm_invokes_selected_entry()
        {
            int a = 0, b = 0;
            var vm = Make(() => a++, () => b++);
            vm.MoveSelectionDown(0);
            vm.Confirm(0);
            Assert.AreEqual(0, a);
            Assert.AreEqual(1, b);
        }

        [Test]
        public void Empty_entries_throw()
        {
            Assert.Throws<ArgumentException>(() => new MainMenuViewModel(new MainMenuViewModel.Entry[0]));
        }
    }

    public class OptionsViewModelTests
    {
        [Test]
        public void Increase_and_decrease_step_selected_row()
        {
            var s = new FakeVolumeSettings();
            var vm = new OptionsViewModel(s);
            vm.Open();
            vm.IncreaseSelected();
            Assert.AreEqual(0.55f, s.MasterVolume, 1e-5f);
            vm.MoveSelectionDown();
            vm.DecreaseSelected();
            Assert.AreEqual(0.45f, s.MusicVolume, 1e-5f);
            Assert.AreEqual(0.5f, s.SfxVolume, 1e-5f);
        }

        [Test]
        public void Volume_clamps_via_settings()
        {
            var s = new FakeVolumeSettings();
            var vm = new OptionsViewModel(s) { VolumeStep = 0.7f };
            vm.IncreaseSelected();
            Assert.AreEqual(1f, vm.VolumeOf(OptionsViewModel.Row.Master));
            vm.DecreaseSelected();
            vm.DecreaseSelected();
            Assert.AreEqual(0f, vm.VolumeOf(OptionsViewModel.Row.Master));
        }

        [Test]
        public void Selection_wraps_and_open_resets_it()
        {
            var vm = new OptionsViewModel(new FakeVolumeSettings());
            vm.MoveSelectionUp();
            Assert.AreEqual(OptionsViewModel.Row.Sfx, vm.SelectedRow);
            vm.Open();
            Assert.AreEqual(0, vm.SelectionIndex);
        }

        [Test]
        public void Open_close_raise_events_once()
        {
            var vm = new OptionsViewModel(new FakeVolumeSettings());
            int opened = 0, closed = 0;
            vm.OnOpened += () => opened++;
            vm.OnClosed += () => closed++;
            vm.Open(); vm.Open(); vm.Close(); vm.Close();
            Assert.AreEqual(1, opened);
            Assert.AreEqual(1, closed);
        }

        [Test]
        public void Volume_change_event_names_row()
        {
            var vm = new OptionsViewModel(new FakeVolumeSettings());
            OptionsViewModel.Row? got = null;
            vm.OnVolumeChanged += r => got = r;
            vm.MoveSelectionDown();
            vm.IncreaseSelected();
            Assert.AreEqual(OptionsViewModel.Row.Music, got);
        }
    }

    public class PauseMenuModelTests
    {
        private static PauseMenuModel Make() => new PauseMenuModel(new OptionsViewModel(new FakeVolumeSettings()));

        [Test]
        public void Selection_wraps()
        {
            var m = Make();
            m.MoveUp();
            Assert.AreEqual(3, m.SelectionIndex);
            m.MoveDown();
            Assert.AreEqual(0, m.SelectionIndex);
        }

        [Test]
        public void Toggle_flips_and_resets_selection()
        {
            var m = Make();
            m.Toggle();
            Assert.IsTrue(m.IsPaused);
            m.MoveDown();
            m.Toggle();
            Assert.IsFalse(m.IsPaused);
            Assert.AreEqual(0, m.SelectionIndex);
        }

        [Test]
        public void Unpausing_closes_options_and_controls()
        {
            var m = Make();
            m.Toggle();
            m.Options.Open();
            m.MoveDown(); m.MoveDown();
            m.Confirm();
            Assert.IsTrue(m.Options.IsOpen);
            Assert.IsTrue(m.ControlsOpen);
            m.Toggle();
            Assert.IsFalse(m.Options.IsOpen);
            Assert.IsFalse(m.ControlsOpen);
        }

        [Test]
        public void Options_open_blocks_toggle_request()
        {
            var m = Make();
            Assert.IsTrue(m.IsToggleRequested(true));
            Assert.IsFalse(m.IsToggleRequested(false));
            m.Options.Open();
            Assert.IsFalse(m.IsToggleRequested(true));
        }

        [Test]
        public void Confirm_dispatches_by_item()
        {
            var m = Make();
            int exits = 0;
            m.ExitRequested += () => exits++;
            m.Toggle();

            m.MoveDown();
            m.Confirm();
            Assert.IsTrue(m.Options.IsOpen);
            m.Options.Close();

            m.MoveDown();
            m.Confirm();
            Assert.IsTrue(m.ControlsOpen);
            m.CloseControls();
            Assert.IsFalse(m.ControlsOpen);

            m.MoveDown();
            m.Confirm();
            Assert.AreEqual(1, exits);

            m.MoveDown(); // wraps to Continue
            m.Confirm();
            Assert.IsFalse(m.IsPaused);
        }

        [Test]
        public void Changed_is_raised_for_every_visible_change()
        {
            var m = Make();
            int n = 0;
            m.Changed += () => n++;
            m.Toggle(); Assert.AreEqual(1, n);
            m.MoveDown(); Assert.AreEqual(2, n);
            m.MoveUp(); Assert.AreEqual(3, n);
            m.MoveDown(); m.MoveDown(); n = 0;
            m.Confirm(); Assert.AreEqual(1, n, "controls opened");
            m.CloseControls(); Assert.AreEqual(2, n);
            m.MoveUp(); n = 0;
            m.Confirm(); Assert.AreEqual(1, n, "options opened");
            m.Options.Close(); Assert.AreEqual(2, n, "options closed");
        }

        [Test]
        public void Changed_listener_sees_final_state_when_unpausing()
        {
            var m = Make();
            m.Toggle();
            m.MoveDown(); m.Confirm();
            bool sawStale = false;
            m.Changed += () => sawStale |= m.IsPaused == false && m.ControlsOpen;
            m.Toggle();
            Assert.IsFalse(sawStale);
        }
    }
}
