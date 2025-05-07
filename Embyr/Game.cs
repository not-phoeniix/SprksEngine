using System.Diagnostics;
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
/// Represents all possible render layers in game
/// </summary>
public enum GameLayer {
    //! these should be sorted in order of drawing! back to front!

    /// <summary>
    /// Background parallax layer, furthest from world
    /// </summary>
    ParallaxBg,

    /// <summary>
    /// Far-distanced parallax layer relative to world
    /// </summary>
    ParallaxFar,

    /// <summary>
    /// Medium-distanced parallax layer to world
    /// </summary>
    ParallaxMid,

    /// <summary>
    /// Nearest parallax layer to world
    /// </summary>
    ParallaxNear,

    /// <summary>
    /// Main world layer, where all gameplay and world goes, transformed by camera matrix
    /// </summary>
    World,

    /// <summary>
    /// Main debug for world layer, where debug information is drawn to in world-space, transformed by camera matrix
    /// </summary>
    WorldDebug,

    /// <summary>
    /// UI layer, where all UI is drawn to, independent of world space
    /// </summary>
    UI,

    /// <summary>
    /// Debug ui layer, where all UI's debug info is drawn to
    /// </summary>
    UIDebug,
}

/// <summary>
/// Game class that all the game runs within
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
        public Scene? InitialScene { get; init; }
        public Menu? LoadingMenu { get; init; }
        public Point CanvasRes { get; init; }
        public Point WindowRes { get; init; }
        public Color RenderClearColor { get; init; }
        public RenderPipeline? RenderPipeline { get; init; }
        public string WindowTitle { get; init; }
        public bool EnableVSync { get; init; }
        public bool IsFullscreen { get; init; }
        public bool IsBorderless { get; init; }
        public ActionBindingPreset DefaultBindingPreset { get; init; }

        /// <summary>
        /// Creates a new SetupParams instance with default values
        /// </summary>
        public GameSetupParams() {
            InitialScene = null;
            LoadingMenu = null;
            CanvasRes = new Point(480, 270);
            WindowRes = new Point(1280, 720);
            RenderClearColor = Palette.Col2;
            WindowTitle = "Embyr Project";
            EnableVSync = true;
            IsFullscreen = false;
            IsBorderless = false;
            RenderPipeline = null;
            DefaultBindingPreset = ActionBindingPreset.Default;
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

    internal static readonly int CanvasExpandSize = 32;

    private readonly GraphicsDeviceManager graphics;
    private GameSetupParams setupParams;

    private bool isActivePrev;
    private Menu? loadingMenu;
    private Point prevWindowedSize;

    private Rectangle canvasDestination;
    private float canvasScaling;
    private Matrix mouseMatrix;
    private bool isResizing;
    private Renderer renderer;

    /// <summary>
    /// Action called when focus is lost on window
    /// </summary>
    public Action OnLoseFocus;

    /// <summary>
    /// Action called when low res canvas resolution changes, parameters are width, height, and canvas expand size
    /// </summary>
    public Action<int, int, int> OnResolutionChange;

    /// <summary>
    /// Gets bounds of any menu to create
    /// </summary>
    protected static Rectangle MenuBounds => new(
        new Point(CanvasExpandSize / 2),
        EngineSettings.GameCanvasResolution
    );

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

    protected override sealed void Initialize() {
        Debug.WriteLine("Setting up Embyr game...");

        setupParams = SetupGame();

        if (setupParams.InitialScene == null) {
            throw new NullReferenceException("Cannot set up game with null initial scene!");
        }

        EngineSettings.GameCanvasResolution = setupParams.CanvasRes;
        EngineSettings.CurrentBindingPreset = setupParams.DefaultBindingPreset;
        EngineSettings.EnableVSync = setupParams.EnableVSync;
        EngineSettings.IsFullscreen = setupParams.IsFullscreen;
        EngineSettings.IsBorderless = setupParams.IsBorderless;
        EngineSettings.RenderClearColor = setupParams.RenderClearColor;

        loadingMenu = setupParams.LoadingMenu;

        graphics.PreferredBackBufferWidth = EngineSettings.GameWindowResolution.X;
        graphics.PreferredBackBufferHeight = EngineSettings.GameWindowResolution.Y;
        prevWindowedSize = EngineSettings.GameWindowResolution;
        graphics.SynchronizeWithVerticalRetrace = EngineSettings.EnableVSync;
        graphics.HardwareModeSwitch = !EngineSettings.IsBorderless;
        graphics.IsFullScreen = EngineSettings.IsFullscreen;
        IsFixedTimeStep = false;

        // allowing user resizing seems to explode things <3 unsure why <3
        // Window.AllowUserResizing = true;
        Window.ClientSizeChanged += OnResize;
        Window.Title = setupParams.WindowTitle;

        Point res = EngineSettings.GameCanvasResolution + new Point(CanvasExpandSize);

        ResizeCanvasDestination();

        base.Initialize();
    }

    protected override sealed void LoadContent() {
        ShaderManager.I.Init(GraphicsDevice);
        ContentHelper.I.Init(this);

        RendererSetupParams rParams = SetupRenderer();

        renderer = setupParams.RenderPipeline switch {
            RenderPipeline.Deferred2D => new RendererDeferred2D(rParams.RendererSettings, GraphicsDevice, loadingMenu),
            RenderPipeline.Forward3D => new RendererForward3D(rParams.RendererSettings, GraphicsDevice, loadingMenu),
            _ => throw new Exception("Inputted render pipeline not recognized!")
        };

        foreach (PostProcessingEffect fx in rParams.PostProcessingEffects) {
            renderer.AddPostProcessingEffect(fx);
        }

        // set up scene when loading content !!
        SceneManager.I.Init(this, setupParams.InitialScene!);
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

    protected override void Update(GameTime gameTime) {
        Performance.Update(gameTime);
        Performance.FrametimeMeasureStart();
        Performance.UpdateMeasureStart();

        Matrix invertCam2DMat = Matrix.Identity;
        if (SceneManager.I.CurrentScene is Scene2D scene) {
            invertCam2DMat = scene.Camera.InvertedMatrix;
        }

        Input.Update(
            mouseMatrix,
            invertCam2DMat,
            EngineSettings.CurrentBindingPreset,
            Performance.DeltaTime
        );

        if (Input.IsKeyDown(Keys.RightAlt) && Input.IsKeyDownOnce(Keys.N)) {
            Task.Run(GC.Collect);
            Debug.WriteLine("Running garbage collector Collect...");
        }

        if (Input.IsKeyDown(Keys.RightAlt) && Input.IsKeyDownOnce(Keys.M)) {
            Debug.WriteLine($"{GC.GetTotalMemory(false) / 1_000_000} mb used");
        }

        bool altPressed = Input.IsKeyDown(Keys.LeftAlt) || Input.IsKeyDown(Keys.RightAlt);
        if (altPressed && Input.IsKeyDownOnce(Keys.Enter)) {
            EngineSettings.IsFullscreen = !EngineSettings.IsFullscreen;
        }

        if (!SceneManager.I.IsLoading) {
            for (int i = 0; i < Performance.NumPhysicsUpdateToRun; i++) {
                SceneManager.I.PhysicsUpdate(Performance.FixedDeltaTime);
            }
            SceneManager.I.Update(Performance.DeltaTime);
        } else {
            for (int i = 0; i < Performance.NumPhysicsUpdateToRun; i++) {
                loadingMenu?.PhysicsUpdate(Performance.FixedDeltaTime);
            }
            loadingMenu?.Update(Performance.DeltaTime);
        }

        // if game goes inactive, pause
        if (!IsActive && isActivePrev) {
            OnLoseFocus?.Invoke();
        }

        // hide mouse cursor when using a controller
        IsMouseVisible = Input.Mode == InputMode.Keyboard;

        isActivePrev = IsActive;

        if (EngineSettings.ShouldApplyGraphicsChanges) {
            ChangeResolution(
                EngineSettings.GameCanvasResolution.X,
                EngineSettings.GameCanvasResolution.Y
            );
            SetFullscreen(EngineSettings.IsFullscreen, EngineSettings.IsBorderless);
            graphics.SynchronizeWithVerticalRetrace = EngineSettings.EnableVSync;

            graphics.ApplyChanges();

            EngineSettings.ShouldApplyGraphicsChanges = false;
        }

        Performance.UpdateMeasureEnd();

        base.Update(gameTime);
    }

    protected override sealed void Draw(GameTime gameTime) {
        Performance.DrawMeasureStart();

        if (!SceneManager.I.IsLoading) {
            renderer.RenderScene(SceneManager.I.CurrentScene);
        } else {
            renderer.RenderLoading();
        }

        renderer.Render(canvasDestination, canvasScaling);

        base.Draw(gameTime);

        Performance.DrawMeasureEnd();
        Performance.FrametimeMeasureEnd();

#if DEBUG
        if (Input.IsKeyDown(Keys.K)) {
            Debug.WriteLine(
                $"Avg Frametime: {Performance.FrametimeAvg}\n" +
                $"Avg Update time: {Performance.UpdateTimeAvg}\n" +
                $"Avg Draw time: {Performance.DrawTimeAvg}\n"
            );
        }
#endif
    }

    #endregion

    #region // Helper methods

    /// <summary>
    /// Sets fullscreen state of game
    /// </summary>
    /// <param name="fullscreen">Whether or not to enable fullscreen</param>
    /// <param name="borderless">Whether or not to make window borderless</param>
    private void SetFullscreen(bool fullscreen, bool borderless) {
        graphics.HardwareModeSwitch = !borderless;

        if (fullscreen) {
            // save previous windowed size
            prevWindowedSize.X = Window.ClientBounds.Width;
            prevWindowedSize.Y = Window.ClientBounds.Height;

            // set width and height to be that of the current system screen
            graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
        } else {
            // set to previous windowed size
            graphics.PreferredBackBufferWidth = prevWindowedSize.X;
            graphics.PreferredBackBufferHeight = prevWindowedSize.Y;
        }

        graphics.IsFullScreen = fullscreen;
        graphics.ApplyChanges();
        ResizeCanvasDestination();
    }

    /// <summary>
    /// Method that is run every time the window is resized
    /// </summary>
    private void OnResize(object? sender, EventArgs? e) {
        if (!isResizing && Window.ClientBounds.Width > 0 && Window.ClientBounds.Height > 0) {
            isResizing = true;

            FixWindow();
            ResizeCanvasDestination();

            isResizing = false;
        }
    }

    private void FixWindow() {
        graphics.PreferredBackBufferWidth = Window.ClientBounds.Width;
        graphics.PreferredBackBufferHeight = Window.ClientBounds.Height;

        // cap width/height to be the size of the screen,
        //   prevents crashing with really big resolutions
        graphics.PreferredBackBufferWidth = Math.Clamp(graphics.PreferredBackBufferWidth, 1, GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width);
        graphics.PreferredBackBufferHeight = Math.Clamp(graphics.PreferredBackBufferHeight, 1, GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height);

        graphics.ApplyChanges();
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
        SceneManager.I.ChangeResolution(width, height, CanvasExpandSize);

        OnResolutionChange?.Invoke(width, height, CanvasExpandSize);

        ResizeCanvasDestination();
    }

    #endregion
}
