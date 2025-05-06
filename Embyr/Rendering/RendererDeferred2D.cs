using Embyr.Scenes;
using Embyr.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Embyr.Rendering;

public class RendererDeferred2D : Renderer2D {
    // shaders
    private readonly Effect fxLightCombine;
    private readonly Effect fxJumpFloodSeed;
    private readonly Effect fxJumpFloodStep;
    private readonly Effect fxJumpFloodDistRender;

    // render targets/layers
    private RenderTarget2D depthBuffer;
    private RenderTarget2D lightBuffer;
    private RenderTarget2D distanceBackBuffer;
    private RenderTarget2D distanceFrontBuffer;
    private RenderTarget2D worldLayerDistanceField;
    private RenderTarget2D skyLayerDistanceField;

    // other misc things!
    private readonly Menu? loadingMenu;

    public RendererDeferred2D(GraphicsDevice gd, Menu? loadingMenu) : base(gd) {
        this.loadingMenu = loadingMenu;

        fxLightCombine = ShaderManager.I.LoadShader("light_combine", ShaderManager.ShaderProfile.OpenGL);
        fxJumpFloodSeed = ShaderManager.I.LoadShader("jump_flood_seed", ShaderManager.ShaderProfile.OpenGL);
        fxJumpFloodStep = ShaderManager.I.LoadShader("jump_flood_step", ShaderManager.ShaderProfile.OpenGL);
        fxJumpFloodDistRender = ShaderManager.I.LoadShader("jump_flood_dist_render", ShaderManager.ShaderProfile.OpenGL);

        RecreateRenderTargets(
            EngineSettings.GameCanvasResolution.X,
            EngineSettings.GameCanvasResolution.Y,
            Game.CanvasExpandSize
        );
    }

    public override void RenderScene(Scene scene) {
        Scene currentScene = SceneManager.I.CurrentScene;

        Camera camera = SceneManager.I.Camera;
        Matrix worldMatrix = camera.FlooredMatrix;

        // draw to buffers, render lighting
        SpriteBatch.GraphicsDevice.SetRenderTarget(depthBuffer);
        SpriteBatch.GraphicsDevice.Clear(Color.White);
        currentScene.DrawDepthmap(SpriteBatch);
        RenderDistanceField(worldLayerDistanceField, depthBuffer, 0.25f);
        RenderDistanceField(skyLayerDistanceField, depthBuffer, 1.0f);
        currentScene.DrawLightsDeferred(SpriteBatch, lightBuffer, worldLayerDistanceField, skyLayerDistanceField);

        // ~~~ draw all game layers to their respective RenderLayers ~~~

        fxLightCombine.Parameters["LightBuffer"].SetValue(lightBuffer);
        fxLightCombine.Parameters["VolumetricScalar"].SetValue(currentScene.VolumetricScalar);
        fxLightCombine.Parameters["AmbientColor"].SetValue(currentScene.AmbientColor.ToVector3());
        fxLightCombine.Parameters["DistanceField"]?.SetValue(worldLayerDistanceField);
        Layers[GameLayer.World].SmoothingOffset = camera.Position;
        Layers[GameLayer.World].IndividualEffect = null;
        Layers[GameLayer.World].ScreenSpaceEffect = fxLightCombine;
        Layers[GameLayer.World].DrawTo(currentScene.Draw, SpriteBatch, worldMatrix);

        // render all post processing effects !!
        RenderTarget2D prevTarget = Layers[GameLayer.World].RenderTarget;
        for (int i = 0; i < PostProcessingEffects.Count; i++) {
            // grab reference to iteration effect, skip if disabled
            PostProcessingEffect fx = PostProcessingEffects[i];
            if (!fx.Enabled) continue;

            fx.InputRenderTarget = prevTarget;
            fx.Draw(SpriteBatch);

            prevTarget = fx.FinalRenderTarget;
        }

        // draw final post process layer BACK to world layer
        //   (if any post processes were used in the first place)
        if (prevTarget != Layers[GameLayer.World].RenderTarget) {
            Layers[GameLayer.World].ScreenSpaceEffect = null;
            Layers[GameLayer.World].DrawTo(
                () => SpriteBatch.Draw(prevTarget, Vector2.Zero, Color.White),
                SpriteBatch
            );
        }

        Layers[GameLayer.WorldDebug].SmoothingOffset = camera.Position;
        Layers[GameLayer.WorldDebug].DrawTo(currentScene.DebugDraw, SpriteBatch, worldMatrix);

        Layers[GameLayer.UI].DrawTo(currentScene.DrawOverlays, SpriteBatch);
        Layers[GameLayer.UIDebug].DrawTo(currentScene.DebugDrawOverlays, SpriteBatch);

        void DrawParallax(GameLayer gameLayer, ParallaxBackground bg) {
            ParallaxLayer? layer = bg?.GetLayer(gameLayer);
            if (layer == null) return;  // don't draw if layer doesn't exist
            Layers[gameLayer].SmoothingOffset = layer.WorldLocation;
            Layers[gameLayer].ColorTint = currentScene.GlobalLightTint;
            Layers[gameLayer].DrawTo(layer.Draw, SpriteBatch, worldMatrix);
        }

        ParallaxBackground parallax = currentScene.GetCurrentParallax();
        DrawParallax(GameLayer.ParallaxBg, parallax);
        DrawParallax(GameLayer.ParallaxFar, parallax);
        DrawParallax(GameLayer.ParallaxMid, parallax);
        DrawParallax(GameLayer.ParallaxNear, parallax);
    }

    public override void RenderLoading() {
        if (loadingMenu != null) {
            Layers[GameLayer.UI].DrawTo(loadingMenu.Draw, SpriteBatch);

            if (EngineSettings.ShowDebugDrawing) {
                Layers[GameLayer.UIDebug].DrawTo(loadingMenu.DebugDraw, SpriteBatch);
            }
        }
    }

    private void RecreateRenderTargets(int width, int height, int canvasExpandSize) {
        void ResizeBuffer(ref RenderTarget2D buffer) {
            buffer?.Dispose();
            buffer = new RenderTarget2D(
                GraphicsDevice,
                width + canvasExpandSize,
                height + canvasExpandSize
            );
        }

        ResizeBuffer(ref lightBuffer);
        ResizeBuffer(ref depthBuffer);
        ResizeBuffer(ref distanceBackBuffer);
        ResizeBuffer(ref distanceFrontBuffer);
        ResizeBuffer(ref worldLayerDistanceField);
        ResizeBuffer(ref skyLayerDistanceField);
    }

    public override void ChangeResolution(int width, int height, int canvasExpandSize) {
        base.ChangeResolution(width, height, canvasExpandSize);

        RecreateRenderTargets(width, height, canvasExpandSize);

        foreach (RenderLayer layer in Layers.Values) {
            layer.ChangeResolution(width, height, canvasExpandSize);
        }

        loadingMenu?.ChangeResolution(width, height, canvasExpandSize);
    }

    /// <summary>
    /// Renders a distance field to a render target based on a defined target depth
    /// </summary>
    /// <param name="destination">Final destination target for distance field</param>
    /// <param name="depthBuffer">Depth buffer of world</param>
    /// <param name="targetDepth">Target depth to generate distance from in buffer</param>
    private void RenderDistanceField(RenderTarget2D destination, RenderTarget2D depthBuffer, float targetDepth) {
        // https://blog.demofox.org/2016/02/29/fast-voronoi-diagrams-and-distance-dield-textures-on-the-gpu-with-the-jump-flooding-algorithm/

        // render depth buffer initially as seed
        fxJumpFloodSeed.Parameters["TargetDepth"].SetValue(targetDepth);
        SpriteBatch.GraphicsDevice.SetRenderTarget(distanceBackBuffer);
        SpriteBatch.GraphicsDevice.Clear(Color.Black);
        SpriteBatch.Begin(effect: fxJumpFloodSeed);
        SpriteBatch.Draw(depthBuffer, new Rectangle(0, 0, distanceBackBuffer.Width, distanceBackBuffer.Height), Color.White);
        SpriteBatch.End();

        bool drawToFrontBuffer = true;

        void Step(int offset) {
            RenderTarget2D target;
            RenderTarget2D toDraw;

            if (drawToFrontBuffer) {
                target = distanceFrontBuffer;
                toDraw = distanceBackBuffer;
            } else {
                target = distanceBackBuffer;
                toDraw = distanceFrontBuffer;
            }

            SpriteBatch.GraphicsDevice.SetRenderTarget(target);
            SpriteBatch.GraphicsDevice.Clear(Color.Black);
            fxJumpFloodStep.Parameters["Offset"].SetValue((float)offset);
            fxJumpFloodStep.Parameters["ScreenRes"].SetValue(new Vector2(toDraw.Width, toDraw.Height));
            SpriteBatch.Begin(effect: fxJumpFloodStep);
            // spriteBatch.Draw(toDraw, Vector2.Zero, Color.White);
            SpriteBatch.Draw(toDraw, new Rectangle(0, 0, target.Width, target.Height), Color.White);
            SpriteBatch.End();

            drawToFrontBuffer = !drawToFrontBuffer;
        }

        // offest should be: 2 ^ (ceil(log2(N)) – passIndex – 1),
        int N = Math.Max(worldLayerDistanceField.Width / 2, worldLayerDistanceField.Height / 2);
        int offset = 100;
        int i = 0;
        while (offset > 0) {
            offset = (int)MathF.Pow(2, MathF.Ceiling(MathF.Log2(N)) - i - 1);
            Step(offset);
            i++;
        }

        // https://en.wikipedia.org/wiki/Jump_flooding_algorithm
        //   JFA+1 can possibly increase accuracy!
        Step(1);

        // get the final buffer that was just drawn to
        RenderTarget2D finalTarget;
        if (drawToFrontBuffer) {
            finalTarget = distanceBackBuffer;
        } else {
            finalTarget = distanceFrontBuffer;
        }

        // draw buffer to actual distance buffer using distance render shader
        SpriteBatch.GraphicsDevice.SetRenderTarget(destination);
        SpriteBatch.GraphicsDevice.Clear(Color.Black);
        SpriteBatch.Begin(effect: fxJumpFloodDistRender);
        // spriteBatch.Draw(finalTarget, Vector2.Zero, Color.White);
        SpriteBatch.Draw(finalTarget, new Rectangle(0, 0, destination.Width, destination.Height), Color.White);
        SpriteBatch.End();
    }
}
