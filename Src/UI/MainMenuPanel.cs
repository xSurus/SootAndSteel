using System;
using System.Collections.Generic;
using FontStashSharp;
using Gamelab.Config;
using Gamelab.Players;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gamelab.UI;

public class MainMenuPanel
{
    private const int VolumeMenuIndex = 1;

    private readonly GamelabGame game;
    private readonly GameplayConfig gameplayConfig;
    private readonly MenuList menuList;

    public MainMenuPanel(GamelabGame game, GraphicsDevice graphicsDevice, Action onStartSelected, Action onQuitSelected,
        GameplayConfig gameplayConfig)
    {
        this.game = game;
        this.gameplayConfig = gameplayConfig;
        menuList = new MenuList([
            new MenuList.MenuEntry("Start Game", onStartSelected),
            new MenuList.MenuEntry("Music Volume", () => { }),
            new MenuList.MenuEntry("Quit Game", onQuitSelected),
        ]);
    }

    public void Update(GameTime gameTime)
    {
        menuList.Update(game.playerManager.Configs);
        HandleVolumeInput();
    }

    public void Draw(SpriteBatch spriteBatch, Point virtualScreenSize, SpriteFontBase titleFont,
        SpriteFontBase buttonFont)
    {
        // Keep menu controls lower so they don't cover key parts of the background image.
        var panelRect = new Rectangle(
            0,
            virtualScreenSize.Y - gameplayConfig.MenuPanelBottomOffsetPixels,
            virtualScreenSize.X,
            gameplayConfig.MenuPanelHeightPixels);

        int firstItemY = panelRect.Y + gameplayConfig.MenuPanelFirstItemOffsetYPixels;
        int itemSpacing = gameplayConfig.MenuPanelItemSpacingPixels;

        DrawMenuItem(
            spriteBatch,
            buttonFont,
            panelRect,
            firstItemY + 0 * itemSpacing,
            "Start Game",
            0);

        int volumePercent = (int)(game.MusicVolume * 100f);
        DrawMenuItem(
            spriteBatch,
            buttonFont,
            panelRect,
            firstItemY + 1 * itemSpacing,
            $"Music Volume   < {volumePercent}% >",
            1);

        DrawMenuItem(
            spriteBatch,
            buttonFont,
            panelRect,
            firstItemY + 2 * itemSpacing,
            "Quit Game",
            2);
    }

    private void HandleVolumeInput()
    {
        foreach (PlayerConfiguration player in game.playerManager.Configs)
        {
            int pIndex = player.PlayerIndex;

            if (!menuList.PlayerSelections.TryGetValue(pIndex, out int selectedIndex))
            {
                continue;
            }

            // only listen to Left/Right if specific player is doing the input
            if (selectedIndex == VolumeMenuIndex)
            {
                if (player.Input.IsLeftJustPressed())
                {
                    game.SetMusicVolume(game.MusicVolume - gameplayConfig.MenuVolumeStep);
                }
                else if (player.Input.IsRightJustPressed())
                {
                    game.SetMusicVolume(game.MusicVolume + gameplayConfig.MenuVolumeStep);
                }
            }
        }
    }

    private void DrawMenuItem(SpriteBatch spriteBatch, SpriteFontBase buttonFont, Rectangle panelRect, int yPosition,
        string label, int itemIndex)
    {
        Vector2 labelSize = buttonFont.MeasureString(label);
        Vector2 labelPosition = new(panelRect.Center.X - labelSize.X / 2f, yPosition);

        bool isSelectedByAnyone = menuList.PlayerSelections.ContainsValue(itemIndex);
        Color color = isSelectedByAnyone ? Color.Gold * 0.95f : Color.White * 0.92f;

        spriteBatch.DrawString(buttonFont, label, labelPosition, color);

        foreach (KeyValuePair<int, int> selection in menuList.PlayerSelections)
        {
            int playerIndex = selection.Key;
            int selectedItemIndex = selection.Value;

            if (selectedItemIndex == itemIndex)
            {
                string pText = $"P{playerIndex + 1}";
                Vector2 pSize = buttonFont.MeasureString(pText);
                Vector2 playerPos = Vector2.Zero;

                // P1 and P2 on the left, P3 and P4 on the right
                if (playerIndex == 0)
                    playerPos = new Vector2(labelPosition.X - gameplayConfig.MenuPlayerMarkerNearOffsetPixels - pSize.X, yPosition);
                else if (playerIndex == 1)
                    playerPos = new Vector2(labelPosition.X - gameplayConfig.MenuPlayerMarkerFarOffsetPixels - pSize.X, yPosition);
                else if (playerIndex == 2)
                    playerPos = new Vector2(labelPosition.X + labelSize.X + gameplayConfig.MenuPlayerMarkerNearOffsetPixels, yPosition);
                else if (playerIndex == 3)
                    playerPos = new Vector2(labelPosition.X + labelSize.X + gameplayConfig.MenuPlayerMarkerFarOffsetPixels, yPosition);

                spriteBatch.DrawString(buttonFont, pText, playerPos, Color.White);
            }
        }
    }
}