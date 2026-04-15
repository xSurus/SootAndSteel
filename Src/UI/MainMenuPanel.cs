using System;
using System.Collections.Generic;
using FontStashSharp;
using Gamelab.Players;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gamelab.UI;

public class MainMenuPanel(GamelabGame game, Action onStartSelected, Action onQuitSelected)
{
    private const int VolumeMenuIndex = 1;

    private readonly MenuList menuList = new([
        new MenuList.MenuEntry("Start Game", onStartSelected),
        new MenuList.MenuEntry("Music Volume", () => { }),
        new MenuList.MenuEntry("Quit Game", onQuitSelected),
    ]);

    private float MenuVolumeStep => GamelabGame.Instance.GameplayConfig.MenuVolumeStep;

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
            virtualScreenSize.Y - 660,
            virtualScreenSize.X,
            250);

        int firstItemY = panelRect.Y + 20;
        int itemSpacing = 70;

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
                    game.SetMusicVolume(game.MusicVolume - MenuVolumeStep);
                }
                else if (player.Input.IsRightJustPressed())
                {
                    game.SetMusicVolume(game.MusicVolume + MenuVolumeStep);
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
                    playerPos = new Vector2(labelPosition.X - 60 - pSize.X, yPosition);
                else if (playerIndex == 1)
                    playerPos = new Vector2(labelPosition.X - 140 - pSize.X, yPosition);
                else if (playerIndex == 2)
                    playerPos = new Vector2(labelPosition.X + labelSize.X + 60, yPosition);
                else if (playerIndex == 3)
                    playerPos = new Vector2(labelPosition.X + labelSize.X + 140, yPosition);

                spriteBatch.DrawString(buttonFont, pText, playerPos, Color.White);
            }
        }
    }
}