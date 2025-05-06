using Embyr.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Embyr.Rendering;

public abstract class Renderer : IResolution {
    protected readonly List<PostProcessingEffect> PostProcessingEffects;
    public readonly GraphicsDevice GraphicsDevice;

    public Renderer(GraphicsDevice gd) {
        GraphicsDevice = gd ?? throw new NullReferenceException("Cannot initialize render pipeline with null graphics device!");
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
