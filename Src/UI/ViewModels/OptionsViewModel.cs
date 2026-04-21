using System;
using System.Collections.Generic;
using Gamelab.Services.Sound;

namespace Gamelab.UI.ViewModels;

public class OptionsViewModel
{
    public enum Row
    {
        Master,
        Music,
        Sfx,
        Back,
    }

    private static readonly Row[] DefaultRowOrder = [Row.Master, Row.Music, Row.Sfx, Row.Back];

    private readonly ISoundService soundService;

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

    public OptionsViewModel(ISoundService soundService)
    {
        this.soundService = soundService;
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

    public static bool IsAdjustable(Row row) => row != Row.Back;

    public void DecreaseSelected() => AdjustSelected(-VolumeStep);

    public void IncreaseSelected() => AdjustSelected(VolumeStep);

    /// <summary>Activate the currently selected non-adjustable row (e.g. Back).</summary>
    public void ConfirmSelection()
    {
        if (SelectedRow == Row.Back) Close();
    }

    public float VolumeOf(Row row) => row switch
    {
        Row.Master => soundService.Settings.MasterVolume,
        Row.Music => soundService.Settings.MusicVolume,
        Row.Sfx => soundService.Settings.SfxVolume,
        _ => 0f,
    };

    public static string LabelOf(Row row) => row switch
    {
        Row.Master => "Master Volume",
        Row.Music => "Ambient / Music",
        Row.Sfx => "Sound Effects",
        Row.Back => "Back",
        _ => string.Empty,
    };

    private void AdjustSelected(float delta)
    {
        Row row = SelectedRow;
        if (!IsAdjustable(row)) return;

        SoundSettings s = soundService.Settings;
        switch (row)
        {
            case Row.Master: soundService.SetMasterVolume(s.MasterVolume + delta); break;
            case Row.Music: soundService.SetMusicVolume(s.MusicVolume + delta); break;
            case Row.Sfx: soundService.SetSfxVolume(s.SfxVolume + delta); break;
        }
        OnVolumeChanged?.Invoke(row);
    }
}
