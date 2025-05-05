using Embyr.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Embyr.Rendering;

public abstract class Renderer : IResolution {
    public GraphicsDevice GraphicsDevice { get; private set; }

    public Renderer(GraphicsDevice gd) {
        GraphicsDevice = gd ?? throw new NullReferenceException("Cannot initialize render pipeline with null graphics device!");
    }

    public abstract void RenderScene(Scene scene);
    public abstract void RenderLoading();
    public abstract void Render(Rectangle canvasDestination, float canvasScale);

    public abstract void ChangeResolution(int width, int height, int canvasExpandSize);
}
