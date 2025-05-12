using Embyr.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Embyr.Rendering;

public abstract class Renderer : IResolution {
    protected readonly List<PostProcessingEffect> PostProcessingEffects;
    public readonly GraphicsDevice GraphicsDevice;
    public readonly SpriteBatch SpriteBatch;

    public RendererSettings Settings { get; }

    public Renderer(RendererSettings settings, GraphicsDevice gd) {
        GraphicsDevice = gd ?? throw new NullReferenceException("Cannot initialize renderer with null graphics device!");
        SpriteBatch = new SpriteBatch(gd);
        this.Settings = settings ?? throw new NullReferenceException("Cannot initialize renderer with null settings object!");
        PostProcessingEffects = new List<PostProcessingEffect>();
    }

    public abstract void RenderScene(Scene scene);
    public abstract void RenderLoading();
    public abstract void Render(Rectangle canvasDestination, float canvasScale);

    public virtual void ChangeResolution(int width, int height, int canvasExpandSize) {
        foreach (PostProcessingEffect fx in PostProcessingEffects) {
            fx.ChangeResolution(width, height, canvasExpandSize);
        }
    }

    public void AddPostProcessingEffect(PostProcessingEffect effect) {
        if (effect != null) {
            PostProcessingEffects.Add(effect);
        }
    }

    public bool RemovePostProcessingEffect(PostProcessingEffect effect) {
        return PostProcessingEffects.Remove(effect);
    }

    public void ClearPostProcessingEffects() {
        foreach (PostProcessingEffect fx in PostProcessingEffects) {
            fx.Dispose();
        }

        PostProcessingEffects.Clear();
    }

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
                () => SpriteBatch.Draw(prevTarget, Vector2.Zero, Color.White),
                SpriteBatch
            );
        }
    }
}
