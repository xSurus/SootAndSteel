using System;
using System.Collections.Generic;
using System.Linq;
using Gamelab.Components;
using Gamelab.Players;
using Gamelab.Services.Sound;
using MonoGameGum;

namespace Gamelab.UI;

public class PauseMenuController
{
    private readonly ISoundService soundService = GamelabGame.Instance.Services.GetService<ISoundService>();
    private readonly PauseOverlay overlay;
    private readonly MenuButtonWithIcon[] selectableButtons;
    private int selectionIndex;

    public bool IsPaused { get; private set; }
    public OptionsPanel OptionsPanel { get; }
    public ControlsOverlay ControlsPanel { get; }

    public event Action OnExitRequested;

    public PauseMenuController()
    {
        overlay = new PauseOverlay();
        overlay.AddToRoot();
        overlay.Visual.Visible = false;
        selectableButtons = [overlay.Continue, overlay.Options, overlay.Controls, overlay.Exit];
        OptionsPanel = new OptionsPanel(GamelabGame.Instance);
        OptionsPanel.OnClosed += () => overlay.Visual.Visible = IsPaused;
        ControlsPanel = new ControlsOverlay();
        UpdateSelectionVisuals();
    }

    public bool IsToggleRequested(IReadOnlyList<PlayerConfiguration> playerConfigs)
    {
        if (OptionsPanel.IsOpen) return false;
        return playerConfigs.Any(player => player.Input.IsPauseJustPressed());
    }

    public void Toggle()
    {
        IsPaused = !IsPaused;
        selectionIndex = 0;
        overlay.Visual.Visible = IsPaused;
        if (!IsPaused && OptionsPanel.IsOpen)
        {
            OptionsPanel.Close();
        }

        if (!IsPaused && ControlsPanel.IsOpen)
        {
            ControlsPanel.ClosePanel();
        }

        UpdateSelectionVisuals();
    }

    public void Update(IReadOnlyList<PlayerConfiguration> playerConfigs)
    {
        if (!IsPaused) return;

        if (OptionsPanel.IsOpen)
        {
            overlay.Visual.Visible = false;
            OptionsPanel.Update(playerConfigs);
            if (!OptionsPanel.IsOpen) overlay.Visual.Visible = IsPaused;
            return;
        }

        if (ControlsPanel.IsOpen)
        {
            overlay.Visual.Visible = false;
            ControlsPanel.Update(playerConfigs);
            if (!ControlsPanel.IsOpen) overlay.Visual.Visible = IsPaused;
            return;
        }

        bool moveUp = false;
        bool moveDown = false;
        bool confirm = false;

        foreach (PlayerConfiguration player in playerConfigs)
        {
            moveUp |= player.Input.IsUpJustPressed();
            moveDown |= player.Input.IsDownJustPressed();
            confirm |= player.Input.IsPickupJustPressed();
        }

        if (moveUp)
        {
            selectionIndex = (selectionIndex - 1 + selectableButtons.Length) % selectableButtons.Length;
            UpdateSelectionVisuals();
            soundService.PlayOnce(Sounds.MenuSelect);
        }
        else if (moveDown)
        {
            selectionIndex = (selectionIndex + 1) % selectableButtons.Length;
            UpdateSelectionVisuals();
            soundService.PlayOnce(Sounds.MenuSelect);
        }

        if (!confirm) return;

        soundService.PlayOnce(Sounds.MenuSelect);

        switch (selectionIndex)
        {
            case 0:
                Toggle();
                break;
            case 1:
                OptionsPanel.Open();
                overlay.Visual.Visible = false;
                break;
            case 2:
                ControlsPanel.OpenPanel();
                overlay.Visual.Visible = false;
                break;
            case 3:
                OnExitRequested?.Invoke();
                break;
        }
    }

    private void UpdateSelectionVisuals()
    {
        for (int i = 0; i < selectableButtons.Length; i++)
        {
            selectableButtons[i].SelectedState = i == selectionIndex
                ? MenuButtonWithIcon.Selected.isSelected
                : MenuButtonWithIcon.Selected.notSelected;
        }
    }

    public void Dispose()
    {
        OptionsPanel.Dispose();
        GumService.Default.Root.Children.Remove(overlay.Visual);
    }
}