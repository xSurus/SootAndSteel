using System;
using System.Collections.Generic;
using FontStashSharp;
using Gamelab.Players;
using Gamelab.UI.ViewModels;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gamelab.UI;
public class MainMenuPanel
{
    private readonly GamelabGame game;
    private readonly MainMenuViewModel viewModel;

    public MainMenuViewModel ViewModel => viewModel;

    public MainMenuPanel(GamelabGame game, Action onContinueSelected, Action onStartNewSelected,
        Action onOptionsSelected, Action onQuitSelected)
    {
        this.game = game;

        var entries = new List<MainMenuViewModel.Entry>();
        if (onContinueSelected != null)
        {
            entries.Add(new MainMenuViewModel.Entry("Continue Game", onContinueSelected));
        }
        entries.Add(new MainMenuViewModel.Entry("New Game", onStartNewSelected));
        entries.Add(new MainMenuViewModel.Entry("Options", onOptionsSelected));
        entries.Add(new MainMenuViewModel.Entry("Quit Game", onQuitSelected));

        viewModel = new MainMenuViewModel(entries);
    }

    public void Update(GameTime gameTime)
    {
        foreach (PlayerConfiguration player in game.playerManager.Configs)
        {
            int pIndex = player.PlayerIndex;
            viewModel.EnsurePlayer(pIndex);

            var input = player.Input;
            if (input.IsUpJustPressed()) viewModel.MoveSelectionUp(pIndex);
            else if (input.IsDownJustPressed()) viewModel.MoveSelectionDown(pIndex);

            if (input.IsPickupJustPressed()) viewModel.Confirm(pIndex);
        }
    }

    public void Draw(SpriteBatch spriteBatch, Point virtualScreenSize, SpriteFontBase titleFont,
        SpriteFontBase buttonFont)
    {
        var panelRect = new Rectangle(
            0,
            virtualScreenSize.Y - 330,
            virtualScreenSize.X,
            250);

        IReadOnlyList<MainMenuViewModel.Entry> entries = viewModel.Entries;
        int firstItemY = panelRect.Y + (entries.Count > 3 ? -20 : 20);
        int itemSpacing = 85;

        for (int i = 0; i < entries.Count; i++)
        {
            DrawMenuItem(
                spriteBatch,
                buttonFont,
                panelRect,
                firstItemY + i * itemSpacing,
                entries[i].Label,
                i);
        }
    }

    private void DrawMenuItem(SpriteBatch spriteBatch, SpriteFontBase buttonFont, Rectangle panelRect, int yPosition,
        string label, int itemIndex)
    {
        Vector2 labelSize = buttonFont.MeasureString(label);
        Vector2 labelPosition = new(panelRect.Center.X - labelSize.X / 2f, yPosition);

        bool isSelectedByAnyone = viewModel.IsEntrySelectedByAnyone(itemIndex);
        Color color = isSelectedByAnyone ? Color.Gold * 0.95f : Color.White * 0.92f;

        spriteBatch.DrawString(buttonFont, label, labelPosition, color);

        foreach (KeyValuePair<int, int> selection in viewModel.PlayerSelections)
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
