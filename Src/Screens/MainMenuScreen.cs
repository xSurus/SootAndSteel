using System.Collections.Generic;
using FontStashSharp;
using Gamelab.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Gamelab.Screens;

public class MainMenuScreen(GamelabGame game) : AbstractGameScreen(game)
{
    private const string MainMenuBackgroundAsset = "placeholder_main_menu_background";
    private const string MainMenuSongAsset = "placeholder_main_menu_theme";
    protected Texture2D bgTexture;
    private MainMenuPanel mainMenuPanel;
    private Song mainMenuSong;

    public override void LoadContent()
    {
        base.LoadContent();

        TryLoadBackgroundTexture();
        mainMenuPanel = new MainMenuPanel(Game, GraphicsDevice, StartGame, Game.Exit, Game.GameplayConfig);
        TryStartMainMenuMusic();
    }

    protected override void Update(GameTime gameTime, KeyboardState keyboard, Dictionary<int, GamePadState> gamePads)
    {
        mainMenuPanel.Update(gameTime);
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

        spriteBatch.End();

        base.Draw(gameTime);
    }

    private void StartGame()
    {
        MediaPlayer.Stop();
        Game.SwitchToScreen(new GameplayScreen(Game));
    }

    private void TryStartMainMenuMusic()
    {
        try
        {
            if (MediaPlayer.State == MediaState.Playing)
            {
                MediaPlayer.IsRepeating = true;
                Game.SetMusicVolume(Game.MusicVolume);
                return;
            }

            // Requires this song to be added to Content.mgcb and built as a Song.
            mainMenuSong = Game.Content.Load<Song>(MainMenuSongAsset);
            MediaPlayer.IsRepeating = true;
            Game.SetMusicVolume(Game.MusicVolume);
            MediaPlayer.Play(mainMenuSong);
        }
        catch
        {
            // Keep the menu usable even when music asset is not added yet.
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
}