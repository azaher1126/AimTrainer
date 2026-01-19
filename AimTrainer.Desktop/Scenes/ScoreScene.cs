using AimTrainer.Desktop.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AimTrainer.Desktop.Scenes;

public class ScoreScene: Scene
{
    public override void Initialize()
    {
        base.Initialize();
        
        GameCore.ExitOnEscape = true;
    }

    public override void Draw(GameTime gameTime)
    {
        GameCore.GraphicsDevice.Clear(new Color(32, 40, 78, 255));
    }
}