using AimTrainer.Desktop.Core;
using AimTrainer.Desktop.Scenes;
using MonoGameGum;
using Gum.Forms;
using Gum.Forms.Controls;

namespace AimTrainer.Desktop;

public class Game : GameCore
{
    public Game(): base("AimTrainer", 1280, 720, false)
    {
    }

    protected override void Initialize()
    {
        base.Initialize();

        // Load persisted settings, configuration, and leaderboard.
        Settings = GameSettings.Load();
        Config = GameConfig.Load();
        Leaderboard = Leaderboard.Load();

        // Apply the loaded display settings.
        Settings.Apply();

        InitializeGum();

        ChangeScene(new TitleScene());
    }

    private void InitializeGum()
    {
        GumService.Default.Initialize(this, DefaultVisualsVersion.V3);

        GumService.Default.ContentLoader!.XnaContentManager = Content;

        FrameworkElement.KeyboardsForUiControl.Add(GumService.Default.Keyboard);

        FrameworkElement.TabReverseKeyCombos.Add(
            new KeyCombo() { PushedKey = Microsoft.Xna.Framework.Input.Keys.Up });

        FrameworkElement.TabKeyCombos.Add(
            new KeyCombo() { PushedKey = Microsoft.Xna.Framework.Input.Keys.Down });
    }
}
