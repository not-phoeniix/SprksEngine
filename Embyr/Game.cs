using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Embyr.Scenes;
using Embyr.Tools;
using Embyr.UI;
using Embyr.Data;

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
    #region // Fields

    internal static readonly int CanvasExpandSize = 32;

    private GraphicsDeviceManager graphics;
    private SpriteBatch spriteBatch;

    private bool isActivePrev;
    private Menu loadingMenu;
    private Point prevWindowedSize;

    private Dictionary<GameLayer, RenderLayer> layers;
    private RenderTarget2D lightBuffer;
    private RenderTarget2D depthBuffer;
    private Effect fxLightCombine;
    private Effect fxSolidColor;

    // bloom things!
    private RenderTarget2D bloomBackBuffer;
    private RenderTarget2D bloomFrontBuffer;
    private Effect fxBloomThreshold;
    private Effect fxBloomBlur;
    private Effect fxBloomCombine;

    // distance field things!
    private RenderTarget2D distanceBackBuffer;
    private RenderTarget2D distanceFrontBuffer;
    private RenderTarget2D worldLayerDistanceField;
    private RenderTarget2D skyLayerDistanceField;
    private Effect fxJumpFloodSeed;
    private Effect fxJumpFloodStep;
    private Effect fxJumpFloodDistRender;

    private Rectangle canvasDestination;
    private float canvasScaling;
    private Matrix mouseMatrix;
    private bool isResizing;

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

    /// <summary>
    /// Simple readonly struct that contains all parameters to set up a new Embyr game, should be created in <c>Game.Setup</c>
    /// </summary>
    public readonly struct SetupParams {
        public Scene InitialScene { get; init; }
        public Menu LoadingMenu { get; init; }
        public Point CanvasRes { get; init; }
        public Point WindowRes { get; init; }
        public string WindowTitle { get; init; }
        public bool EnableVSync { get; init; }
        public bool IsFullscreen { get; init; }
        public bool IsBorderless { get; init; }
        public ActionBindingPreset DefaultBindingPreset { get; init; }

        /// <summary>
        /// Creates a new SetupParams instance with default values
        /// </summary>
        public SetupParams() {
            InitialScene = null;
            LoadingMenu = null;
            CanvasRes = new Point(480, 270);
            WindowRes = new Point(1280, 720);
            WindowTitle = "Embyr Project";
            EnableVSync = true;
            IsFullscreen = false;
            IsBorderless = false;
            DefaultBindingPreset = ActionBindingPreset.Default;
        }
    }

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

        //! check if content helper being initialized here causes
        //!   any errors vs in LoadContet() !!
        ContentHelper.I.Init(this);

        SetupParams sp = Setup();

        if (sp.InitialScene == null) {
            throw new Exception("Cannot set up game with null initial scene!");
        }

        EngineSettings.GameCanvasResolution = sp.CanvasRes;
        EngineSettings.CurrentBindingPreset = sp.DefaultBindingPreset;
        EngineSettings.EnableVSync = sp.EnableVSync;
        EngineSettings.IsFullscreen = sp.IsFullscreen;
        EngineSettings.IsBorderless = sp.IsBorderless;

        loadingMenu = sp.LoadingMenu;

        graphics.PreferredBackBufferWidth = EngineSettings.GameWindowResolution.X;
        graphics.PreferredBackBufferHeight = EngineSettings.GameWindowResolution.Y;
        prevWindowedSize = EngineSettings.GameWindowResolution;
        graphics.SynchronizeWithVerticalRetrace = EngineSettings.EnableVSync;
        graphics.HardwareModeSwitch = !EngineSettings.IsBorderless;
        graphics.IsFullScreen = EngineSettings.IsFullscreen;
        IsFixedTimeStep = false;

        Window.AllowUserResizing = true;
        Window.ClientSizeChanged += OnResize;
        Window.Title = sp.WindowTitle;

        Point res = EngineSettings.GameCanvasResolution + new Point(CanvasExpandSize);

        layers = new Dictionary<GameLayer, RenderLayer>() {
            { GameLayer.World, new(res, GraphicsDevice) },
            { GameLayer.WorldDebug, new(res, GraphicsDevice) },
            { GameLayer.UI, new(res, GraphicsDevice) },
            { GameLayer.UIDebug, new(res, GraphicsDevice) },
            { GameLayer.ParallaxNear, new(res, GraphicsDevice) },
            { GameLayer.ParallaxMid, new(res, GraphicsDevice) },
            { GameLayer.ParallaxFar, new(res, GraphicsDevice) },
            { GameLayer.ParallaxBg, new(res, GraphicsDevice) }
        };

        lightBuffer = new RenderTarget2D(GraphicsDevice, res.X, res.Y);
        depthBuffer = new RenderTarget2D(GraphicsDevice, res.X, res.Y);
        distanceFrontBuffer = new RenderTarget2D(GraphicsDevice, res.X, res.Y);
        distanceBackBuffer = new RenderTarget2D(GraphicsDevice, res.X, res.Y);
        worldLayerDistanceField = new RenderTarget2D(GraphicsDevice, res.X, res.Y);
        skyLayerDistanceField = new RenderTarget2D(GraphicsDevice, res.X, res.Y);
        bloomBackBuffer = new RenderTarget2D(GraphicsDevice, res.X, res.Y);
        bloomFrontBuffer = new RenderTarget2D(GraphicsDevice, res.X, res.Y);

        ResizeCanvasDestination();

        // we don't have to apply changes because that happens
        //   automatically in base.Initialize()
        EngineSettings.ShouldApplyGraphicsChanges = false;

        // initialize scene stuff after everything is done !!
        SceneManager.I.Init(this, sp.InitialScene);

        base.Initialize();
    }

    protected override void LoadContent() {
        spriteBatch = new SpriteBatch(GraphicsDevice);

        fxLightCombine = ContentHelper.I.LoadGlobal<Effect>("shaders/light_combine");
        fxJumpFloodSeed = ContentHelper.I.LoadGlobal<Effect>("shaders/jump_flood_seed");
        fxJumpFloodStep = ContentHelper.I.LoadGlobal<Effect>("shaders/jump_flood_step");
        fxJumpFloodDistRender = ContentHelper.I.LoadGlobal<Effect>("shaders/jump_flood_dist_render");
        fxSolidColor = ContentHelper.I.LoadGlobal<Effect>("shaders/solid_color");
        fxBloomThreshold = ContentHelper.I.LoadGlobal<Effect>("shaders/bloom_threshold");
        fxBloomBlur = ContentHelper.I.LoadGlobal<Effect>("shaders/bloom_blur");
        fxBloomCombine = ContentHelper.I.LoadGlobal<Effect>("shaders/bloom_combine");
    }

    protected abstract SetupParams Setup();

    #endregion

    #region // Game loop

    protected override sealed void Update(GameTime gameTime) {
        Performance.Update(gameTime);
        Performance.FrametimeMeasureStart();
        Performance.UpdateMeasureStart();

        Input.Update(
            mouseMatrix,
            SceneManager.I.Camera?.InvertedMatrix ?? Matrix.Identity,
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
            Scene currentScene = SceneManager.I.CurrentScene;

            Camera camera = SceneManager.I.Camera;
            Matrix worldMatrix = camera.FlooredMatrix;

            // draw to buffers, render lighting
            currentScene.DrawDepthmap(fxSolidColor, depthBuffer, spriteBatch);
            RenderDistanceField(worldLayerDistanceField, depthBuffer, 0.25f);
            RenderDistanceField(skyLayerDistanceField, depthBuffer, 1.0f);
            currentScene.DrawLightsDeferred(spriteBatch, lightBuffer, worldLayerDistanceField, skyLayerDistanceField);

            // ~~~ draw all game layers to their respective RenderLayers ~~~

            // Vector3 sunColorVec3 = currentScene.Sun.Color.ToVector3() * currentScene.Sun.Intensity;

            fxLightCombine.Parameters["LightBuffer"].SetValue(lightBuffer);
            fxLightCombine.Parameters["VolumetricScalar"].SetValue(currentScene.VolumetricScalar);
            fxLightCombine.Parameters["AmbientColor"].SetValue(currentScene.AmbientColor.ToVector3());
            fxLightCombine.Parameters["DistanceField"]?.SetValue(worldLayerDistanceField);
            layers[GameLayer.World].SmoothingOffset = camera.Position;
            layers[GameLayer.World].IndividualEffect = null;
            layers[GameLayer.World].ScreenSpaceEffect = fxLightCombine;
            layers[GameLayer.World].DrawTo(currentScene.Draw, spriteBatch, worldMatrix);

            // render bloom effect after world has been rendered
            RenderBloom(0.98f, 4);

            layers[GameLayer.WorldDebug].SmoothingOffset = camera.Position;
            layers[GameLayer.WorldDebug].DrawTo(currentScene.DebugDraw, spriteBatch, worldMatrix);

            layers[GameLayer.UI].DrawTo(currentScene.DrawOverlays, spriteBatch);
            layers[GameLayer.UIDebug].DrawTo(currentScene.DebugDrawOverlays, spriteBatch);

            void DrawParallax(GameLayer gameLayer, ParallaxBackground bg) {
                ParallaxLayer layer = bg.GetLayer(gameLayer);
                if (layer == null) return;  // don't draw if layer doesn't exist
                layers[gameLayer].SmoothingOffset = bg.GetLayer(gameLayer).WorldLocation;
                layers[gameLayer].ColorTint = currentScene.GlobalLightTint;
                layers[gameLayer].DrawTo(bg.GetLayer(gameLayer).Draw, spriteBatch, worldMatrix);
            }

            ParallaxBackground parallax = currentScene.GetCurrentParallax();
            DrawParallax(GameLayer.ParallaxBg, parallax);
            DrawParallax(GameLayer.ParallaxFar, parallax);
            DrawParallax(GameLayer.ParallaxMid, parallax);
            DrawParallax(GameLayer.ParallaxNear, parallax);

        } else {
            if (loadingMenu != null) {
                layers[GameLayer.UI].DrawTo(loadingMenu.Draw, spriteBatch);

                if (EngineSettings.ShowDebugDrawing) {
                    layers[GameLayer.UIDebug].DrawTo(loadingMenu.DebugDraw, spriteBatch);
                }
            }
        }

        // draw different layers themselves to the screen
        GraphicsDevice.SetRenderTarget(null);
        GraphicsDevice.Clear(Palette.Col0);
        spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        foreach (GameLayer layer in Enum.GetValues<GameLayer>()) {
            // do not draw debug layers if debugging is not enabled
            bool isDebugLayer = layer == GameLayer.WorldDebug || layer == GameLayer.UIDebug;
            if (isDebugLayer && !EngineSettings.ShowDebugDrawing) {
                continue;
            }

            layers[layer].Draw(spriteBatch, canvasDestination, canvasScaling);
        }
        spriteBatch.End();

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
    private void OnResize(object sender, EventArgs e) {
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

        foreach (RenderLayer layer in layers.Values) {
            layer.ChangeResolution(width, height, CanvasExpandSize);
        }

        void ResizeBuffer(ref RenderTarget2D buffer) {
            buffer?.Dispose();
            buffer = new RenderTarget2D(
                GraphicsDevice,
                width + CanvasExpandSize,
                height + CanvasExpandSize
            );
        }

        ResizeBuffer(ref lightBuffer);
        ResizeBuffer(ref depthBuffer);
        ResizeBuffer(ref distanceBackBuffer);
        ResizeBuffer(ref distanceFrontBuffer);
        ResizeBuffer(ref worldLayerDistanceField);
        ResizeBuffer(ref skyLayerDistanceField);
        ResizeBuffer(ref bloomBackBuffer);
        ResizeBuffer(ref bloomFrontBuffer);

        OnResolutionChange?.Invoke(width, height, CanvasExpandSize);

        ResizeCanvasDestination();
    }

    /// <summary>
    /// Renders a distance field to a render target based on a defined target depth
    /// </summary>
    /// <param name="destination">Final destination target for distance field</param>
    /// <param name="depthBuffer">Depth buffer of world</param>
    /// <param name="targetDepth">Target depth to generate distance from in buffer</param>
    private void RenderDistanceField(RenderTarget2D destination, RenderTarget2D depthBuffer, float targetDepth) {
        // https://blog.demofox.org/2016/02/29/fast-voronoi-diagrams-and-distance-dield-textures-on-the-gpu-with-the-jump-flooding-algorithm/

        // render depth buffer initially as seed
        fxJumpFloodSeed.Parameters["TargetDepth"].SetValue(targetDepth);
        spriteBatch.GraphicsDevice.SetRenderTarget(distanceBackBuffer);
        spriteBatch.GraphicsDevice.Clear(Color.Black);
        spriteBatch.Begin(effect: fxJumpFloodSeed);
        spriteBatch.Draw(depthBuffer, new Rectangle(0, 0, distanceBackBuffer.Width, distanceBackBuffer.Height), Color.White);
        spriteBatch.End();

        bool drawToFrontBuffer = true;

        void Step(int offset) {
            RenderTarget2D target;
            RenderTarget2D toDraw;

            if (drawToFrontBuffer) {
                target = distanceFrontBuffer;
                toDraw = distanceBackBuffer;
            } else {
                target = distanceBackBuffer;
                toDraw = distanceFrontBuffer;
            }

            spriteBatch.GraphicsDevice.SetRenderTarget(target);
            spriteBatch.GraphicsDevice.Clear(Color.Black);
            fxJumpFloodStep.Parameters["Offset"].SetValue((float)offset);
            fxJumpFloodStep.Parameters["ScreenRes"].SetValue(new Vector2(toDraw.Width, toDraw.Height));
            spriteBatch.Begin(effect: fxJumpFloodStep);
            // spriteBatch.Draw(toDraw, Vector2.Zero, Color.White);
            spriteBatch.Draw(toDraw, new Rectangle(0, 0, target.Width, target.Height), Color.White);
            spriteBatch.End();

            drawToFrontBuffer = !drawToFrontBuffer;
        }

        // offest should be: 2 ^ (ceil(log2(N)) – passIndex – 1),
        int N = Math.Max(worldLayerDistanceField.Width / 2, worldLayerDistanceField.Height / 2);
        int offset = 100;
        int i = 0;
        while (offset > 0) {
            offset = (int)MathF.Pow(2, MathF.Ceiling(MathF.Log2(N)) - i - 1);
            Step(offset);
            i++;
        }

        // https://en.wikipedia.org/wiki/Jump_flooding_algorithm
        //   JFA+1 can possibly increase accuracy!
        Step(1);

        // get the final buffer that was just drawn to
        RenderTarget2D finalTarget;
        if (drawToFrontBuffer) {
            finalTarget = distanceBackBuffer;
        } else {
            finalTarget = distanceFrontBuffer;
        }

        // draw buffer to actual distance buffer using distance render shader
        spriteBatch.GraphicsDevice.SetRenderTarget(destination);
        spriteBatch.GraphicsDevice.Clear(Color.Black);
        spriteBatch.Begin(effect: fxJumpFloodDistRender);
        // spriteBatch.Draw(finalTarget, Vector2.Zero, Color.White);
        spriteBatch.Draw(finalTarget, new Rectangle(0, 0, destination.Width, destination.Height), Color.White);
        spriteBatch.End();
    }

    private void RenderBloom(float bloomThreshold, int blurPasses) {
        // pass data to shaders before drawing
        Vector2 screenRes = (EngineSettings.GameCanvasResolution + new Point(CanvasExpandSize)).ToVector2();
        fxBloomThreshold.Parameters["Threshold"].SetValue(bloomThreshold);
        fxBloomBlur.Parameters["ScreenRes"].SetValue(screenRes);

        // draw threshold pass
        spriteBatch.GraphicsDevice.SetRenderTarget(bloomBackBuffer);
        spriteBatch.GraphicsDevice.Clear(Color.Black);
        spriteBatch.Begin(effect: fxBloomThreshold);
        layers[GameLayer.World].Draw(spriteBatch, Vector2.Zero);
        spriteBatch.End();

        bool drawToFrontBuffer = true;

        do {
            RenderTarget2D target;
            RenderTarget2D toDraw;
            if (drawToFrontBuffer) {
                target = bloomFrontBuffer;
                toDraw = bloomBackBuffer;
            } else {
                target = bloomBackBuffer;
                toDraw = bloomFrontBuffer;
            }

            // draw blurring pass
            spriteBatch.GraphicsDevice.SetRenderTarget(target);
            spriteBatch.GraphicsDevice.Clear(Color.Black);
            spriteBatch.Begin(effect: fxBloomBlur);
            spriteBatch.Draw(toDraw, Vector2.Zero, Color.White);
            spriteBatch.End();

            drawToFrontBuffer = !drawToFrontBuffer;
            blurPasses--;
        } while (blurPasses > 0);

        RenderTarget2D finalBuffer;
        if (drawToFrontBuffer) {
            finalBuffer = bloomBackBuffer;
        } else {
            finalBuffer = bloomFrontBuffer;
        }

        // combine dry and wet effects (heh music reference)
        fxBloomCombine.Parameters["BloomTexture"].SetValue(finalBuffer);
        layers[GameLayer.World].ScreenSpaceEffect = fxBloomCombine;
        layers[GameLayer.World].DrawTo(
            () => layers[GameLayer.World].Draw(spriteBatch, Vector2.Zero),
            spriteBatch
        );
    }

    #endregion
}
