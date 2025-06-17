using Embyr.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Embyr.Rendering;

/// <summary>
/// An abstract 2D renderer, inherits from <c>Renderer</c>
/// </summary>
internal abstract class Renderer2D : Renderer {
    private readonly Menu? loadingMenu;

    /// <summary>
    /// Render layer that scene is rendered to
    /// </summary>
    protected readonly RenderLayer SceneRenderLayer;

    /// <summary>
    /// Render layer that all overlaying UI is rendered to
    /// </summary>
    protected readonly RenderLayer UIRenderLayer;

    /// <summary>
    /// Creates a new Renderer2D
    /// </summary>
    /// <param name="settings">RendererSettings for renderer to use</param>
    /// <param name="gd">GraphicsDevice to create layers with</param>
    /// <param name="loadingMenu">Optional reference to the loading menu to show when loading between scenes</param>
    public Renderer2D(RendererSettings settings, GraphicsDevice gd, Menu? loadingMenu) : base(settings, gd) {
        this.loadingMenu = loadingMenu;

        Point res = EngineSettings.GameCanvasResolution + new Point(Game.CanvasExpandSize);
        SceneRenderLayer = new RenderLayer(res, gd, SurfaceFormat.HalfVector4);
        UIRenderLayer = new RenderLayer(res, gd, SurfaceFormat.Color);
    }

    /// <inheritdoc/>
    public override void Render(Rectangle canvasDestination, float canvasScale) {
        // draw different layers themselves to the screen
        GraphicsDevice.SetRenderTarget(null);
        GraphicsDevice.Clear(EngineSettings.RenderClearColor);
        SpriteBatch.Begin(samplerState: SamplerState.PointClamp);

        SceneRenderLayer.Draw(SpriteBatch, canvasDestination, canvasScale);
        UIRenderLayer.Draw(SpriteBatch, canvasDestination, canvasScale);

        SpriteBatch.End();
    }

    /// <inheritdoc/>
    public override void RenderLoading() {
        if (loadingMenu != null) {
            UIRenderLayer.DrawTo(loadingMenu.Draw, SpriteBatch);

            if (EngineSettings.ShowDebugDrawing) {
                UIRenderLayer.DrawTo(loadingMenu.DebugDraw, SpriteBatch, resetTarget: false);
            }
        }
    }

    /// <inheritdoc/>
    public override void ChangeResolution(int width, int height, int canvasExpandSize) {
        base.ChangeResolution(width, height, canvasExpandSize);
        loadingMenu?.ChangeResolution(width, height, canvasExpandSize);
        SceneRenderLayer.ChangeResolution(width, height, canvasExpandSize);
        UIRenderLayer.ChangeResolution(width, height, canvasExpandSize);
    }
}
