using Embyr.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Embyr.Rendering;

/// <summary>
/// An abstract class that represents a renderer
/// </summary>
internal abstract class Renderer : IResolution {
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
    /// Creates a new Renderer
    /// </summary>
    /// <param name="settings">Settings to use when rendering</param>
    /// <param name="gd">Graphics device used to create renderer</param>
    public Renderer(RendererSettings settings, GraphicsDevice gd) {
        GraphicsDevice = gd ?? throw new NullReferenceException("Cannot initialize renderer with null graphics device!");
        SpriteBatch = new SpriteBatch(gd);
        this.Settings = settings ?? throw new NullReferenceException("Cannot initialize renderer with null settings object!");
        PostProcessingEffects = new List<PostProcessingEffect>();
    }

    /// <summary>
    /// Renders a scene
    /// </summary>
    /// <param name="scene">Scene to render</param>
    public abstract void RenderScene(Scene scene);

    /// <summary>
    /// Renders the loading menu between scenes
    /// </summary>
    public abstract void RenderLoading();

    /// <summary>
    /// Renders an already rendered scene output to the screen
    /// </summary>
    /// <param name="canvasDestination">Destination rectangle to render scene into</param>
    /// <param name="canvasScale">Scale of canvas on the screen</param>
    public abstract void Render(Rectangle canvasDestination, float canvasScale);

    /// <summary>
    /// Changes resolution of this renderer
    /// </summary>
    /// <param name="width">Width in pixels</param>
    /// <param name="height">Height in pixels</param>
    /// <param name="canvasExpandSize">Number of extra expanded pixels for canvas</param>
    public virtual void ChangeResolution(int width, int height, int canvasExpandSize) {
        foreach (PostProcessingEffect fx in PostProcessingEffects) {
            fx.ChangeResolution(width, height, canvasExpandSize);
        }
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
        // render all post processing effects !!
        RenderTarget2D prevTarget = targetLayer.RenderTarget;
        for (int i = 0; i < PostProcessingEffects.Count; i++) {
            // just don't do any post processing if it's disabled!
            if (!Settings.EnablePostProcessing) break;

            // grab reference to iteration effect, skip if disabled
            PostProcessingEffect fx = PostProcessingEffects[i];
            if (!fx.Enabled) continue;

            fx.InputRenderTarget = prevTarget;
            fx.Draw(SpriteBatch);

            prevTarget = fx.FinalRenderTarget;
        }

        // draw final post process layer BACK to world layer
        //   (if any post processes were used in the first place)
        if (prevTarget != targetLayer.RenderTarget) {
            targetLayer.ScreenSpaceEffect = null;
            targetLayer.DrawTo(
                sb => sb.Draw(prevTarget, Vector2.Zero, Color.White),
                SpriteBatch
            );
        }
    }
}
