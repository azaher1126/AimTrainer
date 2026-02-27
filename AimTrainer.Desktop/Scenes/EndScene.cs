using System;
using AimTrainer.Desktop.Core;
using AimTrainer.Desktop.Core.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace AimTrainer.Desktop.Scenes;

public class EndScene : Scene
{
    private readonly int _hits;
    private readonly int _misses;
    private readonly int _totalClicks;
    private readonly double _hitRate;

    // 1-second cooldown before allowing interaction
    private double _cooldownRemaining = 1.0;
    private bool _canInteract;

    // Buttons: 0 = Play Again, 1 = Leaderboard, 2 = Main Menu
    private const int BUTTON_COUNT = 3;
    private Rectangle _playAgainButton;
    private Rectangle _leaderboardButton;
    private Rectangle _mainMenuButton;
    private int _selectedButton;

    // When true, the score has already been recorded (returning from leaderboard).
    private readonly bool _scoreRecorded;

    public EndScene(int hits, int misses, int totalClicks, bool scoreRecorded = false)
    {
        _hits = hits;
        _misses = misses;
        _totalClicks = totalClicks;
        _hitRate = totalClicks > 0 ? (double)hits / totalClicks : 0.0;
        _scoreRecorded = scoreRecorded;
    }

    public override void Initialize()
    {
        base.Initialize();
        GameCore.ExitOnEscape = true;
        _selectedButton = 0; // Play Again focused by default

        // Record the score to the leaderboard (only on first visit)
        if (!_scoreRecorded)
        {
            GameCore.Leaderboard.TryAdd(
                GameCore.Config.PlayerName,
                _hits,
                _totalClicks,
                _hitRate,
                GameCore.Config.GameDurationSeconds);
            GameCore.Leaderboard.Save();
        }

        int vw = GameCore.GraphicsDevice.Viewport.Width;
        int vh = GameCore.GraphicsDevice.Viewport.Height;
        int btnW = 220;
        int btnH = 50;
        int gap = 20;
        int totalW = btnW * BUTTON_COUNT + gap * (BUTTON_COUNT - 1);
        int leftX = vw / 2 - totalW / 2;
        int btnY = vh / 2 + 140;

        _playAgainButton = new Rectangle(leftX, btnY, btnW, btnH);
        _leaderboardButton = new Rectangle(leftX + btnW + gap, btnY, btnW, btnH);
        _mainMenuButton = new Rectangle(leftX + 2 * (btnW + gap), btnY, btnW, btnH);
    }

    private void ActivateSelected()
    {
        switch (_selectedButton)
        {
            case 0: GameCore.ChangeScene(new ConfigScene()); break;
            case 1: GameCore.ChangeScene(new LeaderboardScene(new EndScene(_hits, _misses, _totalClicks, true))); break;
            case 2: GameCore.ChangeScene(new TitleScene()); break;
        }
    }

    public override void Update(GameTime gameTime)
    {
        if (!_canInteract)
        {
            _cooldownRemaining -= gameTime.ElapsedGameTime.TotalSeconds;
            if (_cooldownRemaining <= 0)
            {
                _canInteract = true;
            }
        }

        if (!_canInteract)
            return;

        // ── Keyboard navigation ──
        if (GameCore.Input.Keyboard.WasKeyJustPressed(Keys.Left) ||
            GameCore.Input.Keyboard.WasKeyJustPressed(Keys.Up))
        {
            _selectedButton = (_selectedButton - 1 + BUTTON_COUNT) % BUTTON_COUNT;
        }

        if (GameCore.Input.Keyboard.WasKeyJustPressed(Keys.Right) ||
            GameCore.Input.Keyboard.WasKeyJustPressed(Keys.Down))
        {
            _selectedButton = (_selectedButton + 1) % BUTTON_COUNT;
        }

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

            if (_playAgainButton.Contains(mx, my))
            { _selectedButton = 0; ActivateSelected(); }
            else if (_leaderboardButton.Contains(mx, my))
            { _selectedButton = 1; ActivateSelected(); }
            else if (_mainMenuButton.Contains(mx, my))
            { _selectedButton = 2; ActivateSelected(); }
        }
    }

    public override void Draw(GameTime gameTime)
    {
        GameCore.GraphicsDevice.Clear(new Color(32, 40, 78, 255));

        int vw = GameCore.GraphicsDevice.Viewport.Width;
        int vh = GameCore.GraphicsDevice.Viewport.Height;
        float cx = vw / 2f;

        GameCore.SpriteBatch.Begin(samplerState: SamplerState.PointClamp);

        // Title
        string title = "Round Over";
        Vector2 titleSize = GameCore.Assets.TitleFont.MeasureString(title);
        GameCore.SpriteBatch.DrawString(GameCore.Assets.TitleFont, title,
            new Vector2(cx - titleSize.X / 2, vh / 2f - 180), Color.White);

        // Stats
        string hitsText = $"Targets Hit: {_hits}";
        string missesText = $"Missed Clicks: {_misses}";
        string rateText = $"Hit Rate: {_hitRate:P1}";

        float statsY = vh / 2f - 40;
        DrawCentered(GameCore.Assets.TextFont, hitsText, cx, statsY);
        DrawCentered(GameCore.Assets.TextFont, missesText, cx, statsY + 35);
        DrawCentered(GameCore.Assets.TextFont, rateText, cx, statsY + 70);

        // Buttons (only shown after cooldown)
        if (_canInteract)
        {
            int mx = GameCore.Input.Mouse.X;
            int my = GameCore.Input.Mouse.Y;

            DrawButton("Play Again", _playAgainButton, mx, my, _selectedButton == 0);
            DrawButton("Leaderboard", _leaderboardButton, mx, my, _selectedButton == 1);
            DrawButton("Main Menu", _mainMenuButton, mx, my, _selectedButton == 2);
        }

        GameCore.SpriteBatch.End();
    }

    private void DrawCentered(SpriteFont font, string text, float cx, float y)
    {
        Vector2 size = font.MeasureString(text);
        GameCore.SpriteBatch.DrawString(font, text, new Vector2(cx - size.X / 2, y), Color.White);
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