using Embyr.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Embyr.Rendering;

internal abstract class Renderer3D : Renderer {
    private readonly Menu? loadingMenu;

    protected readonly RenderLayer MainLayer;
    protected readonly RenderLayer DebugLayer;
    protected readonly RenderLayer UILayer;
    protected readonly RenderLayer UIDebugLayer;

    public Renderer3D(RendererSettings settings, GraphicsDevice gd, Menu? loadingMenu) : base(settings, gd) {
        this.loadingMenu = loadingMenu;

        MainLayer = new RenderLayer(EngineSettings.GameCanvasResolution, gd);
        DebugLayer = new RenderLayer(EngineSettings.GameCanvasResolution, gd);
        UILayer = new RenderLayer(EngineSettings.GameCanvasResolution, gd);
        UIDebugLayer = new RenderLayer(EngineSettings.GameCanvasResolution, gd);
    }

    public override void Render(Rectangle canvasDestination, float canvasScale) {
        GraphicsDevice.SetRenderTarget(null);
        GraphicsDevice.Clear(EngineSettings.RenderClearColor);
        SpriteBatch.Begin(samplerState: SamplerState.PointClamp);

        MainLayer.SmoothingOffset = Vector2.Zero;
        MainLayer.Draw(SpriteBatch, canvasDestination, canvasScale);
        DebugLayer.SmoothingOffset = Vector2.Zero;
        DebugLayer.Draw(SpriteBatch, canvasDestination, canvasScale);

        UILayer.SmoothingOffset = Vector2.Zero;
        UILayer.Draw(SpriteBatch, canvasDestination, canvasScale);
        UIDebugLayer.SmoothingOffset = Vector2.Zero;
        UIDebugLayer.Draw(SpriteBatch, canvasDestination, canvasScale);

        SpriteBatch.End();
    }

    public override void RenderLoading() {
        if (loadingMenu != null) {
            UILayer.DrawTo(loadingMenu.Draw, SpriteBatch);

            if (EngineSettings.ShowDebugDrawing) {
                UILayer.DrawTo(loadingMenu.DebugDraw, SpriteBatch);
            }
        }
    }

    /// <inheritdoc/>
    public override void ChangeResolution(int width, int height, int canvasExpandSize) {
        base.ChangeResolution(width, height, canvasExpandSize);
        loadingMenu?.ChangeResolution(width, height, canvasExpandSize);
        MainLayer?.ChangeResolution(width, height, canvasExpandSize);
        DebugLayer?.ChangeResolution(width, height, canvasExpandSize);
        UILayer?.ChangeResolution(width, height, canvasExpandSize);
        UIDebugLayer?.ChangeResolution(width, height, canvasExpandSize);
    }
}
