using System.Collections.Generic;
using System.Linq;
using Gamelab.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Myra.Graphics2D;
using Myra.Graphics2D.UI;

namespace Gamelab.Screens;

public class FailScreen(GamelabGame game) : AbstractGameScreen(game)
{
    private Desktop desktop;
    private Label titleLabel;
    private Label hintLabel;

    public override void LoadContent()
    {
        base.LoadContent();

        desktop = new Desktop();
        var root = new Panel
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };

        titleLabel = new Label
        {
            Text = "You failed to deliver the coal",
            Font = Game.fontSystem.GetFont(64),
            TextColor = Color.White,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, -80, 0, 0)
        };

        hintLabel = new Label
        {
            Text = "Press to return to menu",
            Font = Game.fontSystem.GetFont(40),
            TextColor = Color.LightBlue,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 20, 0, 0)
        };

        root.Widgets.Add(titleLabel);
        root.Widgets.Add(hintLabel);
        desktop.Root = root;
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        if (Game.playerManager.Configs.Any(c => c.Input.IsPickupJustPressed() || c.Input.IsStartJustPressed()))
        {
            Game.SwitchToScreen(new MainMenuScreen(Game));
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

