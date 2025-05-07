using Embyr.Scenes;
using Embyr.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Embyr.Rendering;

public class RendererForward3D : Renderer3D {
    public RendererForward3D(RendererSettings settings, GraphicsDevice gd, Menu? loadingMenu)
    : base(settings, gd, loadingMenu) {
    }

    public override void RenderScene(Scene inputScene) {
        // don't render non-3D scenes!
        if (inputScene is not Scene3D scene) return;

        GraphicsDevice.SetRenderTarget(MainLayer.RenderTarget);
        GraphicsDevice.Clear(Color.Transparent);

        foreach (IActor3D actor in scene.GetActorsInViewport(scene.Camera.ViewBounds)) {
            actor.Draw(scene.Camera);
        }
    }
}
