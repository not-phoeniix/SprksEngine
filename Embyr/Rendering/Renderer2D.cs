using Embyr.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Embyr.Rendering;

public abstract class Renderer2D : Renderer {
    private readonly Menu? loadingMenu;

    protected readonly Dictionary<GameLayer, RenderLayer> Layers;

    public Renderer2D(RendererSettings settings, GraphicsDevice gd, Menu? loadingMenu) : base(settings, gd) {
        this.loadingMenu = loadingMenu;

        Point res = EngineSettings.GameCanvasResolution + new Point(Game.CanvasExpandSize);
        Layers = new() {
            { GameLayer.World, new(res, GraphicsDevice) },
            { GameLayer.WorldDebug, new(res, GraphicsDevice) },
            { GameLayer.UI, new(res, GraphicsDevice) },
            { GameLayer.UIDebug, new(res, GraphicsDevice) },
            { GameLayer.ParallaxNear, new(res, GraphicsDevice) },
            { GameLayer.ParallaxMid, new(res, GraphicsDevice) },
            { GameLayer.ParallaxFar, new(res, GraphicsDevice) },
            { GameLayer.ParallaxBg, new(res, GraphicsDevice) }
        };
    }

    /// <inheritdoc/>
    public override void Render(Rectangle canvasDestination, float canvasScale) {
        // draw different layers themselves to the screen
        GraphicsDevice.SetRenderTarget(null);
        GraphicsDevice.Clear(EngineSettings.RenderClearColor);
        SpriteBatch.Begin(samplerState: SamplerState.PointClamp);

        foreach (GameLayer layer in Enum.GetValues<GameLayer>()) {
            // do not draw debug layers if debugging is not enabled
            bool isDebugLayer = layer == GameLayer.WorldDebug || layer == GameLayer.UIDebug;
            if (isDebugLayer && !EngineSettings.ShowDebugDrawing) {
                continue;
            }

            Layers[layer].Draw(SpriteBatch, canvasDestination, canvasScale);
        }

        SpriteBatch.End();
    }

    /// <inheritdoc/>
    public override void RenderLoading() {
        if (loadingMenu != null) {
            Layers[GameLayer.UI].DrawTo(loadingMenu.Draw, SpriteBatch);

            if (EngineSettings.ShowDebugDrawing) {
                Layers[GameLayer.UIDebug].DrawTo(loadingMenu.DebugDraw, SpriteBatch);
            }
        }
    }

    /// <inheritdoc/>
    public override void ChangeResolution(int width, int height, int canvasExpandSize) {
        base.ChangeResolution(width, height, canvasExpandSize);
        loadingMenu?.ChangeResolution(width, height, canvasExpandSize);
    }
}
