using System;
using System.Collections.Generic;

namespace Gamelab.UI.ViewModels
{
    /// <summary>Volume settings the options menu reads and writes. Setters clamp to 0..1.</summary>
    public interface IVolumeSettings
    {
        float MasterVolume { get; }
        float MusicVolume { get; }
        float SfxVolume { get; }
        void SetMasterVolume(float value);
        void SetMusicVolume(float value);
        void SetSfxVolume(float value);
    }

    public class OptionsViewModel
    {
        public enum Row
        {
            Master,
            Music,
            Sfx,
        }

        private static readonly Row[] DefaultRowOrder = { Row.Master, Row.Music, Row.Sfx };

        private readonly IVolumeSettings settings;

        public IReadOnlyList<Row> Rows => DefaultRowOrder;
        public int SelectionIndex { get; private set; }
        public Row SelectedRow => Rows[SelectionIndex];
        public bool IsOpen { get; private set; }

        /// <summary>Volume delta applied per Increase/Decrease call.</summary>
        public float VolumeStep { get; set; } = 0.05f;

        public event Action OnOpened;
        public event Action OnClosed;
        public event Action OnSelectionChanged;
        public event Action<Row> OnVolumeChanged;

        public OptionsViewModel(IVolumeSettings settings)
        {
            this.settings = settings;
        }

        public void Open()
        {
            if (IsOpen) return;
            IsOpen = true;
            SelectionIndex = 0;
            OnOpened?.Invoke();
        }

        public void Close()
        {
            if (!IsOpen) return;
            IsOpen = false;
            OnClosed?.Invoke();
        }

        public void MoveSelectionUp()
        {
            SelectionIndex = (SelectionIndex - 1 + Rows.Count) % Rows.Count;
            OnSelectionChanged?.Invoke();
        }

        public void MoveSelectionDown()
        {
            SelectionIndex = (SelectionIndex + 1) % Rows.Count;
            OnSelectionChanged?.Invoke();
        }

        public void SetSelection(int index)
        {
            if (index < 0 || index >= Rows.Count) return;
            if (index == SelectionIndex) return;
            SelectionIndex = index;
            OnSelectionChanged?.Invoke();
        }

        public void DecreaseSelected() => AdjustSelected(-VolumeStep);

        public void IncreaseSelected() => AdjustSelected(VolumeStep);

        public float VolumeOf(Row row)
        {
            switch (row)
            {
                case Row.Master: return settings.MasterVolume;
                case Row.Music: return settings.MusicVolume;
                case Row.Sfx: return settings.SfxVolume;
                default: return 0f;
            }
        }

        private void AdjustSelected(float delta)
        {
            Row row = SelectedRow;
            switch (row)
            {
                case Row.Master: settings.SetMasterVolume(settings.MasterVolume + delta); break;
                case Row.Music: settings.SetMusicVolume(settings.MusicVolume + delta); break;
                case Row.Sfx: settings.SetSfxVolume(settings.SfxVolume + delta); break;
            }
            OnVolumeChanged?.Invoke(row);
        }
    }
}
