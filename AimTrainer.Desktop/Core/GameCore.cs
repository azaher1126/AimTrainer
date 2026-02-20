using System;
using AimTrainer.Desktop.Core.Audio;
using AimTrainer.Desktop.Core.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace AimTrainer.Desktop.Core;

public class GameCore: Microsoft.Xna.Framework.Game
{
    private static GameCore _instance;

    /// <summary>
    /// Gets a reference to the Core instance.
    /// </summary>
    public static GameCore Instance => _instance;
    
    // The scene that is currently active.
    private static Scene _activeScene;

    // The next scene to switch to, if there is one.
    private static Scene _nextScene;
    
    /// <summary>
    /// Gets the graphics device manager to control the presentation of graphics.
    /// </summary>
    public static GraphicsDeviceManager Graphics { get; private set; }

    /// <summary>
    /// Gets the graphics device used to create graphical resources and perform primitive rendering.
    /// </summary>
    public new static GraphicsDevice GraphicsDevice { get; private set; }

    /// <summary>
    /// Gets the sprite batch used for all 2D rendering.
    /// </summary>
    public static SpriteBatch SpriteBatch { get; private set; }

    /// <summary>
    /// Gets the content manager used to load global assets.
    /// </summary>
    public static new ContentManager Content { get; private set; }
    
    /// <summary>
    /// Gets a reference to the input management system.
    /// </summary>
    public static InputManager Input { get; private set; }

    /// <summary>
    /// Gets or Sets a value that indicates if the game should exit when the esc key on the keyboard is pressed.
    /// </summary>
    public static bool ExitOnEscape { get; set; }
    
    /// <summary>
    /// Gets a reference to the audio control system.
    /// </summary>
    public static AudioController Audio { get; private set; }
    
    public static AssetManager Assets { get; private set; }
    
    
    /// <summary>
    /// Creates a new Core instance.
    /// </summary>
    /// <param name="title">The title to display in the title bar of the game window.</param>
    /// <param name="width">The initial width, in pixels, of the game window.</param>
    /// <param name="height">The initial height, in pixels, of the game window.</param>
    /// <param name="fullScreen">Indicates if the game should start in fullscreen mode.</param>
    public GameCore(string title, int width, int height, bool fullScreen)
    {
        // Ensure that multiple cores are not created.
        if (_instance != null)
        {
            throw new InvalidOperationException($"Only a single Core instance can be created");
        }

        // Store reference to engine for global member access.
        _instance = this;

        // Create a new graphics device manager.
        Graphics = new GraphicsDeviceManager(this);

        // Set the graphics defaults.
        Graphics.PreferredBackBufferWidth = width;
        Graphics.PreferredBackBufferHeight = height;
        Graphics.IsFullScreen = fullScreen;

        // Apply the graphic presentation changes.
        Graphics.ApplyChanges();

        // Set the window title.
        Window.Title = title;

        // Set the core's content manager to a reference of the base Game's
        // content manager.
        Content = base.Content;

        // Set the root directory for content.
        Content.RootDirectory = "Content";

        // Mouse is visible by default.
        IsMouseVisible = true;
        
        ExitOnEscape = true;
    }
    
    protected override void Initialize()
    {
        base.Initialize();

        // Set the core's graphics device to a reference of the base Game's
        // graphics device.
        GraphicsDevice = base.GraphicsDevice;

        // Create the sprite batch instance.
        SpriteBatch = new SpriteBatch(GraphicsDevice);
        
        // Create a new input manager.
        Input = new InputManager();
        
        // Create a new audio controller.
        Audio = new AudioController();
        
        Assets = new AssetManager(Content);
    }
    
    protected override void UnloadContent()
    {
        // Dispose of the audio controller.
        Audio.Dispose();

        base.UnloadContent();
    }
    
    protected override void Update(GameTime gameTime)
    {
        // Update the input manager.
        Input.Update(gameTime);
        
        // Update the audio controller.
        Audio.Update();

        if (ExitOnEscape && Input.Keyboard.IsKeyDown(Keys.Escape))
        {
            Exit();
        }
        
        // if there is a next scene waiting to be switch to, then transition
        // to that scene.
        if (_nextScene != null)
        {
            TransitionScene();
        }

        // If there is an active scene, update it.
        if (_activeScene != null)
        {
            _activeScene.Update(gameTime);
        }

        base.Update(gameTime);
    }
    
    protected override void Draw(GameTime gameTime)
    {
        // If there is an active scene, draw it.
        if (_activeScene != null)
        {
            _activeScene.Draw(gameTime);
        }

        base.Draw(gameTime);
    }

    public static void ChangeScene(Scene next)
    {
        // Only set the next scene value if it is not the same
        // instance as the currently active scene.
        if (_activeScene != next)
        {
            _nextScene = next;
        }
    }

    private static void TransitionScene()
    {
        // If there is an active scene, dispose of it.
        if (_activeScene != null)
        { 
            _activeScene.Dispose();
        }

        // Force the garbage collector to collect to ensure memory is cleared.
        GC.Collect();

        // Change the currently active scene to the new scene.
        _activeScene = _nextScene;

        // Null out the next scene value so it does not trigger a change over and over.
        _nextScene = null;

        // If the active scene now is not null, initialize it.
        // Remember, just like with Game, the Initialize call also calls the
        // Scene.LoadContent
        if (_activeScene != null)
        {
            _activeScene.Initialize();
        }
    }
}