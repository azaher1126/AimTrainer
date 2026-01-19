using System;
using System.Collections.Generic;
using AimTrainer.Desktop.Core;
using AimTrainer.Desktop.Core.Input;
using AimTrainer.Desktop.Sprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AimTrainer.Desktop.Scenes;

public class GameScene: Scene
{
    private TextureRegion _targetTexture;

    private List<Target> _targets;
    
    private TimeSpan? _startTime;
    
    private const int TARGET_COUNT = 5;
    
    public override void Initialize()
    {
        base.Initialize();

        GameCore.ExitOnEscape = false;
        _targets = new List<Target>(TARGET_COUNT);
    }

    public override void LoadContent()
    {
        var texture = new Texture2D(GameCore.GraphicsDevice, 1, 1);
        texture.SetData([Color.White]);
        
        _targetTexture = new TextureRegion(texture, 0, 0, 1, 1);
    }
    
    public override void Update(GameTime gameTime)
    {
        if (_startTime != null && gameTime.TotalGameTime - _startTime > TimeSpan.FromSeconds(60))
        {
            GameCore.ChangeScene(new ScoreScene());
        }
        
        if (GameCore.Input.Mouse.WasButtonJustPressed(MouseButton.Left))
        {
            _startTime ??= gameTime.TotalGameTime;

            for (int i = TARGET_COUNT - 1; i >= 0; i--)
            {
                if (!IsTargetClicked(_targets[i])) continue;
                _targets.RemoveAt(i);
                break;
            }
        }

        if (_targets.Count >= TARGET_COUNT) return;
        
        for (int i = _targets.Count; i < TARGET_COUNT; i++)
        {
            _targets.Add(CreateTarget());
        }
    }

    public override void Draw(GameTime gameTime)
    {
        GameCore.GraphicsDevice.Clear(new Color(32, 40, 78, 255));
        
        GameCore.SpriteBatch.Begin();
        
        foreach (var target in _targets)
        {
            target.Draw(GameCore.SpriteBatch);
        }
        
        GameCore.SpriteBatch.End();
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
        var color = GetRandomColor();
        var size = GetRandomSize();
        var position = GetRandomPosition(size);
        return new Target(_targetTexture, color, position, size);
    }
    
    private Color GetRandomColor()
    {
        var red = Random.Shared.Next(256);
        var green = Random.Shared.Next(256);
        var blue = Random.Shared.Next(256);

        return new Color(red, green, blue);
    }
    
    private int GetRandomSize()
    {
        return Random.Shared.Next(24, 97);
    }

    private Vector2 GetRandomPosition(int size)
    {
        var maxX = GameCore.GraphicsDevice.Viewport.Width - size;
        var maxY = GameCore.GraphicsDevice.Viewport.Height - size;
        
        return new Vector2(Random.Shared.Next(maxX), Random.Shared.Next(maxY));
    }
}