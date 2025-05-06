using Embyr.Scenes;
using Embyr.UI;
using Microsoft.Xna.Framework.Graphics;

namespace Embyr.Rendering;

public class RendererForward3D : Renderer3D {
    private readonly Menu? loadingMenu;

    public RendererForward3D(RendererSettings settings, GraphicsDevice gd, Menu? loadingMenu)
    : base(settings, gd, loadingMenu) {
        this.loadingMenu = loadingMenu;
    }

    public override void RenderScene(Scene scene) {
    }
}
