using System;
using AimTrainer.Desktop.Core;
using AimTrainer.Desktop.Core.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace AimTrainer.Desktop.Scenes;

/// <summary>
/// Settings menu for adjusting resolution, framerate, and window mode.
/// Fully navigable with arrow keys + Enter, as well as mouse.
/// </summary>
public class SettingsScene : Scene
{
    // Available resolution presets
    private static readonly (int w, int h)[] Resolutions =
    {
        (1280, 720),
        (1366, 768),
        (1600, 900),
        (1920, 1080),
        (2560, 1440),
        (3840, 2160)
    };

    // Available framerate presets (0 = Unlimited)
    private static readonly int[] FrameRates = { 60, 120, 144, 165, 240, 0 };

    private int _resolutionIndex;
    private int _frameRateIndex;
    private bool _isFullScreen;

    // Keyboard-navigable items:
    // 0 = Resolution, 1 = Framerate, 2 = Display, 3 = Save, 4 = Back
    // Up/Down navigates value rows (0-2) and enters/exits the button row (3-4).
    // Left/Right switches between Save and Back when on the button row.
    private const int VALUE_ROW_COUNT = 3;
    private int _selectedItem;

    private Rectangle[] _leftArrows;
    private Rectangle[] _rightArrows;
    private Rectangle _saveButton;
    private Rectangle _backButton;

    public override void Initialize()
    {
        base.Initialize();
        GameCore.ExitOnEscape = false;

        var settings = GameCore.Settings;

        // Find current resolution index
        _resolutionIndex = 0;
        for (int i = 0; i < Resolutions.Length; i++)
        {
            if (Resolutions[i].w == settings.ResolutionWidth && Resolutions[i].h == settings.ResolutionHeight)
            {
                _resolutionIndex = i;
                break;
            }
        }

        // Find current framerate index
        _frameRateIndex = 0;
        for (int i = 0; i < FrameRates.Length; i++)
        {
            if (FrameRates[i] == settings.FrameRate)
            {
                _frameRateIndex = i;
                break;
            }
        }

        _isFullScreen = settings.IsFullScreen;
        _selectedItem = 0;

        CalculateLayout();
    }

    private void CalculateLayout()
    {
        int vw = GameCore.GraphicsDevice.Viewport.Width;
        int vh = GameCore.GraphicsDevice.Viewport.Height;

        // Total row width: label area (200) + gap (20) + arrow (30) + value area (250) + arrow (30) = 530
        // Center the whole block by computing a left edge offset
        int totalRowWidth = 530;
        int rowLeftEdge = (vw - totalRowWidth) / 2;

        int arrowSize = 30;
        int startY = vh / 2 - 60;
        int rowHeight = 45;
        int valueX = rowLeftEdge + 200 + 20; // after label area + gap

        _leftArrows = new Rectangle[VALUE_ROW_COUNT];
        _rightArrows = new Rectangle[VALUE_ROW_COUNT];

        for (int i = 0; i < VALUE_ROW_COUNT; i++)
        {
            int y = startY + i * rowHeight;
            _leftArrows[i] = new Rectangle(valueX, y, arrowSize, arrowSize);
            _rightArrows[i] = new Rectangle(valueX + 250, y, arrowSize, arrowSize);
        }

        int btnW = 200;
        int btnH = 45;
        int btnY = startY + VALUE_ROW_COUNT * rowHeight + 20;
        _saveButton = new Rectangle(vw / 2 - btnW - 10, btnY, btnW, btnH);
        _backButton = new Rectangle(vw / 2 + 10, btnY, btnW, btnH);
    }

    // ── Shared logic for adjusting a value row ──
    private void AdjustLeft(int row)
    {
        switch (row)
        {
            case 0: _resolutionIndex = (_resolutionIndex - 1 + Resolutions.Length) % Resolutions.Length; break;
            case 1: _frameRateIndex = (_frameRateIndex - 1 + FrameRates.Length) % FrameRates.Length; break;
            case 2: _isFullScreen = !_isFullScreen; break;
        }
    }

    private void AdjustRight(int row)
    {
        switch (row)
        {
            case 0: _resolutionIndex = (_resolutionIndex + 1) % Resolutions.Length; break;
            case 1: _frameRateIndex = (_frameRateIndex + 1) % FrameRates.Length; break;
            case 2: _isFullScreen = !_isFullScreen; break;
        }
    }

    private void ActivateSave()
    {
        var settings = GameCore.Settings;
        settings.ResolutionWidth = Resolutions[_resolutionIndex].w;
        settings.ResolutionHeight = Resolutions[_resolutionIndex].h;
        settings.FrameRate = FrameRates[_frameRateIndex];
        settings.IsFullScreen = _isFullScreen;
        settings.Save();
        settings.Apply();
        CalculateLayout();
    }

    private void ActivateBack()
    {
        GameCore.ChangeScene(new TitleScene());
    }

    public override void Update(GameTime gameTime)
    {
        // Escape always goes back
        if (GameCore.Input.Keyboard.WasKeyJustPressed(Keys.Escape))
        {
            ActivateBack();
            return;
        }

        // ── Keyboard navigation (wraps around) ──
        // Up/Down moves between value rows and the button row as a single group.
        // The two buttons are only navigable between each other via Left/Right.
        if (GameCore.Input.Keyboard.WasKeyJustPressed(Keys.Up))
        {
            if (_selectedItem >= VALUE_ROW_COUNT) // on button row → last value row
                _selectedItem = VALUE_ROW_COUNT - 1;
            else if (_selectedItem > 0)
                _selectedItem--;
            else // wrap: first value row → button row
                _selectedItem = VALUE_ROW_COUNT;
        }

        if (GameCore.Input.Keyboard.WasKeyJustPressed(Keys.Down))
        {
            if (_selectedItem >= VALUE_ROW_COUNT) // on button row → first value row (wrap)
                _selectedItem = 0;
            else if (_selectedItem < VALUE_ROW_COUNT - 1)
                _selectedItem++;
            else // last value row → button row
                _selectedItem = VALUE_ROW_COUNT;
        }

        if (GameCore.Input.Keyboard.WasKeyJustPressed(Keys.Left))
        {
            if (_selectedItem < VALUE_ROW_COUNT)
                AdjustLeft(_selectedItem);
            else if (_selectedItem == VALUE_ROW_COUNT + 1) // on Back, move focus to Save
                _selectedItem = VALUE_ROW_COUNT;
        }

        if (GameCore.Input.Keyboard.WasKeyJustPressed(Keys.Right))
        {
            if (_selectedItem < VALUE_ROW_COUNT)
                AdjustRight(_selectedItem);
            else if (_selectedItem == VALUE_ROW_COUNT) // on Save, move focus to Back
                _selectedItem = VALUE_ROW_COUNT + 1;
        }

        if (GameCore.Input.Keyboard.WasKeyJustPressed(Keys.Enter))
        {
            if (_selectedItem == VALUE_ROW_COUNT) ActivateSave();
            else if (_selectedItem == VALUE_ROW_COUNT + 1) ActivateBack();
        }

        // ── Mouse interaction ──
        if (GameCore.Input.Mouse.WasButtonJustPressed(MouseButton.Left))
        {
            int mx = GameCore.Input.Mouse.X;
            int my = GameCore.Input.Mouse.Y;

            for (int i = 0; i < VALUE_ROW_COUNT; i++)
            {
                if (_leftArrows[i].Contains(mx, my))
                { AdjustLeft(i); _selectedItem = i; }
                if (_rightArrows[i].Contains(mx, my))
                { AdjustRight(i); _selectedItem = i; }
            }

            if (_saveButton.Contains(mx, my))
            { _selectedItem = VALUE_ROW_COUNT; ActivateSave(); }

            if (_backButton.Contains(mx, my))
            { _selectedItem = VALUE_ROW_COUNT + 1; ActivateBack(); }
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

        // Title
        string title = "Settings";
        Vector2 titleSize = GameCore.Assets.TitleFont.MeasureString(title);
        GameCore.SpriteBatch.DrawString(GameCore.Assets.TitleFont, title,
            new Vector2(cx - titleSize.X / 2, vh / 2f - 180), Color.White);

        int startY = vh / 2 - 60;
        int rowHeight = 45;

        // Label X matches the centered layout from CalculateLayout
        int totalRowWidth = 530;
        int rowLeftEdge = (vw - totalRowWidth) / 2;
        int labelX = rowLeftEdge;

        // Labels
        string[] labels = { "Resolution:", "Framerate:", "Display:" };
        string[] values =
        {
            $"{Resolutions[_resolutionIndex].w}x{Resolutions[_resolutionIndex].h}",
            FrameRates[_frameRateIndex] == 0 ? "Unlimited" : $"{FrameRates[_frameRateIndex]} FPS",
            _isFullScreen ? "Full Screen" : "Windowed"
        };

        for (int i = 0; i < VALUE_ROW_COUNT; i++)
        {
            int y = startY + i * rowHeight;
            bool isSelected = (_selectedItem == i);

            // Label – highlight when keyboard-selected
            Color labelColor = isSelected ? Color.Yellow : Color.White;
            GameCore.SpriteBatch.DrawString(GameCore.Assets.TextFont, labels[i], new Vector2(labelX, y + 4), labelColor);

            // Left arrow
            bool leftHover = _leftArrows[i].Contains(mx, my);
            Color leftColor = (isSelected || leftHover) ? Color.Yellow : Color.LightGray;
            GameCore.SpriteBatch.DrawString(GameCore.Assets.TextFont, "<", new Vector2(_leftArrows[i].X + 4, _leftArrows[i].Y + 2), leftColor);

            // Value centered between arrows
            Vector2 valSize = GameCore.Assets.TextFont.MeasureString(values[i]);
            float valX = _leftArrows[i].Right + (_rightArrows[i].Left - _leftArrows[i].Right - valSize.X) / 2;
            GameCore.SpriteBatch.DrawString(GameCore.Assets.TextFont, values[i], new Vector2(valX, y + 4), Color.White);

            // Right arrow
            bool rightHover = _rightArrows[i].Contains(mx, my);
            Color rightColor = (isSelected || rightHover) ? Color.Yellow : Color.LightGray;
            GameCore.SpriteBatch.DrawString(GameCore.Assets.TextFont, ">", new Vector2(_rightArrows[i].X + 4, _rightArrows[i].Y + 2), rightColor);
        }

        // Save button
        DrawButton("Save", _saveButton, mx, my, _selectedItem == VALUE_ROW_COUNT);

        // Back button
        DrawButton("Back", _backButton, mx, my, _selectedItem == VALUE_ROW_COUNT + 1);

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