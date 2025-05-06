using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Embyr.Rendering;

public abstract class Renderer2D : Renderer {
    public readonly SpriteBatch SpriteBatch;
    protected readonly Dictionary<GameLayer, RenderLayer> Layers;

    public Renderer2D(RendererSettings settings, GraphicsDevice gd) : base(settings, gd) {
        SpriteBatch = new SpriteBatch(gd);

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
}
