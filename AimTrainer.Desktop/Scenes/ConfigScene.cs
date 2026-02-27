using System;
using AimTrainer.Desktop.Core;
using AimTrainer.Desktop.Core.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace AimTrainer.Desktop.Scenes;

/// <summary>
/// Configuration screen shown before a game round starts.
/// Allows the user to adjust player name, min/max target size, game duration, and target count.
/// Fully navigable with arrow keys + Enter, as well as mouse.
/// </summary>
public class ConfigScene : Scene
{
    // Editable values (initialized from persisted config)
    private string _playerName;
    private int _minTargetSize;
    private int _maxTargetSize;
    private int _gameDuration;
    private int _targetCount;

    // Player name constraints
    private const int MAX_NAME_LENGTH = 12;

    // Step sizes for adjustments
    private const int SIZE_STEP = 4;
    private const int DURATION_STEP = 5;
    private const int COUNT_STEP = 1;

    // Constraints
    private const int MIN_SIZE_LOWER = 8;
    private const int MIN_SIZE_UPPER = 256;
    private const int MAX_SIZE_LOWER = 16;
    private const int MAX_SIZE_UPPER = 512;
    private const int DURATION_LOWER = 5;
    private const int DURATION_UPPER = 300;
    private const int COUNT_LOWER = 1;
    private const int COUNT_UPPER = 50;

    // Keyboard-navigable items:
    // Row 0 = Player Name (text input), Rows 1-4 = numeric value rows,
    // VALUE_ROW_COUNT (5) = Start button, VALUE_ROW_COUNT+1 (6) = Back button.
    // Up/Down navigates value rows and enters/exits the button row.
    // Left/Right switches between Start and Back when on the button row.
    // Rows 1-4: Left/Right adjusts the value.  Row 0: text input via keyboard.
    private const int VALUE_ROW_COUNT = 5;
    private const int NUMERIC_ROW_START = 1; // first numeric row index
    private int _selectedItem;

    // Arrow hit areas for the 4 numeric rows (indices 1-4)
    private Rectangle[] _leftArrows;
    private Rectangle[] _rightArrows;
    private Rectangle _startButton;
    private Rectangle _backButton;

    // Text input event subscription
    private bool _textInputSubscribed;

    public override void Initialize()
    {
        base.Initialize();
        GameCore.ExitOnEscape = false;

        var config = GameCore.Config;
        _playerName = config.PlayerName ?? "Player";
        _minTargetSize = config.MinTargetSize;
        _maxTargetSize = config.MaxTargetSize;
        _gameDuration = config.GameDurationSeconds;
        _targetCount = config.TargetCount;
        _selectedItem = 0;

        // Subscribe to window text input for player name editing
        GameCore.Instance.Window.TextInput += OnTextInput;
        _textInputSubscribed = true;

        CalculateLayout();
    }

    public override void UnloadContent()
    {
        if (_textInputSubscribed)
        {
            GameCore.Instance.Window.TextInput -= OnTextInput;
            _textInputSubscribed = false;
        }
        base.UnloadContent();
    }

    private void OnTextInput(object sender, TextInputEventArgs e)
    {
        // Only process text input when the player name row (row 0) is selected
        if (_selectedItem != 0)
            return;

        char c = e.Character;

        if (c == '\b') // Backspace
        {
            if (_playerName.Length > 0)
                _playerName = _playerName.Substring(0, _playerName.Length - 1);
        }
        else if (!char.IsControl(c) && _playerName.Length < MAX_NAME_LENGTH)
        {
            // Only allow characters the sprite font can render (printable ASCII)
            if (c >= ' ' && c <= '~')
                _playerName += c;
        }
    }

    private void CalculateLayout()
    {
        int vw = GameCore.GraphicsDevice.Viewport.Width;
        int vh = GameCore.GraphicsDevice.Viewport.Height;

        int arrowSize = 30;
        int startY = vh / 2 - 100;
        int rowHeight = 45;
        int valueX = vw / 2 + 100;

        // Only 4 numeric rows (indices 1-4 in the row list) get arrows
        int numericCount = VALUE_ROW_COUNT - 1;
        _leftArrows = new Rectangle[numericCount];
        _rightArrows = new Rectangle[numericCount];

        for (int i = 0; i < numericCount; i++)
        {
            // Offset by 1 row for the player name row at the top
            int y = startY + (i + 1) * rowHeight;
            _leftArrows[i] = new Rectangle(valueX, y, arrowSize, arrowSize);
            _rightArrows[i] = new Rectangle(valueX + 200, y, arrowSize, arrowSize);
        }

        int btnW = 200;
        int btnH = 45;
        int btnY = startY + VALUE_ROW_COUNT * rowHeight + 25;
        _startButton = new Rectangle(vw / 2 - btnW - 10, btnY, btnW, btnH);
        _backButton = new Rectangle(vw / 2 + 10, btnY, btnW, btnH);
    }

    // ── Shared logic for adjusting a numeric value row ──
    // arrowIndex maps to the arrow arrays (0-3), corresponding to selectedItem rows 1-4.
    private void AdjustLeft(int arrowIndex)
    {
        switch (arrowIndex)
        {
            case 0: _minTargetSize = Math.Max(MIN_SIZE_LOWER, _minTargetSize - SIZE_STEP); break;
            case 1: _maxTargetSize = Math.Max(MAX_SIZE_LOWER, _maxTargetSize - SIZE_STEP); break;
            case 2: _gameDuration = Math.Max(DURATION_LOWER, _gameDuration - DURATION_STEP); break;
            case 3: _targetCount = Math.Max(COUNT_LOWER, _targetCount - COUNT_STEP); break;
        }
        EnforceMinMax();
    }

    private void AdjustRight(int arrowIndex)
    {
        switch (arrowIndex)
        {
            case 0: _minTargetSize = Math.Min(MIN_SIZE_UPPER, _minTargetSize + SIZE_STEP); break;
            case 1: _maxTargetSize = Math.Min(MAX_SIZE_UPPER, _maxTargetSize + SIZE_STEP); break;
            case 2: _gameDuration = Math.Min(DURATION_UPPER, _gameDuration + DURATION_STEP); break;
            case 3: _targetCount = Math.Min(COUNT_UPPER, _targetCount + COUNT_STEP); break;
        }
        EnforceMinMax();
    }

    private void EnforceMinMax()
    {
        if (_minTargetSize > _maxTargetSize)
            _maxTargetSize = _minTargetSize;
    }

    private void ActivateStart()
    {
        var config = GameCore.Config;
        config.PlayerName = _playerName.Length > 0 ? _playerName : "Player";
        config.MinTargetSize = _minTargetSize;
        config.MaxTargetSize = _maxTargetSize;
        config.GameDurationSeconds = _gameDuration;
        config.TargetCount = _targetCount;
        config.Save();

        GameCore.ChangeScene(new GameScene());
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
            if (_selectedItem >= NUMERIC_ROW_START && _selectedItem < VALUE_ROW_COUNT)
                AdjustLeft(_selectedItem - NUMERIC_ROW_START);
            else if (_selectedItem == VALUE_ROW_COUNT + 1) // on Back, move focus to Start
                _selectedItem = VALUE_ROW_COUNT;
            // Row 0 (player name): Left key ignored – text input handled via TextInput event
        }

        if (GameCore.Input.Keyboard.WasKeyJustPressed(Keys.Right))
        {
            if (_selectedItem >= NUMERIC_ROW_START && _selectedItem < VALUE_ROW_COUNT)
                AdjustRight(_selectedItem - NUMERIC_ROW_START);
            else if (_selectedItem == VALUE_ROW_COUNT) // on Start, move focus to Back
                _selectedItem = VALUE_ROW_COUNT + 1;
            // Row 0 (player name): Right key ignored – text input handled via TextInput event
        }

        if (GameCore.Input.Keyboard.WasKeyJustPressed(Keys.Enter))
        {
            if (_selectedItem == VALUE_ROW_COUNT) ActivateStart();
            else if (_selectedItem == VALUE_ROW_COUNT + 1) ActivateBack();
        }

        // ── Mouse interaction ──
        if (GameCore.Input.Mouse.WasButtonJustPressed(MouseButton.Left))
        {
            int mx = GameCore.Input.Mouse.X;
            int my = GameCore.Input.Mouse.Y;

            int numericCount = VALUE_ROW_COUNT - 1;
            for (int i = 0; i < numericCount; i++)
            {
                if (_leftArrows[i].Contains(mx, my))
                { AdjustLeft(i); _selectedItem = i + NUMERIC_ROW_START; }
                if (_rightArrows[i].Contains(mx, my))
                { AdjustRight(i); _selectedItem = i + NUMERIC_ROW_START; }
            }

            if (_startButton.Contains(mx, my))
            { _selectedItem = VALUE_ROW_COUNT; ActivateStart(); }

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
        string title = "Game Config";
        Vector2 titleSize = GameCore.Assets.TitleFont.MeasureString(title);
        GameCore.SpriteBatch.DrawString(GameCore.Assets.TitleFont, title,
            new Vector2(cx - titleSize.X / 2, vh / 2f - 220), Color.White);

        int startY = vh / 2 - 100;
        int rowHeight = 45;
        int labelX = vw / 2 - 240;
        int valueX = vw / 2 + 100;

        // ── Row 0: Player Name (text input) ──
        {
            int y = startY;
            bool isSelected = (_selectedItem == 0);
            Color labelColor = isSelected ? Color.Yellow : Color.White;
            GameCore.SpriteBatch.DrawString(GameCore.Assets.TextFont, "Player Name:", new Vector2(labelX, y + 4), labelColor);

            // Show name with a blinking cursor when selected
            string displayName = _playerName;
            if (isSelected)
                displayName += "_";

            GameCore.SpriteBatch.DrawString(GameCore.Assets.TextFont, displayName, new Vector2(valueX, y + 4),
                isSelected ? Color.Yellow : Color.White);
        }

        // ── Rows 1-4: Numeric value rows ──
        string[] labels = { "Min Target Size:", "Max Target Size:", "Duration (sec):", "Target Count:" };
        string[] values =
        {
            _minTargetSize.ToString(),
            _maxTargetSize.ToString(),
            _gameDuration.ToString(),
            _targetCount.ToString()
        };

        for (int i = 0; i < labels.Length; i++)
        {
            int rowIndex = i + NUMERIC_ROW_START;
            int y = startY + rowIndex * rowHeight;
            bool isSelected = (_selectedItem == rowIndex);

            // Label
            Color labelColor = isSelected ? Color.Yellow : Color.White;
            GameCore.SpriteBatch.DrawString(GameCore.Assets.TextFont, labels[i], new Vector2(labelX, y + 4), labelColor);

            // Left arrow
            bool leftHover = _leftArrows[i].Contains(mx, my);
            Color leftColor = (isSelected || leftHover) ? Color.Yellow : Color.LightGray;
            GameCore.SpriteBatch.DrawString(GameCore.Assets.TextFont, "<", new Vector2(_leftArrows[i].X + 4, _leftArrows[i].Y + 2), leftColor);

            // Value centered between arrows
            Vector2 valSize = GameCore.Assets.TextFont.MeasureString(values[i]);
            float valX2 = _leftArrows[i].Right + (_rightArrows[i].Left - _leftArrows[i].Right - valSize.X) / 2;
            GameCore.SpriteBatch.DrawString(GameCore.Assets.TextFont, values[i], new Vector2(valX2, y + 4), Color.White);

            // Right arrow
            bool rightHover = _rightArrows[i].Contains(mx, my);
            Color rightColor = (isSelected || rightHover) ? Color.Yellow : Color.LightGray;
            GameCore.SpriteBatch.DrawString(GameCore.Assets.TextFont, ">", new Vector2(_rightArrows[i].X + 4, _rightArrows[i].Y + 2), rightColor);
        }

        // Start button
        DrawButton("Start", _startButton, mx, my, _selectedItem == VALUE_ROW_COUNT);

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