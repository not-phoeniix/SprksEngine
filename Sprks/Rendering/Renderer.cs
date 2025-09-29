using Sprks.Scenes;
using Sprks.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sprks.Rendering;

/// <summary>
/// An abstract class that represents a renderer
/// </summary>
internal abstract class Renderer : IResolution {
    private readonly ToneMapGammaPPE toneMapGammaPPE;
    private RenderTarget2D? postProcessPrevTarget;

    /// <summary>
    /// List of all post processing effects to use when drawing
    /// </summary>
    protected readonly List<PostProcessingEffect> PostProcessingEffects;

    /// <summary>
    /// Graphics device associated with this renderer
    /// </summary>
    public readonly GraphicsDevice GraphicsDevice;

    /// <summary>
    /// SpriteBatch associated with this renderer
    /// </summary>
    public readonly SpriteBatch SpriteBatch;

    /// <summary>
    /// Settings to use when rendering
    /// </summary>
    public RendererSettings Settings { get; }

    /// <summary>
    /// Render layer that scene is rendered to
    /// </summary>
    protected readonly RenderLayer SceneRenderLayer;

    /// <summary>
    /// Render layer that all overlaying UI is rendered to
    /// </summary>
    protected readonly RenderLayer UIRenderLayer;

    /// <summary>
    /// Creates a new Renderer
    /// </summary>
    /// <param name="settings">Settings to use when rendering</param>
    /// <param name="gd">Graphics device used to create renderer</param>
    public Renderer(RendererSettings settings, GraphicsDevice gd) {
        GraphicsDevice = gd ?? throw new NullReferenceException("Cannot initialize renderer with null graphics device!");
        SpriteBatch = new SpriteBatch(gd);
        this.Settings = settings ?? throw new NullReferenceException("Cannot initialize renderer with null settings object!");
        PostProcessingEffects = new List<PostProcessingEffect>();
        this.toneMapGammaPPE = new ToneMapGammaPPE(gd);

        Point res = EngineSettings.GameCanvasResolution + new Point(Game.CanvasExpandSize);
        SceneRenderLayer = new RenderLayer(res, gd, SurfaceFormat.HalfVector4, true);
        UIRenderLayer = new RenderLayer(res, gd, SurfaceFormat.Color, false);
    }

    /// <summary>
    /// Renders a scene
    /// </summary>
    /// <param name="scene">Scene to render</param>
    public abstract void RenderScene(Scene scene);

    /// <summary>
    /// Renders an already rendered scene output to the screen
    /// </summary>
    /// <param name="canvasDestination">Destination rectangle to render scene into</param>
    /// <param name="canvasScale">Scale of canvas on the screen</param>
    public virtual void Render(Rectangle canvasDestination, float canvasScale) {
        // draw different layers themselves to the screen
        GraphicsDevice.SetRenderTarget(null);
        GraphicsDevice.Clear(Settings.ClearColor);
        SpriteBatch.Begin(samplerState: SamplerState.PointClamp);

        SceneRenderLayer.Draw(SpriteBatch, canvasDestination, canvasScale);
        UIRenderLayer.Draw(SpriteBatch, canvasDestination, canvasScale);

        SpriteBatch.End();
    }

    /// <inheritdoc/>
    public virtual void ChangeResolution(int width, int height) {
        foreach (PostProcessingEffect fx in PostProcessingEffects) {
            fx.ChangeResolution(width, height);
        }

        toneMapGammaPPE.ChangeResolution(width, height);
        SceneRenderLayer.ChangeResolution(width, height);
        UIRenderLayer.ChangeResolution(width, height);
    }

    /// <summary>
    /// Adds a post processing effect to the end of this rendering pipeline
    /// </summary>
    /// <param name="effect">Effect to add</param>
    public void AddPostProcessingEffect(PostProcessingEffect effect) {
        if (effect != null) {
            PostProcessingEffects.Add(effect);
        }
    }

    /// <summary>
    /// Removes a post processing effect from this rendering pipeline
    /// </summary>
    /// <param name="effect">Effect to remove</param>
    /// <returns>True if successfully removed, false if otherwise</returns>
    public bool RemovePostProcessingEffect(PostProcessingEffect effect) {
        return PostProcessingEffects.Remove(effect);
    }

    /// <summary>
    /// Clears all post processing effects from this renderer
    /// </summary>
    public void ClearPostProcessingEffects() {
        foreach (PostProcessingEffect fx in PostProcessingEffects) {
            fx.Dispose();
        }

        PostProcessingEffects.Clear();
    }

    /// <summary>
    /// Renders all post processing effects onto a target render layer
    /// </summary>
    /// <param name="targetLayer">Render layer to render effects onto</param>
    protected void RenderPostProcessing(RenderLayer targetLayer) {
        // render all PRE-TONEMAP post processing effects !!
        postProcessPrevTarget = targetLayer.RenderTarget;
        for (int i = 0; i < PostProcessingEffects.Count; i++) {
            // just don't do any post processing if it's disabled!
            if (!Settings.EnablePostProcessing) break;

            // grab reference to iteration effect, skip if disabled
            PostProcessingEffect fx = PostProcessingEffects[i];
            if (!fx.Enabled || fx.PostToneMap) continue;

            fx.InputRenderTarget = postProcessPrevTarget;
            fx.Draw(SpriteBatch);

            postProcessPrevTarget = fx.FinalRenderTarget;
        }

        // ~~~ apply tone mapping ~~~
        toneMapGammaPPE.Gamma = Settings.Gamma;
        toneMapGammaPPE.EnableTonemapping = Settings.EnableTonemapping;
        toneMapGammaPPE.InputRenderTarget = postProcessPrevTarget;
        toneMapGammaPPE.Draw(SpriteBatch);
        postProcessPrevTarget = toneMapGammaPPE.FinalRenderTarget;

        // render all POST-TONEMAP post processing effects !!
        for (int i = 0; i < PostProcessingEffects.Count; i++) {
            // just don't do any post processing if it's disabled!
            if (!Settings.EnablePostProcessing) break;

            // grab reference to iteration effect, skip if disabled
            PostProcessingEffect fx = PostProcessingEffects[i];
            if (!fx.Enabled || !fx.PostToneMap) continue;

            fx.InputRenderTarget = postProcessPrevTarget;
            fx.Draw(SpriteBatch);

            postProcessPrevTarget = fx.FinalRenderTarget;
        }

        // draw final post process layer BACK to world layer
        targetLayer.ScreenSpaceEffect = null;
        targetLayer.DrawTo(DrawFinalTarget, SpriteBatch);
    }

    private void DrawFinalTarget(SpriteBatch sb) {
        sb.Draw(postProcessPrevTarget, Vector2.Zero, Color.White);
    }
}
