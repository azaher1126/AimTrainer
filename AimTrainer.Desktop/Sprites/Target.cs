using AimTrainer.Desktop.Core;
using AimTrainer.Desktop.Core.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AimTrainer.Desktop.Sprites;

public class Target : Sprite
{
    public Vector2 Position { get; private set; }

    public Target(TextureRegion region, Color color, Vector2 position, int size) : base(region)
    {
        Color = color;
        Position = position;
        Scale = new Vector2(size);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        Draw(spriteBatch, Position);
    }
}