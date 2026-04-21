using System;
using Gamelab.Serialization;
using Gamelab.Services.Sound;
using Gamelab.UI;
using Gamelab.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gamelab.Screens;

public class MainMenuScreen(GamelabGame game) : AbstractGameScreen(game)
{
    private Logger logger = new Logger("MainMenuScreen");

    private const string MainMenuBackgroundAsset = "placeholder_main_menu_background";

    protected Texture2D bgTexture;
    private MainMenuPanel mainMenuPanel;
    private OptionsPanel optionsPanel;
    private ISoundService soundService;

    public override void LoadContent()
    {
        base.LoadContent();

        TryLoadBackgroundTexture();
        soundService = Services.GetService<ISoundService>();
        soundService.LoadSound(Sounds.MenuSelect);
        Action continueAction = SaveManager.HasSave() ? ContinueGame : null;
        optionsPanel = new OptionsPanel(Game);
        mainMenuPanel = new MainMenuPanel(Game, continueAction, StartNewGame, OpenOptions, Game.Exit);
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        if (optionsPanel.IsOpen)
        {
            optionsPanel.Update(Game.playerManager.Configs);
            return;
        }

        mainMenuPanel.Update(gameTime);

        foreach (var player in game.playerManager.Configs)
        {
            if (player.Input.IsUpJustPressed() || player.Input.IsDownJustPressed())
            {
                soundService.PlayOnce(Sounds.MenuSelect);
            }
        }
    }

    public override void Draw(GameTime gameTime)
    {
        // Scale to fit the virtual screen size to the actual window size
        spriteBatch.Begin(transformMatrix: viewportAdapter.GetScaleMatrix());

        // Rendering is done in the virtual screen space
        spriteBatch.Draw(bgTexture, new Rectangle(0, 0, virtualScreenSize.X, virtualScreenSize.Y), Color.White);

        mainMenuPanel.Draw(
            spriteBatch,
            virtualScreenSize,
            Game.fontSystem.GetFont(96),
            Game.fontSystem.GetFont(52));

        optionsPanel.Draw(
            spriteBatch,
            virtualScreenSize,
            Game.fontSystem.GetFont(72),
            Game.fontSystem.GetFont(40));

        spriteBatch.End();

        base.Draw(gameTime);
    }

    private void OpenOptions()
    {
        soundService.PlayOnce(Sounds.MenuSelect);
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

    private void TryLoadBackgroundTexture()
    {
        try
        {
            bgTexture = Game.Content.Load<Texture2D>(MainMenuBackgroundAsset);
        }
        catch
        {
            // Fallback keeps menu usable when the texture is missing from content.
            bgTexture = new Texture2D(GraphicsDevice, 1, 1);
            bgTexture.SetData([Color.White]);
        }
    }

    public override void Dispose()
    {
        soundService.UnloadSound(Sounds.MenuSelect);
        base.Dispose();
    }
}
