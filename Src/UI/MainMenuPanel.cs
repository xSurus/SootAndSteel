using System;
using System.Collections.Generic;
using FontStashSharp;
using Gamelab.Players;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gamelab.UI;

public class MainMenuPanel
{
    private readonly GamelabGame game;
    private readonly MenuList menuList;
    private readonly int volumeMenuIndex;
    private readonly List<string> baseLabels = new();

    private float MenuVolumeStep => game.GameplayConfig.MenuVolumeStep;

    public MainMenuPanel(GamelabGame game, Action onContinueSelected, Action onStartNewSelected, Action onQuitSelected)
    {
        this.game = game;
        var entries = new List<MenuList.MenuEntry>();

        if (onContinueSelected != null)
        {
            entries.Add(new MenuList.MenuEntry("Continue Game", onContinueSelected));
            baseLabels.Add("Continue Game");
        }

        entries.Add(new MenuList.MenuEntry("New Game", onStartNewSelected));
        baseLabels.Add("New Game");

        volumeMenuIndex = entries.Count;
        entries.Add(new MenuList.MenuEntry("Music Volume", () => { }));
        baseLabels.Add("Music Volume");

        entries.Add(new MenuList.MenuEntry("Quit Game", onQuitSelected));
        baseLabels.Add("Quit Game");

        menuList = new MenuList([.. entries]);
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
            virtualScreenSize.Y - 330,
            virtualScreenSize.X,
            250);

        int firstItemY = panelRect.Y + (baseLabels.Count > 3 ? -20 : 20);
        int itemSpacing = 85;

        for (int i = 0; i < baseLabels.Count; i++)
        {
            string label = baseLabels[i];

            if (i == volumeMenuIndex)
            {
                int volumePercent = (int)(game.MusicVolume * 100f);
                label = $"Music Volume   < {volumePercent}% >";
            }

            DrawMenuItem(
                spriteBatch,
                buttonFont,
                panelRect,
                firstItemY + i * itemSpacing,
                label,
                i);
        }
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

            if (selectedIndex == volumeMenuIndex)
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