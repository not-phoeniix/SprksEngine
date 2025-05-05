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
    private readonly Effect fxBloomThreshold;
    private readonly Effect fxBloomBlur;
    private readonly Effect fxBloomCombine;

    // render targets/layers
    private RenderTarget2D depthBuffer;
    private RenderTarget2D lightBuffer;
    private RenderTarget2D bloomBackBuffer;
    private RenderTarget2D bloomFrontBuffer;
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
        fxBloomThreshold = ShaderManager.I.LoadShader("bloom_threshold", ShaderManager.ShaderProfile.OpenGL);
        fxBloomBlur = ShaderManager.I.LoadShader("bloom_blur", ShaderManager.ShaderProfile.OpenGL);
        fxBloomCombine = ShaderManager.I.LoadShader("bloom_combine", ShaderManager.ShaderProfile.OpenGL);

        RecreateRenderTargets(
            EngineSettings.GameCanvasResolution.X,
            EngineSettings.GameCanvasResolution.Y,
            Game.CanvasExpandSize
        );
    }

    public override void RenderScene(Scene scene) {
        if (!SceneManager.I.IsLoading) {
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

            // Vector3 sunColorVec3 = currentScene.Sun.Color.ToVector3() * currentScene.Sun.Intensity;

            fxLightCombine.Parameters["LightBuffer"].SetValue(lightBuffer);
            fxLightCombine.Parameters["VolumetricScalar"].SetValue(currentScene.VolumetricScalar);
            fxLightCombine.Parameters["AmbientColor"].SetValue(currentScene.AmbientColor.ToVector3());
            fxLightCombine.Parameters["DistanceField"]?.SetValue(worldLayerDistanceField);
            Layers[GameLayer.World].SmoothingOffset = camera.Position;
            Layers[GameLayer.World].IndividualEffect = null;
            Layers[GameLayer.World].ScreenSpaceEffect = fxLightCombine;
            Layers[GameLayer.World].DrawTo(currentScene.Draw, SpriteBatch, worldMatrix);

            // render bloom effect after world has been rendered
            RenderBloom(0.98f, 4);

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

        } else {
        }
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
        ResizeBuffer(ref bloomBackBuffer);
        ResizeBuffer(ref bloomFrontBuffer);
        ResizeBuffer(ref distanceBackBuffer);
        ResizeBuffer(ref distanceFrontBuffer);
        ResizeBuffer(ref worldLayerDistanceField);
        ResizeBuffer(ref skyLayerDistanceField);
    }

    public override void ChangeResolution(int width, int height, int canvasExpandSize) {
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

    private void RenderBloom(float bloomThreshold, int blurPasses) {
        // pass data to shaders before drawing
        Vector2 screenRes = (EngineSettings.GameCanvasResolution + new Point(Game.CanvasExpandSize)).ToVector2();
        fxBloomThreshold.Parameters["Threshold"].SetValue(bloomThreshold);
        fxBloomBlur.Parameters["ScreenRes"].SetValue(screenRes);

        // draw threshold pass
        SpriteBatch.GraphicsDevice.SetRenderTarget(bloomBackBuffer);
        SpriteBatch.GraphicsDevice.Clear(Color.Black);
        SpriteBatch.Begin(effect: fxBloomThreshold);
        Layers[GameLayer.World].Draw(SpriteBatch, Vector2.Zero);
        SpriteBatch.End();

        bool drawToFrontBuffer = true;

        do {
            RenderTarget2D target;
            RenderTarget2D toDraw;
            if (drawToFrontBuffer) {
                target = bloomFrontBuffer;
                toDraw = bloomBackBuffer;
            } else {
                target = bloomBackBuffer;
                toDraw = bloomFrontBuffer;
            }

            // draw blurring pass
            SpriteBatch.GraphicsDevice.SetRenderTarget(target);
            SpriteBatch.GraphicsDevice.Clear(Color.Black);
            SpriteBatch.Begin(effect: fxBloomBlur);
            SpriteBatch.Draw(toDraw, Vector2.Zero, Color.White);
            SpriteBatch.End();

            drawToFrontBuffer = !drawToFrontBuffer;
            blurPasses--;
        } while (blurPasses > 0);

        RenderTarget2D finalBuffer;
        if (drawToFrontBuffer) {
            finalBuffer = bloomBackBuffer;
        } else {
            finalBuffer = bloomFrontBuffer;
        }

        // combine dry and wet effects (heh music reference)
        fxBloomCombine.Parameters["BloomTexture"].SetValue(finalBuffer);
        Layers[GameLayer.World].ScreenSpaceEffect = fxBloomCombine;
        Layers[GameLayer.World].DrawTo(
            () => Layers[GameLayer.World].Draw(SpriteBatch, Vector2.Zero),
            SpriteBatch
        );
    }

}
