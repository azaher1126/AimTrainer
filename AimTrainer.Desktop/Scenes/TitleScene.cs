using Microsoft.Xna.Framework;
using AimTrainer.Desktop.Core;
using AimTrainer.Desktop.Core.Input;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace AimTrainer.Desktop.Scenes;

public class TitleScene : Scene
{
    private const string TITLE_TEXT = "AimTrainer";
    private const int BUTTON_COUNT = 4; // Play, Leaderboard, Settings, Quit

    private Rectangle _playButton;
    private Rectangle _leaderboardButton;
    private Rectangle _settingsButton;
    private Rectangle _quitButton;

    // Keyboard selection: 0 = Play, 1 = Leaderboard, 2 = Settings, 3 = Quit
    private int _selectedButton;

    public override void Initialize()
    {
        base.Initialize();
        GameCore.ExitOnEscape = true;
        _selectedButton = 0;

        CalculateLayout();
    }

    private void CalculateLayout()
    {
        int vw = GameCore.GraphicsDevice.Viewport.Width;
        int vh = GameCore.GraphicsDevice.Viewport.Height;
        int btnW = 300;
        int btnH = 50;
        int cx = vw / 2 - btnW / 2;
        int startY = vh / 2 + 20;
        int gap = 15;

        _playButton = new Rectangle(cx, startY, btnW, btnH);
        _leaderboardButton = new Rectangle(cx, startY + (btnH + gap), btnW, btnH);
        _settingsButton = new Rectangle(cx, startY + 2 * (btnH + gap), btnW, btnH);
        _quitButton = new Rectangle(cx, startY + 3 * (btnH + gap), btnW, btnH);
    }

    private void ActivateSelected()
    {
        switch (_selectedButton)
        {
            case 0: GameCore.ChangeScene(new ConfigScene()); break;
            case 1: GameCore.ChangeScene(new LeaderboardScene()); break;
            case 2: GameCore.ChangeScene(new SettingsScene()); break;
            case 3: GameCore.Instance.Exit(); break;
        }
    }

    public override void Update(GameTime gameTime)
    {
        // ── Keyboard navigation (wraps around) ──
        if (GameCore.Input.Keyboard.WasKeyJustPressed(Keys.Up))
            _selectedButton = (_selectedButton - 1 + BUTTON_COUNT) % BUTTON_COUNT;

        if (GameCore.Input.Keyboard.WasKeyJustPressed(Keys.Down))
            _selectedButton = (_selectedButton + 1) % BUTTON_COUNT;

        if (GameCore.Input.Keyboard.WasKeyJustPressed(Keys.Enter))
        {
            ActivateSelected();
            return;
        }

        // ── Mouse interaction ──
        if (GameCore.Input.Mouse.WasButtonJustPressed(MouseButton.Left))
        {
            int mx = GameCore.Input.Mouse.X;
            int my = GameCore.Input.Mouse.Y;

            if (_playButton.Contains(mx, my))
            { _selectedButton = 0; ActivateSelected(); }
            else if (_leaderboardButton.Contains(mx, my))
            { _selectedButton = 1; ActivateSelected(); }
            else if (_settingsButton.Contains(mx, my))
            { _selectedButton = 2; ActivateSelected(); }
            else if (_quitButton.Contains(mx, my))
            { _selectedButton = 3; ActivateSelected(); }
        }
    }

    public override void Draw(GameTime gameTime)
    {
        GameCore.GraphicsDevice.Clear(new Color(32, 40, 78, 255));

        int vw = GameCore.GraphicsDevice.Viewport.Width;
        int vh = GameCore.GraphicsDevice.Viewport.Height;
        float cx = vw / 2f;
        int mx = GameCore.Input.Mouse.X;
        int my = GameCore.Input.Mouse.Y;

        GameCore.SpriteBatch.Begin(samplerState: SamplerState.PointClamp);

        // Draw title with drop shadow
        Vector2 titleSize = GameCore.Assets.TitleFont.MeasureString(TITLE_TEXT);
        Vector2 titlePos = new Vector2(cx, vh * 0.2f);
        Vector2 titleOrigin = titleSize * 0.5f;

        Color dropShadowColor = Color.Black * 0.5f;
        GameCore.SpriteBatch.DrawString(GameCore.Assets.TitleFont, TITLE_TEXT, titlePos + new Vector2(10, 10), dropShadowColor, 0.0f, titleOrigin, 1.0f, SpriteEffects.None, 1.0f);
        GameCore.SpriteBatch.DrawString(GameCore.Assets.TitleFont, TITLE_TEXT, titlePos, Color.White, 0.0f, titleOrigin, 1.0f, SpriteEffects.None, 1.0f);

        // Draw buttons
        DrawButton("Play", _playButton, mx, my, _selectedButton == 0);
        DrawButton("Leaderboard", _leaderboardButton, mx, my, _selectedButton == 1);
        DrawButton("Settings", _settingsButton, mx, my, _selectedButton == 2);
        DrawButton("Quit", _quitButton, mx, my, _selectedButton == 3);

        GameCore.SpriteBatch.End();
    }

    private void DrawButton(string text, Rectangle rect, int mx, int my, bool keyboardSelected)
    {
        bool hovering = rect.Contains(mx, my);
        Color btnColor = (hovering || keyboardSelected) ? new Color(80, 120, 200) : new Color(50, 70, 140);
        GameCore.SpriteBatch.Draw(GameCore.Assets.BlankPixel, rect, btnColor);

        Vector2 textSize = GameCore.Assets.TextFont.MeasureString(text);
        Vector2 textPos = new Vector2(
            rect.X + (rect.Width - textSize.X) / 2,
            rect.Y + (rect.Height - textSize.Y) / 2);
        GameCore.SpriteBatch.DrawString(GameCore.Assets.TextFont, text, textPos, Color.White);
    }
}
