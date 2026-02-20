using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace AimTrainer.Desktop.Core;

public class AssetManager
{
    public SpriteFont TitleFont { get; init; }
    public SpriteFont TextFont { get; init; }
    
    public Texture2D BlankPixel { get; init; }

    public AssetManager(ContentManager content)
    {
        TitleFont = content.Load<SpriteFont>("fonts/Merriweather_5x");
        TextFont = content.Load<SpriteFont>("fonts/Merriweather");
        
        BlankPixel = new Texture2D(GameCore.GraphicsDevice, 1, 1);
        BlankPixel.SetData([Color.White]);
    }
}