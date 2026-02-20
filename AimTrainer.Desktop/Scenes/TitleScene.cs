using Microsoft.Xna.Framework;
using AimTrainer.Desktop.Core;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace AimTrainer.Desktop.Scenes;

public class TitleScene : Scene
{
    private const string TITLE_TEXT = "AimTrainer";

    public override void Initialize()
    {
        base.Initialize();
        GameCore.ExitOnEscape = true;
    }

    public override void Draw(GameTime gameTime)
    {
        GameCore.GraphicsDevice.Clear(new Color(32, 40, 78, 255));

        int vw = GameCore.GraphicsDevice.Viewport.Width;
        int vh = GameCore.GraphicsDevice.Viewport.Height;
        float cx = vw / 2f;

        GameCore.SpriteBatch.Begin(samplerState: SamplerState.PointClamp);

        // Draw title with drop shadow
        Vector2 titleSize = GameCore.Assets.TitleFont.MeasureString(TITLE_TEXT);
        Vector2 titlePos = new Vector2(cx, vh * 0.2f);
        Vector2 titleOrigin = titleSize * 0.5f;

        Color dropShadowColor = Color.Black * 0.5f;
        GameCore.SpriteBatch.DrawString(GameCore.Assets.TitleFont, TITLE_TEXT, titlePos + new Vector2(10, 10), dropShadowColor, 0.0f, titleOrigin, 1.0f, SpriteEffects.None, 1.0f);
        GameCore.SpriteBatch.DrawString(GameCore.Assets.TitleFont, TITLE_TEXT, titlePos, Color.White, 0.0f, titleOrigin, 1.0f, SpriteEffects.None, 1.0f);

        GameCore.SpriteBatch.End();
    }
}
