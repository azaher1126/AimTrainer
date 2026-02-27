using AimTrainer.Desktop.Core;
using AimTrainer.Desktop.Core.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace AimTrainer.Desktop.Scenes;

/// <summary>
/// Displays the top 5 leaderboard entries with player name, score, and hit rate.
/// A single "Back" button returns to the scene that opened it.
/// </summary>
public class LeaderboardScene : Scene
{
    private Rectangle _backButton;

    // The scene to return to when Back is pressed.
    // null means return to TitleScene (default).
    private readonly Scene _returnScene;

    public LeaderboardScene() : this(null) { }

    public LeaderboardScene(Scene returnScene)
    {
        _returnScene = returnScene;
    }

    public override void Initialize()
    {
        base.Initialize();
        GameCore.ExitOnEscape = false;

        int vw = GameCore.GraphicsDevice.Viewport.Width;
        int vh = GameCore.GraphicsDevice.Viewport.Height;

        int btnW = 200;
        int btnH = 50;
        _backButton = new Rectangle(vw / 2 - btnW / 2, vh - 120, btnW, btnH);
    }

    private void GoBack()
    {
        GameCore.ChangeScene(_returnScene ?? new TitleScene());
    }

    public override void Update(GameTime gameTime)
    {
        if (GameCore.Input.Keyboard.WasKeyJustPressed(Keys.Escape) ||
            GameCore.Input.Keyboard.WasKeyJustPressed(Keys.Enter))
        {
            GoBack();
            return;
        }

        if (GameCore.Input.Mouse.WasButtonJustPressed(MouseButton.Left))
        {
            int mx = GameCore.Input.Mouse.X;
            int my = GameCore.Input.Mouse.Y;

            if (_backButton.Contains(mx, my))
                GoBack();
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
        string title = "Leaderboard";
        Vector2 titleSize = GameCore.Assets.TitleFont.MeasureString(title);
        GameCore.SpriteBatch.DrawString(GameCore.Assets.TitleFont, title,
            new Vector2(cx - titleSize.X / 2, 40), Color.White);

        var entries = GameCore.Leaderboard.Entries;

        if (entries.Count == 0)
        {
            // No entries yet
            string noEntries = "No scores recorded yet!";
            Vector2 noSize = GameCore.Assets.TextFont.MeasureString(noEntries);
            GameCore.SpriteBatch.DrawString(GameCore.Assets.TextFont, noEntries,
                new Vector2(cx - noSize.X / 2, vh / 2f - noSize.Y / 2), Color.LightGray);
        }
        else
        {
            // Table layout
            int tableTop = 180;
            int rowHeight = 40;

            // Column positions (left-aligned from computed start)
            int rankX = (int)(cx - 420);
            int nameX = rankX + 50;
            int scoreX = rankX + 280;
            int durationX = rankX + 420;
            int hpmX = rankX + 550;
            int rateX = rankX + 720;
            int tableWidth = 860;

            // Header
            Color headerColor = Color.Gold;
            GameCore.SpriteBatch.DrawString(GameCore.Assets.TextFont, "#", new Vector2(rankX, tableTop), headerColor);
            GameCore.SpriteBatch.DrawString(GameCore.Assets.TextFont, "Name", new Vector2(nameX, tableTop), headerColor);
            GameCore.SpriteBatch.DrawString(GameCore.Assets.TextFont, "Score", new Vector2(scoreX, tableTop), headerColor);
            GameCore.SpriteBatch.DrawString(GameCore.Assets.TextFont, "Time", new Vector2(durationX, tableTop), headerColor);
            GameCore.SpriteBatch.DrawString(GameCore.Assets.TextFont, "Hits/Min", new Vector2(hpmX, tableTop), headerColor);
            GameCore.SpriteBatch.DrawString(GameCore.Assets.TextFont, "Hit Rate", new Vector2(rateX, tableTop), headerColor);

            // Separator line
            int lineY = tableTop + 30;
            GameCore.SpriteBatch.Draw(GameCore.Assets.BlankPixel,
                new Rectangle(rankX, lineY, tableWidth, 2), Color.White * 0.3f);

            // Entries
            for (int i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                int y = tableTop + 40 + i * rowHeight;

                // Highlight first place
                Color rowColor = (i == 0) ? Color.Gold : Color.White;

                string rank = $"{i + 1}.";
                string name = entry.PlayerName.Length > 12
                    ? entry.PlayerName.Substring(0, 12)
                    : entry.PlayerName;
                string score = entry.Score.ToString();
                string duration = $"{entry.DurationSeconds}s";
                string hpm = $"{entry.HitsPerMinute:F1}";
                string rate = $"{entry.HitRate:P1}";

                GameCore.SpriteBatch.DrawString(GameCore.Assets.TextFont, rank, new Vector2(rankX, y), rowColor);
                GameCore.SpriteBatch.DrawString(GameCore.Assets.TextFont, name, new Vector2(nameX, y), rowColor);
                GameCore.SpriteBatch.DrawString(GameCore.Assets.TextFont, score, new Vector2(scoreX, y), rowColor);
                GameCore.SpriteBatch.DrawString(GameCore.Assets.TextFont, duration, new Vector2(durationX, y), rowColor);
                GameCore.SpriteBatch.DrawString(GameCore.Assets.TextFont, hpm, new Vector2(hpmX, y), rowColor);
                GameCore.SpriteBatch.DrawString(GameCore.Assets.TextFont, rate, new Vector2(rateX, y), rowColor);
            }
        }

        // Back button (always highlighted since it's the only action)
        DrawButton("Back", _backButton, mx, my, true);

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