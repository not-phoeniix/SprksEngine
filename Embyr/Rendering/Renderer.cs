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
}
