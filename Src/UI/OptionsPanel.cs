using System;
using System.Collections.Generic;
using Gamelab.Components;
using Gamelab.Players;
using Gamelab.Services.Sound;
using Gamelab.UI.ViewModels;
using MonoGameGum;

namespace Gamelab.UI;

public sealed class OptionsPanel : IDisposable
{
    private const string AdjustmentsHint = "- Adjustments -";

    private readonly ISoundService soundService;
    private readonly OptionsViewModel viewModel;
    private readonly OptionOverlay optionsOverlay;
    private bool disposed;

    public bool IsOpen => viewModel.IsOpen;
    public OptionsViewModel ViewModel => viewModel;

    public event Action OnClosed
    {
        add => viewModel.OnClosed += value;
        remove => viewModel.OnClosed -= value;
    }

    public OptionsPanel(GamelabGame game)
        : this(game.Services.GetService<ISoundService>(), game.GameplayConfig.MenuVolumeStep)
    {
    }

    public OptionsPanel(ISoundService soundService, float volumeStep)
    {
        this.soundService = soundService;
        viewModel = new OptionsViewModel(soundService) { VolumeStep = volumeStep };

        optionsOverlay = new OptionOverlay();
        optionsOverlay.AddToRoot();
        optionsOverlay.Visual.Visible = false;

        viewModel.OnOpened += OnViewModelOpened;
        viewModel.OnClosed += OnViewModelClosed;
        viewModel.OnSelectionChanged += OnSelectionChanged;
        viewModel.OnVolumeChanged += OnVolumeChanged;
    }

    public void Open() => viewModel.Open();

    public void Close() => viewModel.Close();

    public void Update(IReadOnlyList<PlayerConfiguration> players)
    {
        if (!viewModel.IsOpen) return;

        foreach (PlayerConfiguration player in players)
        {
            var input = player.Input;

            if (input.IsPauseJustPressed())
            {
                viewModel.Close();
                return;
            }

            if (input.IsUpJustPressed())
            {
                viewModel.MoveSelectionUp();
                soundService.PlayOnce(Sounds.MenuSelect);
            }
            else if (input.IsDownJustPressed())
            {
                viewModel.MoveSelectionDown();
                soundService.PlayOnce(Sounds.MenuSelect);
            }

            if (input.IsLeftJustPressed()) viewModel.DecreaseSelected();
            else if (input.IsRightJustPressed()) viewModel.IncreaseSelected();
        }
    }

    public void Dispose()
    {
        if (disposed) return;
        disposed = true;

        viewModel.OnOpened -= OnViewModelOpened;
        viewModel.OnClosed -= OnViewModelClosed;
        viewModel.OnSelectionChanged -= OnSelectionChanged;
        viewModel.OnVolumeChanged -= OnVolumeChanged;

        if (optionsOverlay.Visual != null)
        {
            var root = GumService.Default.Root;
            if (root.Children.Contains(optionsOverlay.Visual))
                root.Children.Remove(optionsOverlay.Visual);
        }
    }

    private void OnViewModelOpened()
    {
        optionsOverlay.Visual.Visible = true;
        optionsOverlay.Visual.UpdateLayout();
        GumService.Default.Root.UpdateLayout();

        RefreshVolumeDisplays();
        RefreshSelectionVisuals();
    }

    private void OnViewModelClosed() => optionsOverlay.Visual.Visible = false;

    private void OnSelectionChanged() => RefreshSelectionVisuals();

    private void OnVolumeChanged(OptionsViewModel.Row _) => RefreshVolumeDisplays();

    private void RefreshVolumeDisplays()
    {
        OptionsMenu menu = optionsOverlay.OptionsMenuInstance;
        ApplyVolumeRow(menu.Master, viewModel.VolumeOf(OptionsViewModel.Row.Master));
        ApplyVolumeRow(menu.Ambient, viewModel.VolumeOf(OptionsViewModel.Row.Music));
        ApplyVolumeRow(menu.Sound_Effects, viewModel.VolumeOf(OptionsViewModel.Row.Sfx));
    }

    private static void ApplyVolumeRow(MenuItemAdjustable row, float volume)
    {
        volume = Math.Clamp(volume, 0f, 1f);
        int pct = (int)Math.Round(volume * 100f);
        row.MusicPercentage = $"{pct} %";

        Slider slider = row.SliderInstance;
        float track = slider.Rectangle.Width;
        float thumb = slider.ColoredRectangleInstance1.Width;
        float maxX = Math.Max(0f, track - thumb);
        row.SliderInstanceColoredRectangleInstance1X = volume * maxX;
    }

    private void RefreshSelectionVisuals()
    {
        OptionsMenu menu = optionsOverlay.OptionsMenuInstance;
        int i = viewModel.SelectionIndex;

        menu.Master.SelectedState = i == 0
            ? MenuItemAdjustable.Selected.isSelected
            : MenuItemAdjustable.Selected.notSelected;
        menu.Ambient.SelectedState = i == 1
            ? MenuItemAdjustable.Selected.isSelected
            : MenuItemAdjustable.Selected.notSelected;
        menu.Sound_Effects.SelectedState = i == 2
            ? MenuItemAdjustable.Selected.isSelected
            : MenuItemAdjustable.Selected.notSelected;

        menu.ItemFunction.Text = AdjustmentsHint;
    }
}
