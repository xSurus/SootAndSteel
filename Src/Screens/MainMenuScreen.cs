using System;
using System.Collections.Generic;
using System.Linq;
using Gamelab.Components;
using Gamelab.Input;
using Gamelab.Players;
using Gamelab.Screens;
using Gamelab.Serialization;
using Gamelab.Services.Sound;
using Gamelab.Particles;
using Gamelab.Services.Vfx;
using Gamelab.UI;
using Gamelab.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGameGum;

namespace Gamelab;

/// <summary>
/// Main menu MonoGame screen. The Gum Forms UI is <see cref="Screens.MainMenuScreen"/> (generated); this type
/// stays in namespace <see cref="Gamelab"/> so it does not collide with Gum when you regenerate code, mirroring
/// the pattern used by <see cref="JoinScreen"/>.
/// </summary>
public sealed class MainMenuScreen(GamelabGame game) : Screens.GamelabGameScreen(game)
{
    private readonly Logger logger = new("MainMenuScreen");

    private Screens.MainMenuScreen menuUi;
    private MainMenuButton[] buttons;
    private Action[] actions;
    private int selectedIndex;

    private OptionsPanel optionsPanel;
    private ISoundService soundService;

    private static GumService Gum => GumService.Default;

    public override void Initialize()
    {
        base.Initialize();
        Gum.Root.Children.Clear();
    }

    public override void LoadContent()
    {
        base.LoadContent();

        menuUi = new Screens.MainMenuScreen();
        menuUi.AddToRoot();

        soundService = Services.GetService<ISoundService>();
        soundService.LoadSound(Sounds.MenuSelect);
        soundService.LoadSound(Sounds.AmbientSong);
        soundService.GetSoundInstance(Sounds.AmbientSong)?.Start();
        Services.GetService<IVfxService>().AddContinuous(ParticleFactory.CreateSnowstorm());

        optionsPanel = new OptionsPanel(Game);

        BuildButtonList();
        ApplySelectionStates();
    }

    public override void UnloadContent()
    {
        Services.GetService<IVfxService>().ClearAll();
        Gum.Root.Children.Clear();
        menuUi = null;
        buttons = null;
        actions = null;
        base.UnloadContent();
    }

    protected override void Update(GameTime gameTime, KeyboardState keyboard, Dictionary<int, GamePadState> gamePads)
    {
        base.Update(gameTime, keyboard, gamePads);

        if (optionsPanel.IsOpen)
        {
            optionsPanel.Update(Game.playerManager.Configs);
            return;
        }

        // Player one drives the menu. Fall back to whoever has the lowest PlayerIndex if P1 hasn't joined
        // (e.g. keyboard-only run before anyone presses Start on a gamepad).
        PlayerConfiguration driver = Game.playerManager.Configs
            .OrderBy(c => c.PlayerIndex)
            .FirstOrDefault();
        if (driver == null) return;

        IInputProvider input = driver.Input;

        if (input.IsUpJustPressed())
        {
            selectedIndex = (selectedIndex - 1 + buttons.Length) % buttons.Length;
            soundService.PlayOnce(Sounds.MenuSelect);
            ApplySelectionStates();
        }
        else if (input.IsDownJustPressed())
        {
            selectedIndex = (selectedIndex + 1) % buttons.Length;
            soundService.PlayOnce(Sounds.MenuSelect);
            ApplySelectionStates();
        }

        if (input.IsPickupJustPressed() && selectedIndex >= 0 && selectedIndex < actions.Length)
        {
            soundService.PlayOnce(Sounds.MenuSelect);
            actions[selectedIndex].Invoke();
        }

        if (Keyboard.GetState().IsKeyDown(Keys.LeftShift) && Keyboard.GetState().IsKeyDown(Keys.R))
        {
            Game.SwitchToScreen(new ShootingRangeScreen(Game));
        }
    }

    public override void Draw(GameTime gameTime)
    {
        // Snow under Gum UI; title/background is the Gum "Background" sprite (Title.png).
        spriteBatch.Begin(transformMatrix: viewportAdapter.GetScaleMatrix());
        Services.GetService<IVfxService>().Render(spriteBatch);
        spriteBatch.End();

        Gum.Draw();

        base.Draw(gameTime);
    }

    private void BuildButtonList()
    {
        bool hasSave = SaveManager.HasSave();

        // Hide the Continue slot when there is no save; the stack container collapses the empty row.
        menuUi.ContinueButton.Visual.Visible = hasSave;

        var buttonList = new List<MainMenuButton>(4);
        var actionList = new List<Action>(4);

        if (hasSave)
        {
            buttonList.Add(menuUi.ContinueButton);
            actionList.Add(ContinueGame);
        }


        buttonList.Add(menuUi.NewGameButton);
        actionList.Add(StartNewGame);

        buttonList.Add(menuUi.OptionsButton);
        actionList.Add(OpenOptions);

        buttonList.Add(menuUi.QuitButton);
        actionList.Add(Game.Exit);

        buttons = buttonList.ToArray();
        actions = actionList.ToArray();

        selectedIndex = 0;
    }

    private void ApplySelectionStates()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].SelectionState = i == selectedIndex
                ? MainMenuButton.Selection.Selected
                : MainMenuButton.Selection.Unselected;
        }
    }

    private void OpenOptions()
    {
        optionsPanel.Open();
    }

    private void StartNewGame()
    {
        SaveManager.DeleteSave();
        Game.CurrentRun = new RunSession();
        Game.SwitchToScreen(new HubScreen(Game));
    }

    private void ContinueGame()
    {
        RunSession loadedSession = SaveManager.LoadRun();
        if (loadedSession != null)
        {
            Game.CurrentRun = loadedSession;
            Game.SwitchToScreen(new HubScreen(Game));
        }
        else
        {
            logger.Warning("Save file corrupted or missing. Defaulting to New Game.");
            StartNewGame();
        }

    }

    public override void Dispose()
    {
        optionsPanel?.Dispose();
        soundService?.UnloadSound(Sounds.MenuSelect);
        base.Dispose();
    }
}
