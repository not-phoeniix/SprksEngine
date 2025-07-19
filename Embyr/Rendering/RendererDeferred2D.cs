using Embyr.Scenes;
using Embyr.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Embyr.Rendering;

/// <summary>
/// Deferred shading 2D renderer, inherits from <c>Renderer</c>
/// </summary>
internal class RendererDeferred2D : Renderer {
    // shaders
    private readonly Effect fxRenderGBuffer;
    private readonly Effect fxRenderGBufferClear;
    private readonly Effect fxLightCombine;
    private readonly Effect fxJumpFloodSeed;
    private readonly Effect fxJumpFloodStep;
    private readonly Effect fxJumpFloodDistRender;
    private readonly Effect fxLightRender;

    // render targets/layers
    private readonly RenderTargetBinding[] sceneMRTTargets;
    private RenderTarget2D albedoBuffer;
    private RenderTarget2D normalBuffer;
    private RenderTarget2D depthBuffer;
    private RenderTarget2D lightBuffer;
    private RenderTarget2D distanceBackBuffer;
    private RenderTarget2D distanceFrontBuffer;
    private RenderTarget2D obstructorDistanceField;

    private const int MaxLightsPerPass = 8;
    private Color globalLightTint;
    private readonly Vector4[] lightPositions = new Vector4[MaxLightsPerPass];
    private readonly Vector3[] lightColors = new Vector3[MaxLightsPerPass];
    private readonly float[] lightIntensities = new float[MaxLightsPerPass];
    private readonly Vector4[] lightSizeParams = new Vector4[MaxLightsPerPass];
    private readonly float[] lightRotations = new float[MaxLightsPerPass];
    private readonly float[] lightCastsShadow = new float[MaxLightsPerPass];

    /// <summary>
    /// Creates a new RendererDeferred2D instance
    /// </summary>
    /// <param name="settings">Renderer settings object to use when rendering</param>
    /// <param name="gd">GraphicsDevice to create renderer with</param>
    /// <param name="loadingMenu">Optional loading menu to draw when a scene is loading</param>
    public RendererDeferred2D(RendererSettings settings, GraphicsDevice gd, Menu? loadingMenu)
    : base(settings, gd, loadingMenu) {
        fxLightRender = ShaderManager.I.LoadShader("Deferred2D/light_render");
        fxLightCombine = ShaderManager.I.LoadShader("Deferred2D/light_combine");
        fxJumpFloodSeed = ShaderManager.I.LoadShader("Deferred2D/jump_flood_seed");
        fxJumpFloodStep = ShaderManager.I.LoadShader("Deferred2D/jump_flood_step");
        fxJumpFloodDistRender = ShaderManager.I.LoadShader("Deferred2D/jump_flood_dist_render");
        fxRenderGBuffer = ShaderManager.I.LoadShader("Deferred2D/gbuffer_render");
        fxRenderGBufferClear = ShaderManager.I.LoadShader("Deferred2D/gbuffer_clear");

        sceneMRTTargets = new RenderTargetBinding[3];

        RecreateRenderTargets(
            EngineSettings.GameCanvasResolution.X,
            EngineSettings.GameCanvasResolution.Y
        );
    }

    /// <inheritdoc/>
    public override void RenderScene(Scene inputScene) {
        // don't render non 2D scenes!
        if (inputScene is not Scene2D scene) return;

        Matrix worldMatrix = scene.Camera.FlooredMatrix;

        SpriteBatch.GraphicsDevice.SetRenderTargets(sceneMRTTargets);

        // use gbuffer clear shader to set default values and
        //   draw a single rectangle to clear the buffers
        fxRenderGBufferClear.Parameters["AlbedoClearColor"].SetValue(new Vector4(0.0f));
        fxRenderGBufferClear.Parameters["NormalDepthClearColor"].SetValue(new Vector4(0.5f, 0.5f, 1.0f, 1.0f));
        fxRenderGBufferClear.Parameters["ObstructorsClearColor"].SetValue(new Vector4(1.0f));
        SpriteBatch.Begin(effect: fxRenderGBufferClear);
        SpriteBatch.DrawRectFill(new Rectangle(0, 0, albedoBuffer.Width, albedoBuffer.Height), Color.Black);
        SpriteBatch.End();

        ShaderManager.I.CurrentActorEffect = fxRenderGBuffer;
        fxRenderGBuffer.Parameters["Gamma"].SetValue(Settings.Gamma);
        SpriteBatchBegin(scene);

        foreach (Actor2D actor in scene.GetDrawableActors()) {
            actor.Draw(SpriteBatch);
        }

        SpriteBatch.End();
        ShaderManager.I.CurrentActorEffect = null;

        if (Settings.EnableLighting) {
            RenderDistanceField(obstructorDistanceField, 0.0f);
            RenderLightsDeferred(scene, SpriteBatch);
        }

        // draw buffers into the actual render layer
        SceneRenderLayer.SmoothingOffset = scene.Camera.Position;
        if (EngineSettings.ShowDebugNormalBuffer) {
            SceneRenderLayer.ScreenSpaceEffect = null;
            SceneRenderLayer.DrawTo(sb => sb.Draw(normalBuffer, Vector2.Zero, Color.White), SpriteBatch, null);
        } else if (EngineSettings.ShowDebugDepthBuffer) {
            SceneRenderLayer.ScreenSpaceEffect = null;
            SceneRenderLayer.DrawTo(sb => sb.Draw(depthBuffer, Vector2.Zero, Color.White), SpriteBatch, null);
        } else {
            if (Settings.EnableLighting) {
                fxLightCombine.Parameters["LightBuffer"].SetValue(lightBuffer);
                fxLightCombine.Parameters["VolumetricScalar"].SetValue(Settings.VolumetricScalar);
                fxLightCombine.Parameters["AmbientColor"].SetValue(scene.AmbientColor.ToVector3());
                SceneRenderLayer.ScreenSpaceEffect = fxLightCombine;
            } else {
                SceneRenderLayer.ScreenSpaceEffect = null;
            }

            SceneRenderLayer.DrawTo(sb => sb.Draw(albedoBuffer, Vector2.Zero, Color.White), SpriteBatch, null);
            RenderPostProcessing(SceneRenderLayer);
        }

        if (EngineSettings.ShowDebugDrawing) {
            SceneRenderLayer.ScreenSpaceEffect = null;
            SceneRenderLayer.IndividualEffect = null;
            SceneRenderLayer.DrawTo(
                sb => {
                    foreach (Actor2D actor in scene.GetDrawableActors()) {
                        actor.DebugDraw(sb);
                    }
                },
                SpriteBatch,
                worldMatrix,
                resetTarget: false
            );
        }

        // draw UI to its respective render layer
        // UIRenderLayer.DrawTo(scene.DrawOverlays, SpriteBatch);
        // if (EngineSettings.ShowDebugDrawing) {
        //     UIRenderLayer.DrawTo(scene.DebugDrawOverlays, SpriteBatch, resetTarget: false);
        // }
        UIRenderLayer.DrawTo(UIBuilder.DrawAll, SpriteBatch);

        // TODO: somehow get dynamic parallax drawing on different RenderLayer's

        // void DrawParallax(GameLayer gameLayer, ParallaxBackground? bg) {
        //     ParallaxLayer? layer = bg?.GetLayer(gameLayer);
        //     if (layer == null) return;  // don't draw if layer doesn't exist
        //     Layers[gameLayer].SmoothingOffset = layer.WorldLocation;
        //     Layers[gameLayer].ColorTint = globalLightTint;
        //     Layers[gameLayer].DrawTo(layer.Draw, SpriteBatch, worldMatrix);
        // }

        // ParallaxBackground? parallax = scene.GetCurrentParallax();
        // DrawParallax(GameLayer.ParallaxBg, parallax);
        // DrawParallax(GameLayer.ParallaxFar, parallax);
        // DrawParallax(GameLayer.ParallaxMid, parallax);
        // DrawParallax(GameLayer.ParallaxNear, parallax);
    }

    /// <inheritdoc/>
    public override void ChangeResolution(int width, int height) {
        base.ChangeResolution(width, height);
        RecreateRenderTargets(width, height);
    }

    /// <summary>
    /// Restarts sprite batch, flushing buffer and drawing to the screen
    /// </summary>
    /// <param name="scene">Scene to continue rendering from</param>
    public void RestartSpriteBatch(Scene2D scene) {
        SpriteBatch.End();
        SpriteBatchBegin(scene);
    }

    private void SpriteBatchBegin(Scene2D scene) {
        Matrix worldMatrix = scene.Camera.FlooredMatrix;
        SpriteBatch.Begin(
            SpriteSortMode.Deferred,
            BlendState.AlphaBlend,
            SamplerState.PointClamp,
            null,
            null,
            fxRenderGBuffer,
            worldMatrix
        );
    }

    private void RecreateRenderTargets(int width, int height) {
        void ResizeBuffer(ref RenderTarget2D buffer, SurfaceFormat format) {
            buffer?.Dispose();
            buffer = new RenderTarget2D(
                GraphicsDevice,
                width,
                height,
                false,
                format,
                DepthFormat.None
            );
        }

        // light buffer needs to be 64-bit HDR for super bright lights lol
        ResizeBuffer(ref lightBuffer, SurfaceFormat.HalfVector4);

        // everything else can be regular 32-bit color
        ResizeBuffer(ref albedoBuffer, SurfaceFormat.Color);
        ResizeBuffer(ref normalBuffer, SurfaceFormat.Color);
        ResizeBuffer(ref depthBuffer, SurfaceFormat.Color);
        ResizeBuffer(ref distanceBackBuffer, SurfaceFormat.Color);
        ResizeBuffer(ref distanceFrontBuffer, SurfaceFormat.Color);
        ResizeBuffer(ref obstructorDistanceField, SurfaceFormat.Color);

        sceneMRTTargets[0] = new RenderTargetBinding(albedoBuffer);
        sceneMRTTargets[1] = new RenderTargetBinding(normalBuffer);
        sceneMRTTargets[2] = new RenderTargetBinding(depthBuffer);
    }

    /// <summary>
    /// Renders a distance field to a render target based on a defined target depth
    /// </summary>
    /// <param name="destination">Final destination target for distance field</param>
    /// <param name="targetDepth">Target depth to generate distance from in buffer</param>
    private void RenderDistanceField(RenderTarget2D destination, float targetDepth) {
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
            SpriteBatch.Draw(toDraw, new Rectangle(0, 0, target.Width, target.Height), Color.White);
            SpriteBatch.End();

            drawToFrontBuffer = !drawToFrontBuffer;
        }

        // https://en.wikipedia.org/wiki/Jump_flooding_algorithm#Variants
        //   1+JFA can increase accuracy!
        Step(1);

        // offest should be: 2 ^ (ceil(log2(N)) – passIndex – 1),
        int N = Math.Max(destination.Width / 2, destination.Height / 2);
        int offset;
        int i = 0;
        do {
            offset = (int)MathF.Pow(2, MathF.Ceiling(MathF.Log2(N)) - i - 1);
            if (offset > 0) {
                Step(offset);
            }
            i++;
        } while (offset > 0);

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
        fxJumpFloodDistRender.Parameters["DepthBuffer"].SetValue(depthBuffer);
        fxJumpFloodDistRender.Parameters["TargetDepth"].SetValue(targetDepth);
        SpriteBatch.Begin(effect: fxJumpFloodDistRender);
        SpriteBatch.Draw(finalTarget, new Rectangle(0, 0, destination.Width, destination.Height), Color.White);
        SpriteBatch.End();
    }

    /// <summary>
    /// Draws all lights in the scene to a render target
    /// </summary>
    /// <param name="scene">Scene to grab and draw lights from<param>
    /// <param name="sb">SpriteBatch to draw with</param>
    private void RenderLightsDeferred(Scene2D scene, SpriteBatch sb) {
        sb.GraphicsDevice.SetRenderTarget(lightBuffer);
        sb.GraphicsDevice.Clear(Color.Black);

        Vector3 globalSum = Vector3.Zero;
        int i = 0;

        void SaveLightInArr(Light2D light) {
            // set array values
            Vector2 lightScreenPos = Vector2.Transform(light.Transform.GlobalPosition, scene.Camera.FlooredMatrix);
            lightScreenPos /= new Vector2(lightBuffer.Width, lightBuffer.Height);
            lightPositions[i] = new Vector4(
                lightScreenPos.X,
                lightScreenPos.Y,
                light.Transform.GlobalZIndex,
                light.IsGlobal ? 1 : 0
            );
            lightColors[i] = light.Color.ToVector3();
            lightIntensities[i] = light.Intensity;
            lightRotations[i] = light.Transform.Rotation;
            lightSizeParams[i] = new Vector4(
                light.Radius,
                light.LinearFalloff,
                light.InnerAngle,
                light.OuterAngle
            );
            lightCastsShadow[i] = light.CastsShadow ? 1 : 0;

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
            fxLightRender.Parameters["CastsShadow"].SetValue(lightCastsShadow);
            fxLightRender.Parameters["NormalMap"].SetValue(normalBuffer);
            fxLightRender.Parameters["DepthBuffer"].SetValue(depthBuffer);
            fxLightRender.Parameters["Depth3DScalar"].SetValue(Settings.Depth3DScalar);

            // ~~~ DRAW TO BUFFER ~~~
            sb.Begin(samplerState: SamplerState.PointClamp, effect: fxLightRender);
            // lights draw on top of distance field, easier
            //   than passing in a texture via parameters
            sb.Draw(obstructorDistanceField, new Rectangle(0, 0, lightBuffer.Width, lightBuffer.Height), Color.White);
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
