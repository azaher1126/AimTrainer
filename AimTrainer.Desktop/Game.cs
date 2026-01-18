using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using AimTrainer.Desktop.Core;
using AimTrainer.Desktop.Scenes;

namespace AimTrainer.Desktop;

public class Game : GameCore
{
    public Game(): base("AimTrainer", 1280, 720, false)
    {
    }

    protected override void Initialize()
    {
        // TODO: Add your initialization logic here

        base.Initialize();
        
        ChangeScene(new TitleScene());
    }

    protected override void LoadContent()
    {
        // TODO: use this.Content to load your game content here
    }
}