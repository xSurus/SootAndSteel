using System.Collections.Generic;
using System.Linq;
using Gamelab.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Myra.Graphics2D;
using Myra.Graphics2D.UI;

namespace Gamelab.Screens;

/// <summary>
/// All ways a run can end on <see cref="FailScreen"/>. Only <see cref="GameplayScreen"/> calls <see cref="GamelabGame.SwitchToScreen"/> with that screen.
/// </summary>
public enum FailureReason
{
    /// <summary>Every player was stunned longer than <c>AllPlayersStunnedFailDelaySeconds</c>.</summary>
    AllPlayersKnockedOut,

    /// <summary>Temperature hit 0 with hull breaches while the coal oven was still burning (breach cooling only).</summary>
    TrainFrozenHullBreached,

    /// <summary>Temperature hit 0 with the coal oven off and no breached walls (engine-off cooling only).</summary>
    TrainFrozenFurnaceOut,

    /// <summary>Temperature hit 0 with both breached walls and the coal oven off.</summary>
    TrainFrozenBreachesAndFurnaceOut,

    /// <summary>Temperature hit 0 but state did not match the cases above (should be rare; keeps UI safe if rules change).</summary>
    TrainFrozenOther,
}

public class FailScreen(GamelabGame game, FailureReason reason) : AbstractGameScreen(game)
{
    private Desktop desktop;

    private static string GetReasonText(FailureReason r) => r switch
    {
        FailureReason.AllPlayersKnockedOut => "All players got knocked out.",
        FailureReason.TrainFrozenHullBreached => "The train froze — hull breaches let the cold in.",
        FailureReason.TrainFrozenFurnaceOut => "The train froze — the furnace went out.",
        FailureReason.TrainFrozenBreachesAndFurnaceOut => "The train froze — breaches and a dead furnace.",
        FailureReason.TrainFrozenOther => "The train froze over.",
        _ => ""
    };

    public override void LoadContent()
    {
        base.LoadContent();

        desktop = new Desktop();
        var root = new Panel
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };

        var stack = new VerticalStackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Spacing = 22
        };

        stack.Widgets.Add(new Label
        {
            Text = "You failed to deliver the coal",
            Font = Game.fontSystem.GetFont(64),
            TextColor = Color.White,
            HorizontalAlignment = HorizontalAlignment.Center
        });

        stack.Widgets.Add(new Label
        {
            Text = GetReasonText(reason),
            Font = Game.fontSystem.GetFont(44),
            TextColor = new Color(220, 200, 200),
            HorizontalAlignment = HorizontalAlignment.Center
        });

        stack.Widgets.Add(new Label
        {
            Text = "Press to return to menu",
            Font = Game.fontSystem.GetFont(40),
            TextColor = Color.LightBlue,
            HorizontalAlignment = HorizontalAlignment.Center
        });

        root.Widgets.Add(stack);
        desktop.Root = root;
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        if (Game.playerManager.Configs.Any(c => c.Input.IsPickupJustPressed() || c.Input.IsStartJustPressed()))
        {
            Game.SwitchToScreen(new global::Gamelab.MainMenuScreen(Game));
        }
    }

    public override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(new Color(20, 0, 0));
        spriteBatch.Begin(transformMatrix: viewportAdapter.GetScaleMatrix());
        desktop?.Render();
        spriteBatch.End();
        base.Draw(gameTime);
    }
}

