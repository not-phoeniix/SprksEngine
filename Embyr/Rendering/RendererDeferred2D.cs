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
    private readonly Effect fxLightRender;

    // render targets/layers
    private RenderTarget2D depthBuffer;
    private RenderTarget2D lightBuffer;
    private RenderTarget2D distanceBackBuffer;
    private RenderTarget2D distanceFrontBuffer;
    private RenderTarget2D worldLayerDistanceField;
    private RenderTarget2D skyLayerDistanceField;

    private const int MaxLightsPerPass = 8;
    private Color globalLightTint;
    private readonly Vector3[] lightPositions = new Vector3[MaxLightsPerPass];
    private readonly Vector3[] lightColors = new Vector3[MaxLightsPerPass];
    private readonly float[] lightIntensities = new float[MaxLightsPerPass];
    private readonly Vector4[] lightSizeParams = new Vector4[MaxLightsPerPass];
    private readonly float[] lightRotations = new float[MaxLightsPerPass];

    public RendererDeferred2D(RendererSettings settings, GraphicsDevice gd, Menu? loadingMenu)
    : base(settings, gd, loadingMenu) {
        fxLightRender = ShaderManager.I.LoadShader("light_render");
        fxLightCombine = ShaderManager.I.LoadShader("light_combine");
        fxJumpFloodSeed = ShaderManager.I.LoadShader("jump_flood_seed");
        fxJumpFloodStep = ShaderManager.I.LoadShader("jump_flood_step");
        fxJumpFloodDistRender = ShaderManager.I.LoadShader("jump_flood_dist_render");

        RecreateRenderTargets(
            EngineSettings.GameCanvasResolution.X,
            EngineSettings.GameCanvasResolution.Y,
            Game.CanvasExpandSize
        );
    }

    /// <inheritdoc/>
    public override void RenderScene(Scene inputScene) {
        // don't render non 2D scenes!
        if (inputScene is not Scene2D scene) return;

        Matrix worldMatrix = scene.Camera.FlooredMatrix;

        // draw to buffers, render lighting
        SpriteBatch.GraphicsDevice.SetRenderTarget(depthBuffer);
        SpriteBatch.GraphicsDevice.Clear(Color.White);
        scene.DrawDepthmap(SpriteBatch);
        RenderDistanceField(worldLayerDistanceField, depthBuffer, 0.25f);
        RenderDistanceField(skyLayerDistanceField, depthBuffer, 1.0f);

        if (Settings.EnableLighting) {
            DrawLightsDeferred(scene, SpriteBatch);
        }

        // ~~~ draw all game layers to their respective RenderLayers ~~~

        fxLightCombine.Parameters["LightBuffer"].SetValue(lightBuffer);
        fxLightCombine.Parameters["VolumetricScalar"].SetValue(Settings.VolumetricScalar);
        fxLightCombine.Parameters["AmbientColor"].SetValue(scene.AmbientColor.ToVector3());
        fxLightCombine.Parameters["DistanceField"]?.SetValue(worldLayerDistanceField);
        Layers[GameLayer.World].SmoothingOffset = scene.Camera.Position;
        Layers[GameLayer.World].IndividualEffect = null;
        if (Settings.EnableLighting) {
            Layers[GameLayer.World].ScreenSpaceEffect = fxLightCombine;
        } else {
            Layers[GameLayer.World].ScreenSpaceEffect = null;
        }
        Layers[GameLayer.World].DrawTo(scene.Draw, SpriteBatch, worldMatrix);

        RenderPostProcessing(Layers[GameLayer.World]);

        Layers[GameLayer.WorldDebug].SmoothingOffset = scene.Camera.Position;
        Layers[GameLayer.WorldDebug].DrawTo(scene.DebugDraw, SpriteBatch, worldMatrix);

        Layers[GameLayer.UI].DrawTo(scene.DrawOverlays, SpriteBatch);
        Layers[GameLayer.UIDebug].DrawTo(scene.DebugDrawOverlays, SpriteBatch);

        void DrawParallax(GameLayer gameLayer, ParallaxBackground? bg) {
            ParallaxLayer? layer = bg?.GetLayer(gameLayer);
            if (layer == null) return;  // don't draw if layer doesn't exist
            Layers[gameLayer].SmoothingOffset = layer.WorldLocation;
            Layers[gameLayer].ColorTint = globalLightTint;
            Layers[gameLayer].DrawTo(layer.Draw, SpriteBatch, worldMatrix);
        }

        ParallaxBackground? parallax = scene.GetCurrentParallax();
        DrawParallax(GameLayer.ParallaxBg, parallax);
        DrawParallax(GameLayer.ParallaxFar, parallax);
        DrawParallax(GameLayer.ParallaxMid, parallax);
        DrawParallax(GameLayer.ParallaxNear, parallax);
    }

    /// <inheritdoc/>
    public override void ChangeResolution(int width, int height, int canvasExpandSize) {
        base.ChangeResolution(width, height, canvasExpandSize);

        RecreateRenderTargets(width, height, canvasExpandSize);

        foreach (RenderLayer layer in Layers.Values) {
            layer.ChangeResolution(width, height, canvasExpandSize);
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

    /// <summary>
    /// Draws all lights in the scene to a render target
    /// </summary>
    /// <param name="scene">Scene to grab and draw lights from<param>
    /// <param name="sb">SpriteBatch to draw with</param>
    private void DrawLightsDeferred(Scene2D scene, SpriteBatch sb) {
        sb.GraphicsDevice.SetRenderTarget(lightBuffer);
        sb.GraphicsDevice.Clear(Color.Black);

        Vector3 globalSum = Vector3.Zero;
        int i = 0;

        void SaveLightInArr(Light2D light) {
            // set array values
            Vector2 lightScreenPos = Vector2.Transform(light.Transform.GlobalPosition, scene.Camera.FlooredMatrix);
            lightScreenPos /= new Vector2(lightBuffer.Width, lightBuffer.Height);
            lightPositions[i] = new Vector3(
                lightScreenPos,
                light.IsGlobal ? 1 : 0
            );
            lightColors[i] = light.Color.ToVector3();
            lightIntensities[i] = light.Intensity;
            lightRotations[i] = light.Transform.Rotation;
            lightSizeParams[i] = new Vector4(
                light.Radius,
                light.AngularWidth,
                light.LinearFalloff,
                light.AngularFalloff
            );

            i++;
        }

        void Draw() {
            // ~~~ PASS DATA ~~~

            // "i" at this point should equal the total light
            //   count since it was incremented before we got here
            fxLightRender.Parameters["NumLights"].SetValue(i);
            fxLightRender.Parameters["ScreenRes"].SetValue(new Vector2(lightBuffer.Width, lightBuffer.Height));
            fxLightRender.Parameters["Positions"].SetValue(lightPositions);
            fxLightRender.Parameters["Colors"].SetValue(lightColors);
            fxLightRender.Parameters["Intensities"].SetValue(lightIntensities);
            fxLightRender.Parameters["Rotations"].SetValue(lightRotations);
            fxLightRender.Parameters["SizeParams"].SetValue(lightSizeParams);
            fxLightRender.Parameters["SkyDistanceField"].SetValue(skyLayerDistanceField);

            // ~~~ DRAW TO BUFFER ~~~
            sb.Begin(samplerState: SamplerState.PointClamp, effect: fxLightRender);
            // lights draw on top of distance field, easier
            //   than passing in a texture via parameters
            sb.Draw(worldLayerDistanceField, new Rectangle(0, 0, lightBuffer.Width, lightBuffer.Height), Color.White);
            sb.End();
        }

        foreach (Light2D light in scene.GetAllLightsToRender()) {
            if (light.Enabled) {
                SaveLightInArr(light);

                if (light.IsGlobal) {
                    globalSum += light.Color.ToVector3() * light.Intensity;
                }
            }

            // if max lights has been reached (or end of lights
            //   list has been reached), draw to the deferred buffer!
            if (i > 0 && i % MaxLightsPerPass == 0) {
                Draw();
                i = 0;
            }
        }

        // draw one final time to make sure everything is
        //   rendered only if i has not yet been reset
        if (i != 0) {
            Draw();
        }

        globalLightTint = new Color(globalSum);
    }
}
