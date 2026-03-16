using System.Collections.Generic;
using Gamelab.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using FontStashSharp;


namespace Gamelab.Screens;

public class MainMenuScreen(GamelabGame game) : AbstractGameScreen(game)
{
    private const string MainMenuBackgroundAsset = "placeholder_main_menu_background";
    private const string MainMenuSongAsset = "placeholder_main_menu_theme";
    protected Texture2D bgTexture;
    private MainMenuPanel mainMenuPanel;
    private Song mainMenuSong;

    class ExampleData
    {
        public int size;
        public Vector2 position;
    }

    private ExampleData exampleData;
    public override void LoadContent()
    {
        base.LoadContent();

        TryLoadBackgroundTexture();
        mainMenuPanel = new MainMenuPanel(Game, GraphicsDevice, StartGame, Game.Exit);
        TryStartMainMenuMusic();

        LoadExampleData();
    }

    protected override void Update(GameTime gameTime, KeyboardState keyboard, Dictionary<int, GamePadState> gamePads)
    {
        mainMenuPanel.Update(gameTime);

        if (Game.IsDebug && keyboard.IsKeyDown(Keys.F5))
        {
            LoadExampleData();
        }

        if (Game.IsDebug && keyboard.IsKeyDown(Keys.F12))
        {
            SaveExampleData();
        }
    }

    public override void Draw(GameTime gameTime)
    {
        // Scale to fit the virtual screen size to the actual window size
        spriteBatch.Begin(transformMatrix: viewportAdapter.GetScaleMatrix());

        // Rendering is done in the virtual screen space
        spriteBatch.Draw(bgTexture, new Rectangle(0, 0, virtualScreenSize.X, virtualScreenSize.Y), Color.White);

        SpriteFontBase font = Game.fontSystem.GetFont(exampleData.size);
        mainMenuPanel.Draw(
            spriteBatch,
            virtualScreenSize,
            Game.fontSystem.GetFont(96),
            Game.fontSystem.GetFont(52));

        spriteBatch.End();

        base.Draw(gameTime);
    }

    protected void LoadExampleData()
    {
        exampleData = Game.jsonLoader.LoadJson<ExampleData>("example.json");
    }

    protected void SaveExampleData()
    {
        var newExampleData = new ExampleData {
            size = 42,
            position = new Vector2(100, 100),
        };

        Game.jsonLoader.SaveJson("example2.json", newExampleData);
    }

    private void StartGame()
    {
        MediaPlayer.Stop();
        Game.SwitchToScreen(new GameplayScene(Game));
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
