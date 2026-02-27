using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace AimTrainer.Desktop.Core;

/// <summary>
/// Rendering/display settings that persist across sessions.
/// </summary>
public class GameSettings
{
    public int ResolutionWidth { get; set; } = 1280;
    public int ResolutionHeight { get; set; } = 720;
    public int FrameRate { get; set; } = 60; // 0 = unlimited
    public bool IsFullScreen { get; set; } = false;

    private static readonly string SettingsDir =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AimTrainer");
    private static readonly string SettingsFile = Path.Combine(SettingsDir, "settings.json");

    public void Save()
    {
        Directory.CreateDirectory(SettingsDir);
        var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(SettingsFile, json);
    }

    public static GameSettings Load()
    {
        try
        {
            if (File.Exists(SettingsFile))
            {
                var json = File.ReadAllText(SettingsFile);
                return JsonSerializer.Deserialize<GameSettings>(json) ?? new GameSettings();
            }
        }
        catch
        {
            // If deserialization fails, return defaults
        }

        return new GameSettings();
    }

    /// <summary>
    /// Apply the current settings to the game graphics.
    /// </summary>
    public void Apply()
    {
        GameCore.Graphics.PreferredBackBufferWidth = ResolutionWidth;
        GameCore.Graphics.PreferredBackBufferHeight = ResolutionHeight;
        GameCore.Graphics.IsFullScreen = IsFullScreen;

        if (FrameRate <= 0)
        {
            // Unlimited
            GameCore.Instance.IsFixedTimeStep = false;
            GameCore.Graphics.SynchronizeWithVerticalRetrace = false;
        }
        else
        {
            GameCore.Instance.IsFixedTimeStep = true;
            GameCore.Instance.TargetElapsedTime = TimeSpan.FromSeconds(1.0 / FrameRate);
            GameCore.Graphics.SynchronizeWithVerticalRetrace = false;
        }

        GameCore.Graphics.ApplyChanges();
    }
}

/// <summary>
/// Game configuration settings (target sizes, duration, count) that persist across sessions.
/// </summary>
public class GameConfig
{
    public int MinTargetSize { get; set; } = 24;
    public int MaxTargetSize { get; set; } = 96;
    public int GameDurationSeconds { get; set; } = 60;
    public int TargetCount { get; set; } = 5;
    public string PlayerName { get; set; } = "Player";

    private static readonly string SettingsDir =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AimTrainer");
    private static readonly string ConfigFile = Path.Combine(SettingsDir, "config.json");

    public void Save()
    {
        Directory.CreateDirectory(SettingsDir);
        var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(ConfigFile, json);
    }

    public static GameConfig Load()
    {
        try
        {
            if (File.Exists(ConfigFile))
            {
                var json = File.ReadAllText(ConfigFile);
                return JsonSerializer.Deserialize<GameConfig>(json) ?? new GameConfig();
            }
        }
        catch
        {
            // If deserialization fails, return defaults
        }

        return new GameConfig();
    }
}

/// <summary>
/// A single leaderboard entry recording a round result.
/// </summary>
public class LeaderboardEntry
{
    public string PlayerName { get; set; } = "";
    public int Score { get; set; }
    public int TotalClicks { get; set; }
    public double HitRate { get; set; }
    public int DurationSeconds { get; set; } = 60;

    /// <summary>
    /// Computed at runtime from Score and DurationSeconds. Not persisted to JSON.
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public double HitsPerMinute => DurationSeconds > 0 ? Score / (DurationSeconds / 60.0) : 0;
}

/// <summary>
/// Persisted leaderboard containing the top 5 scores.
/// </summary>
public class Leaderboard
{
    public List<LeaderboardEntry> Entries { get; set; } = new();

    private const int MaxEntries = 5;

    private static readonly string SettingsDir =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AimTrainer");
    private static readonly string LeaderboardFile = Path.Combine(SettingsDir, "leaderboard.json");

    /// <summary>
    /// Adds a score entry. The list is re-sorted by hits per minute (descending),
    /// then hit rate as a tiebreaker, and trimmed to top 5.
    /// Returns true if the entry made it onto the board.
    /// </summary>
    public bool TryAdd(string playerName, int score, int totalClicks, double hitRate, int durationSeconds)
    {
        var entry = new LeaderboardEntry
        {
            PlayerName = playerName,
            Score = score,
            TotalClicks = totalClicks,
            HitRate = hitRate,
            DurationSeconds = durationSeconds
        };

        Entries.Add(entry);
        SortEntries();

        bool madeIt = Entries.IndexOf(entry) < MaxEntries;

        if (Entries.Count > MaxEntries)
            Entries.RemoveRange(MaxEntries, Entries.Count - MaxEntries);

        return madeIt;
    }

    public void Save()
    {
        Directory.CreateDirectory(SettingsDir);
        var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(LeaderboardFile, json);
    }

    public static Leaderboard Load()
    {
        try
        {
            if (File.Exists(LeaderboardFile))
            {
                var json = File.ReadAllText(LeaderboardFile);
                var leaderboard = JsonSerializer.Deserialize<Leaderboard>(json) ?? new Leaderboard();
                leaderboard.SortEntries();
                return leaderboard;
            }
        }
        catch
        {
            // If deserialization fails, return defaults
        }

        return new Leaderboard();
    }

    /// <summary>
    /// Sorts entries by hits per minute (descending), then hit rate as tiebreaker.
    /// </summary>
    private void SortEntries()
    {
        Entries.Sort((a, b) =>
        {
            int cmp = b.HitsPerMinute.CompareTo(a.HitsPerMinute);
            return cmp != 0 ? cmp : b.HitRate.CompareTo(a.HitRate);
        });
    }
}