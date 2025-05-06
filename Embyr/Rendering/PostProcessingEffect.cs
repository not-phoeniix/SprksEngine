using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Embyr.Rendering;

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

        /// <summary>
        /// Gets the render target of this effect pass
        /// </summary>
        public RenderTarget2D RenderTarget { get; private set; }

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
        /// <param name="width">Resolution width in pixels of effect pass</param>
        /// <param name="height">Resolution height in pixels of effect pass</param>
        public Pass(Effect shader, GraphicsDevice graphicsDevice, Action<Effect>? passShaderDataHandler, int width, int height) {
            this.shader = shader;
            this.passShaderDataHandler = passShaderDataHandler;
            this.ClearColor = Color.Black;
            RenderTarget = new RenderTarget2D(graphicsDevice, width, height);
        }

        /// <summary>
        /// Draws a screenspace target using the shader of this pass to its internal render target
        /// </summary>
        /// <param name="inputTarget">Input screenspace target to render with shader</param>
        /// <param name="sb">SpriteBatch to draw with</param>
        public void Draw(RenderTarget2D inputTarget, SpriteBatch sb) {
            passShaderDataHandler?.Invoke(shader);
            shader.Parameters["ScreenRes"]?.SetValue(new Vector2(RenderTarget.Width, RenderTarget.Height));

            sb.GraphicsDevice.SetRenderTarget(RenderTarget);
            sb.GraphicsDevice.Clear(ClearColor);

            sb.Begin(samplerState: SamplerState.PointClamp, effect: shader);
            sb.Draw(inputTarget, new Rectangle(0, 0, RenderTarget.Width, RenderTarget.Height), Color.White);
            sb.End();
        }

        /// <summary>
        /// Resizes the resolution of this effect pass
        /// </summary>
        /// <param name="width">New width in pixels of this pass</param>
        /// <param name="height">New height in pixels of this pass</param>
        public void Resize(int width, int height) {
            RenderTarget?.Dispose();
            RenderTarget = new RenderTarget2D(shader.GraphicsDevice, width, height);
        }

        /// <summary>
        /// Disposes memory for this pass
        /// </summary>
        public void Dispose() {
            RenderTarget?.Dispose();
        }
    }

    private readonly List<Pass> passes;

    /// <summary>
    /// Gets the width of this effect target in pixels
    /// </summary>
    public int Width { get; private set; }

    /// <summary>
    /// Gets the height of this effect target in pixels
    /// </summary>
    public int Height { get; private set; }

    /// <summary>
    /// Graphics device of this effect
    /// </summary>
    protected readonly GraphicsDevice GraphicsDevice;

    /// <summary>
    /// Gets a reference to the inputted render target from before this effect is applied
    /// </summary>
    public RenderTarget2D InputRenderTarget { get; set; }

    /// <summary>
    /// Gets the render target of the final effect in this post processing effect
    /// </summary>
    public RenderTarget2D FinalRenderTarget {
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
    public PostProcessingEffect(GraphicsDevice graphicsDevice) {
        this.GraphicsDevice = graphicsDevice ?? throw new NullReferenceException("Graphics device null, cannot create post processing effect!");
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
        for (int i = 0; i < passes.Count; i++) {
            RenderTarget2D prevTarget = i == 0
                ? InputRenderTarget
                : passes[i - 1].RenderTarget;

            passes[i].Draw(prevTarget, sb);
        }
    }

    /// <summary>
    /// Changes the resolution of this post processing effect
    /// </summary>
    /// <param name="width">New width in pixels</param>
    /// <param name="height">New height in pixels</param>
    /// <param name="canvasExpandSize">Extra expand size in pixels</param>
    public virtual void ChangeResolution(int width, int height, int canvasExpandSize) {
        foreach (Pass p in passes) {
            p.Resize(width + canvasExpandSize, height + canvasExpandSize);
        }

        Width = width + canvasExpandSize;
        Height = height + canvasExpandSize;
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
