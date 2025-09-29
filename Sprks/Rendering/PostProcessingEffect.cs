using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Sprks.Rendering;

/// <summary>
/// Abstract parent post processing effect class for rendering
/// </summary>
public abstract class PostProcessingEffect : IDisposable, IResolution {
    /// <summary>
    /// A single post process effect pass, many exist in a post process effect
    /// </summary>
    protected class Pass : IDisposable {
        private readonly Effect shader;
        private readonly Action<Effect>? passShaderDataHandler;
        private readonly SurfaceFormat surfaceFormat;
        private readonly GraphicsDevice graphicsDevice;
        private RenderTarget2D renderTarget;

        /// <summary>
        /// Gets the render target of this effect pass
        /// </summary>
        internal RenderTarget2D RenderTarget {
            get {
                if (renderTarget == null) {
                    throw new NullReferenceException("Render target for PPE pass never initialized!");
                }

                return renderTarget;
            }
        }

        /// <summary>
        /// Color to clear pass's render target
        /// </summary>
        public Color ClearColor { get; set; }

        /// <summary>
        /// Creates a new effect pass
        /// </summary>
        /// <param name="shader">Shader that pass runs on</param>
        /// <param name="graphicsDevice">Graphics device to create pass with</param>
        /// <param name="passShaderDataHandler">Callback to handle passing of shader data, called before drawing</param>
        /// <param name="surfaceFormat">Surface format of pass's internal render target</param>
        public Pass(
            Effect shader,
            GraphicsDevice graphicsDevice,
            Action<Effect>? passShaderDataHandler,
            SurfaceFormat surfaceFormat = SurfaceFormat.HalfVector4
        ) {
            this.shader = shader;
            this.passShaderDataHandler = passShaderDataHandler;
            this.ClearColor = Color.Black;
            this.surfaceFormat = surfaceFormat;
            this.graphicsDevice = graphicsDevice;
        }

        /// <summary>
        /// Creates the render target contained within this pass
        /// </summary>
        /// <param name="width">Width of target in pixels</param>
        /// <param name="height">Height of target in pixels</param>
        internal void CreateRenderTarget(int width, int height) {
            renderTarget = new RenderTarget2D(
                graphicsDevice,
                width,
                height,
                false,
                surfaceFormat,
                DepthFormat.None
            );
        }

        /// <summary>
        /// Draws a screenspace target using the shader of this pass to its internal render target
        /// </summary>
        /// <param name="inputTarget">Input screenspace target to render with shader</param>
        /// <param name="sb">SpriteBatch to draw with</param>
        internal void Draw(RenderTarget2D inputTarget, SpriteBatch sb) {
            if (renderTarget == null) {
                throw new NullReferenceException("Cannot draw post processing effect, Render Target was never created!");
            }

            if (inputTarget == null) {
                throw new NullReferenceException("Cannot draw post processing effect, inputted render target is null!");
            }

            passShaderDataHandler?.Invoke(shader);
            shader.Parameters["ScreenRes"]?.SetValue(new Vector2(renderTarget.Width, renderTarget.Height));

            sb.GraphicsDevice.SetRenderTarget(RenderTarget);
            sb.GraphicsDevice.Clear(ClearColor);

            sb.Begin(samplerState: SamplerState.PointClamp, effect: shader);
            sb.Draw(inputTarget, new Rectangle(0, 0, renderTarget.Width, renderTarget.Height), Color.White);
            sb.End();
        }

        /// <summary>
        /// Resizes the resolution of this effect pass
        /// </summary>
        /// <param name="width">New width in pixels of this pass</param>
        /// <param name="height">New height in pixels of this pass</param>
        internal void Resize(int width, int height) {
            renderTarget?.Dispose();
            renderTarget = new RenderTarget2D(
                shader.GraphicsDevice,
                width,
                height,
                false,
                surfaceFormat,
                DepthFormat.None
            );
        }

        /// <summary>
        /// Disposes memory for this pass
        /// </summary>
        public void Dispose() {
            renderTarget?.Dispose();
        }
    }

    private readonly List<Pass> passes;

    /// <summary>
    /// Gets the width of this effect target in pixels
    /// </summary>
    internal int Width { get; private set; }

    /// <summary>
    /// Gets the height of this effect target in pixels
    /// </summary>
    internal int Height { get; private set; }

    /// <summary>
    /// Gets whether or not post processing effect
    /// should apply after tonemapping at the end of the pipeline
    /// </summary>
    internal bool PostToneMap { get; }

    /// <summary>
    /// Graphics device of this effect
    /// </summary>
    protected readonly GraphicsDevice GraphicsDevice;

    /// <summary>
    /// Gets a reference to the inputted render target from before this effect is applied
    /// </summary>
    internal RenderTarget2D? InputRenderTarget { get; set; }

    /// <summary>
    /// Gets the render target of the final effect in this post processing effect
    /// </summary>
    internal RenderTarget2D FinalRenderTarget {
        get {
            if (passes.Count == 0) {
                throw new Exception("Cannot get final render target - no passes set up in effect!");
            }

            return passes[passes.Count - 1].RenderTarget;
        }
    }

    /// <summary>
    /// Gets/sets whether or not to enable this effect in rendering
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Creates a new PostProcessingEffect
    /// </summary>
    /// <param name="graphicsDevice">Graphics Device to create effect with</param>
    /// <param name="postToneMap">Whether or not to trigger post processing effect after tonemapping is applied, at the end of the pipeline</param>
    public PostProcessingEffect(GraphicsDevice graphicsDevice, bool postToneMap = false) {
        this.GraphicsDevice = graphicsDevice ?? throw new NullReferenceException("Graphics device null, cannot create post processing effect!");
        this.PostToneMap = postToneMap;
        passes = new List<Pass>();
        Enabled = true;

        Point res = EngineSettings.GameCanvasResolution + new Point(Game.CanvasExpandSize);
        Width = res.X;
        Height = res.Y;
    }

    /// <summary>
    /// Draws this post processing effect
    /// </summary>
    /// <param name="sb">SpriteBatch to draw with</param>
    public void Draw(SpriteBatch sb) {
        if (InputRenderTarget == null) {
            throw new NullReferenceException("Cannot draw post processing effect with null input render target!");
        }

        for (int i = 0; i < passes.Count; i++) {
            RenderTarget2D prevTarget = i == 0
                ? InputRenderTarget
                : passes[i - 1].RenderTarget!;

            passes[i].Draw(prevTarget, sb);
        }
    }

    /// <inheritdoc/>
    public virtual void ChangeResolution(int width, int height) {
        foreach (Pass p in passes) {
            p.Resize(width, height);
        }

        Width = width;
        Height = height;
    }

    /// <summary>
    /// Disposes memory for this post processing effect
    /// </summary>
    public void Dispose() {
        foreach (Pass p in passes) {
            p.Dispose();
        }

        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Adds a pass to this effect
    /// </summary>
    /// <param name="pass">Pass to add</param>
    protected void AddPass(Pass pass) {
        if (pass != null) {
            passes.Add(pass);
            pass.CreateRenderTarget(Width, Height);
        }
    }

    /// <summary>
    /// Removes a pass from this effect
    /// </summary>
    /// <param name="pass">Pass to remove</param>
    /// <returns>True if successfully removed, false if not</returns>
    protected bool RemovePass(Pass pass) {
        return passes.Remove(pass);
    }

    /// <summary>
    /// Clears and disposes passes in this effect
    /// </summary>
    protected void ClearPasses() {
        foreach (Pass p in passes) {
            p.Dispose();
        }

        passes.Clear();
    }
}
