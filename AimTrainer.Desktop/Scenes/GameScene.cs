using System;
using System.Collections.Generic;
using AimTrainer.Desktop.Core;
using AimTrainer.Desktop.Core.Input;
using AimTrainer.Desktop.Sprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AimTrainer.Desktop.Scenes;

public class GameScene : Scene
{
    // Background color used throughout the scene.
    private static readonly Color BackgroundColor = new Color(32, 40, 78, 255);

    private TextureRegion _targetTexture;
    private List<Target> _targets;

    // Config-driven values
    private int _targetCount;
    private int _minSize;
    private int _maxSize;
    private int _gameDuration;

    // Scoring
    private int _totalClicks;
    private int _hitClicks;

    // Timer
    private double _remainingSeconds;
    private bool _timerStarted = false;

    public override void Initialize()
    {
        base.Initialize();

        GameCore.ExitOnEscape = false;

        var config = GameCore.Config;
        _targetCount = config.TargetCount;
        _minSize = config.MinTargetSize;
        _maxSize = config.MaxTargetSize;
        _gameDuration = config.GameDurationSeconds;
        
        _remainingSeconds = _gameDuration;

        _totalClicks = 0;
        _hitClicks = 0;

        _targets = new List<Target>(_targetCount);
    }

    public override void LoadContent()
    {
        _targetTexture = new TextureRegion(GameCore.Assets.BlankPixel, 0, 0, 1, 1);
    }

    public override void Update(GameTime gameTime)
    {
        if (_timerStarted) {
            // Count down
            _remainingSeconds -= gameTime.ElapsedGameTime.TotalSeconds;

            if (_remainingSeconds <= 0)
            {
                _remainingSeconds = 0;
                int misses = _totalClicks - _hitClicks;
                GameCore.ChangeScene(new EndScene(_hitClicks, misses, _totalClicks));
                return;
            }
        }

        if (GameCore.Input.Mouse.WasButtonJustPressed(MouseButton.Left))
        {
            if (!_timerStarted) _timerStarted = true;

            _totalClicks++;

            // Remove only the topmost (last-drawn) target under the cursor.
            // Targets are drawn in list order, so higher indices are on top.
            for (int i = _targets.Count - 1; i >= 0; i--)
            {
                if (IsTargetClicked(_targets[i]))
                {
                    _targets.RemoveAt(i);
                    _hitClicks++;
                    break;
                }
            }
        }

        // Replenish targets
        while (_targets.Count < _targetCount)
        {
            _targets.Add(CreateTarget());
        }
    }

    public override void Draw(GameTime gameTime)
    {
        GameCore.GraphicsDevice.Clear(BackgroundColor);

        GameCore.SpriteBatch.Begin();

        foreach (var target in _targets)
        {
            target.Draw(GameCore.SpriteBatch);
        }

        // Draw translucent HUD
        DrawHud();

        GameCore.SpriteBatch.End();
    }

    private void DrawHud()
    {
        int seconds = (int)Math.Ceiling(_remainingSeconds);
        string scoreText = $"Score: {_hitClicks}";
        string timerText = $"Time: {seconds}s";

        Color hudColor = Color.White * 0.35f;

        GameCore.SpriteBatch.DrawString(GameCore.Assets.TextFont, scoreText, new Vector2(12, 10), hudColor);
        GameCore.SpriteBatch.DrawString(GameCore.Assets.TextFont, timerText, new Vector2(12, 40), hudColor);
    }

    private bool IsTargetClicked(Target target)
    {
        var minX = target.Position.X;
        var maxX = target.Position.X + target.Width;

        if (GameCore.Input.Mouse.X < minX || GameCore.Input.Mouse.X > maxX) return false;

        var minY = target.Position.Y;
        var maxY = target.Position.Y + target.Height;

        return GameCore.Input.Mouse.Y >= minY && GameCore.Input.Mouse.Y <= maxY;
    }

    private Target CreateTarget()
    {
        var color = GetContrastingColor();
        var size = GetRandomSize();
        var position = GetRandomPosition(size);
        return new Target(_targetTexture, color, position, size);
    }

    /// <summary>
    /// Generates a random color that has enough perceptual distance from the background
    /// so the target is always easy to see.
    /// </summary>
    private Color GetContrastingColor()
    {
        const int maxAttempts = 50;
        const double minDistance = 120.0; // Euclidean RGB distance threshold

        double bgR = BackgroundColor.R;
        double bgG = BackgroundColor.G;
        double bgB = BackgroundColor.B;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            int r = Random.Shared.Next(256);
            int g = Random.Shared.Next(256);
            int b = Random.Shared.Next(256);

            double distance = Math.Sqrt(
                (r - bgR) * (r - bgR) +
                (g - bgG) * (g - bgG) +
                (b - bgB) * (b - bgB));

            if (distance >= minDistance)
            {
                return new Color(r, g, b);
            }
        }

        // Fallback – guaranteed high contrast
        return Color.Yellow;
    }

    private int GetRandomSize()
    {
        return Random.Shared.Next(_minSize, _maxSize + 1);
    }

    private Vector2 GetRandomPosition(int size)
    {
        int maxX = Math.Max(1, GameCore.GraphicsDevice.Viewport.Width - size);
        int maxY = Math.Max(1, GameCore.GraphicsDevice.Viewport.Height - size);

        return new Vector2(Random.Shared.Next(maxX), Random.Shared.Next(maxY));
    }
}
