﻿using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Embyr.Scenes;
using Embyr.Tools;
using Embyr.UI;
using Embyr.Data;
using Embyr.Rendering;

namespace Embyr;

//* lol this used to be Game1

/// <summary>
/// Main game class, base building blocks that this Emybr game runs within
/// </summary>
public abstract class Game : Microsoft.Xna.Framework.Game {
    public enum RenderPipeline {
        Deferred2D,
        Forward3D
    }

    /// <summary>
    /// Simple readonly struct that contains all parameters to set up a new Embyr game, should be created in <c>Game.Setup</c>
    /// </summary>
    public readonly struct GameSetupParams {
        public required Type InitialSceneType { get; init; }
        public Point CanvasRes { get; init; }
        public Point WindowRes { get; init; }
        public required RenderPipeline RenderPipeline { get; init; }
        public string WindowTitle { get; init; }
        public bool EnableVSync { get; init; }
        public bool IsFullscreen { get; init; }
        public bool IsBorderless { get; init; }
        public bool AllowWindowResizing { get; init; }
        public ActionBindingPreset? DefaultBindingPreset { get; init; }

        /// <summary>
        /// Creates a new SetupParams instance with default values
        /// </summary>
        public GameSetupParams() {
            CanvasRes = new Point(480, 270);
            WindowRes = new Point(1280, 720);
            WindowTitle = "Embyr Project";
            EnableVSync = true;
            IsFullscreen = false;
            IsBorderless = false;
            AllowWindowResizing = false;
            DefaultBindingPreset = null;
        }
    }

    /// <summary>
    /// Readonly struct that describes initial setup parameters for the renderer
    /// </summary>
    public readonly struct RendererSetupParams {
        public RendererSettings RendererSettings { get; init; }
        public PostProcessingEffect[] PostProcessingEffects { get; init; }

        public RendererSetupParams() {
            RendererSettings = new RendererSettings();
            PostProcessingEffects = Array.Empty<PostProcessingEffect>();
        }
    }

    #region // Fields

    internal static readonly int CanvasExpandSize = 2;

    private readonly GraphicsDeviceManager graphics;
    private GameSetupParams setupParams;

    private bool isActivePrev;

    private Rectangle canvasDestination;
    private float canvasScaling;
    private Matrix mouseMatrix;
    private bool isResizing;

    /// <summary>
    /// Gets the current renderer used for the game
    /// </summary>
    internal Renderer Renderer { get; private set; }

    /// <summary>
    /// Action called when focus is lost on window
    /// </summary>
    public Action OnLoseFocus;

    /// <summary>
    /// Action called when low res canvas resolution changes, parameters are width and height
    /// </summary>
    public Action<int, int> OnResolutionChange;

    #endregion

    #region // Init methods

    /// <summary>
    /// Creates a new Embyr game
    /// </summary>
    /// <param name="saveDataFolderName">Optional system save data folder name, if omitted then saving & paths will not work!</param>
    public Game(string? saveDataFolderName = null) {
        graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        if (!string.IsNullOrWhiteSpace(saveDataFolderName)) {
            Paths.Init(saveDataFolderName);
        } else {
            Debug.WriteLine("Config folder name not detected. This isn't an issue, just letting you know :3");
        }
    }

    /// <summary>
    /// Initializes game along with basic window and engine setup parameters
    /// </summary>
    protected override sealed void Initialize() {
        Debug.WriteLine("Setting up Embyr game...");

        setupParams = SetupGame();

        if (setupParams.InitialSceneType == null) {
            throw new NullReferenceException("Cannot set up game with null initial scene type!");
        }

        EngineSettings.GameCanvasResolution = setupParams.CanvasRes;
        EngineSettings.GameWindowResolution = setupParams.WindowRes;
        EngineSettings.CurrentBindingPreset = setupParams.DefaultBindingPreset ?? ActionBindingPreset.MakeDefault();
        EngineSettings.EnableVSync = setupParams.EnableVSync;
        EngineSettings.IsFullscreen = setupParams.IsFullscreen;
        EngineSettings.IsBorderless = setupParams.IsBorderless;
        EngineSettings.WindowTitle = setupParams.WindowTitle;

        Window.AllowUserResizing = setupParams.AllowWindowResizing;
        Window.ClientSizeChanged += OnResize;

        EngineSettings.ShouldApplyGraphicsChanges = true;

        base.Initialize();
    }

    /// <summary>
    /// Loads game content and initializes shaders, content management, and renderer
    /// </summary>
    protected override sealed void LoadContent() {
        ShaderManager.Init(GraphicsDevice, ShaderManager.ShaderProfile.OpenGL);
        Assets.Init(this);

        RendererSetupParams rParams = SetupRenderer();

        Renderer = setupParams.RenderPipeline switch {
            RenderPipeline.Deferred2D => new RendererDeferred2D(rParams.RendererSettings, GraphicsDevice),
            RenderPipeline.Forward3D => new RendererForward3D(rParams.RendererSettings, GraphicsDevice),
            _ => throw new Exception("Inputted render pipeline not recognized!")
        };

        foreach (PostProcessingEffect fx in rParams.PostProcessingEffects) {
            Renderer.AddPostProcessingEffect(fx);
        }

        // set up scene when loading content !!
        SceneManager.Init(this, setupParams.InitialSceneType);
    }

    /// <summary>
    /// Sets up basic game parameters
    /// </summary>
    /// <returns>GameSetupParams that describes the game to make</returns>
    protected abstract GameSetupParams SetupGame();

    /// <summary>
    /// Sets up renderer parameters
    /// </summary>
    /// <returns>RendererSetupParams that describes the renderer to create</returns>
    protected abstract RendererSetupParams SetupRenderer();

    #endregion

    #region // Game loop

    /// <summary>
    /// Updates game logic, is run before every draw call
    /// </summary>
    /// <param name="gameTime">GameTime of this current frame</param>
    protected override void Update(GameTime gameTime) {
        Performance.Update(gameTime);
        Performance.FrametimeMeasureStart();
        Performance.UpdateMeasureStart();

        Gooey.ResetPool();

        Window.Title = EngineSettings.WindowTitle;

        Matrix invertCam2DMat = Matrix.Identity;
        if (SceneManager.CurrentScene is Scene2D scene) {
            invertCam2DMat = scene.Camera.InvertedMatrix;
        }

        Input.CurrentBindingPreset = EngineSettings.CurrentBindingPreset;
        Input.Update(
            mouseMatrix,
            invertCam2DMat,
            Performance.DeltaTime
        );

        if (!SceneManager.IsLoading) {
            for (int i = 0; i < Performance.NumPhysicsUpdateToRun; i++) {
                SceneManager.PhysicsUpdate(Performance.FixedDeltaTime);
            }
            SceneManager.Update(Performance.DeltaTime);
        }

        Gooey.ValidateTree();
        Gooey.CalcGrowSizing();
        Gooey.CalcPositions();
        Gooey.ActivateClickables();

        if (!IsActive && isActivePrev) {
            OnLoseFocus?.Invoke();
        }

        // hide mouse cursor when using a controller
        IsMouseVisible = Input.Mode == InputMode.Keyboard;

        isActivePrev = IsActive;

        SceneManager.ChangeQueuedScene();

        if (EngineSettings.ShouldApplyGraphicsChanges) {
            ChangeResolution(
                EngineSettings.GameCanvasResolution.X + CanvasExpandSize,
                EngineSettings.GameCanvasResolution.Y + CanvasExpandSize
            );

            graphics.HardwareModeSwitch = !EngineSettings.IsBorderless;
            graphics.IsFullScreen = EngineSettings.IsFullscreen;
            graphics.SynchronizeWithVerticalRetrace = EngineSettings.EnableVSync;
            graphics.PreferredBackBufferWidth = EngineSettings.GameWindowResolution.X;
            graphics.PreferredBackBufferHeight = EngineSettings.GameWindowResolution.Y;

            graphics.ApplyChanges();

            ResizeCanvasDestination();

            EngineSettings.ShouldApplyGraphicsChanges = false;
        }

        Performance.UpdateMeasureEnd();

        base.Update(gameTime);
    }

    /// <summary>
    /// Draws the game to the window
    /// </summary>
    /// <param name="gameTime">GameTime of this current frame</param>
    protected override sealed void Draw(GameTime gameTime) {
        Performance.DrawMeasureStart();

        if (!SceneManager.IsLoading) {
            Renderer.RenderScene(SceneManager.CurrentScene);
        }

        Renderer.Render(canvasDestination, canvasScaling);
        DrawCropBars();

        base.Draw(gameTime);

        Performance.DrawMeasureEnd();
        Performance.FrametimeMeasureEnd();
    }

    #endregion

    #region // Helper methods

    /// <summary>
    /// Method that is run every time the window is resized
    /// </summary>
    private void OnResize(object? sender, EventArgs? e) {
        if (!isResizing && Window.ClientBounds.Width > 0 && Window.ClientBounds.Height > 0) {
            isResizing = true;

            graphics.PreferredBackBufferWidth = Window.ClientBounds.Width;
            graphics.PreferredBackBufferHeight = Window.ClientBounds.Height;
            graphics.ApplyChanges();

            ResizeCanvasDestination();

            isResizing = false;
        }
    }

    private void ResizeCanvasDestination() {
        Point size = GraphicsDevice.Viewport.Bounds.Size;
        Point gameRes = EngineSettings.GameCanvasResolution;

        float scaleX = (float)size.X / gameRes.X;
        float scaleY = (float)size.Y / gameRes.Y;
        canvasScaling = MathF.Min(scaleX, scaleY);

        // calculate detination rect, centering in screen
        canvasDestination.Width = (int)(gameRes.X * canvasScaling);
        canvasDestination.Height = (int)(gameRes.Y * canvasScaling);
        canvasDestination.X = (size.X - canvasDestination.Width) / 2;
        canvasDestination.Y = (size.Y - canvasDestination.Height) / 2;
        canvasDestination.Inflate(CanvasExpandSize / 2 * canvasScaling, CanvasExpandSize / 2 * canvasScaling);

        // create/update mouse matrix
        Matrix mTranslate = Matrix.CreateTranslation(new Vector3(-canvasDestination.X, -canvasDestination.Y, 0));
        Matrix mLowResScale = Matrix.CreateScale(1 / canvasScaling);
        mouseMatrix = mTranslate * mLowResScale;
    }

    private void ChangeResolution(int width, int height) {
        SceneManager.CurrentScene?.ChangeResolution(width, height);
        Renderer?.ChangeResolution(width, height);

        OnResolutionChange?.Invoke(width, height);
    }

    private void DrawCropBars() {
        Rectangle nonExpandedRect = canvasDestination;
        nonExpandedRect.Inflate(
            -CanvasExpandSize / 2 * canvasScaling,
            -CanvasExpandSize / 2 * canvasScaling
        );

        bool drawHoriz = nonExpandedRect.X == 0;

        Rectangle minBar = new(
            0,
            0,
            drawHoriz ? nonExpandedRect.Width : nonExpandedRect.Left,
            drawHoriz ? nonExpandedRect.Top : nonExpandedRect.Height
        );

        Rectangle maxBar = new(
            drawHoriz ? 0 : nonExpandedRect.Right,
            drawHoriz ? nonExpandedRect.Bottom : 0,
            drawHoriz ? nonExpandedRect.Width : nonExpandedRect.Left,
            drawHoriz ? nonExpandedRect.Top : nonExpandedRect.Height
        );

        Renderer.SpriteBatch.Begin();
        Renderer.SpriteBatch.DrawRectFill(minBar, Renderer.Settings.ClearColor);
        Renderer.SpriteBatch.DrawRectFill(maxBar, Renderer.Settings.ClearColor);
        Renderer.SpriteBatch.End();
    }

    #endregion
}
