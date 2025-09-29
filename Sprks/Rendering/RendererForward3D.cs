using Sprks.Scenes;
using Sprks.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sprks.Rendering;

/// <summary>
/// Classic forward 3D renderer, inherits from <c>Renderer</c>
/// </summary>
internal class RendererForward3D : Renderer {
    private readonly Effect forward3D;

    private const int MaxLightsPerPass = 32;
    private readonly Vector4[] lightPositions = new Vector4[MaxLightsPerPass];
    private readonly Vector3[] lightColors = new Vector3[MaxLightsPerPass];
    private readonly Vector3[] lightDirections = new Vector3[MaxLightsPerPass];
    private readonly float[] lightIntensities = new float[MaxLightsPerPass];
    private readonly Vector4[] lightSizeParams = new Vector4[MaxLightsPerPass];

    /// <summary>
    /// Creates a new RendererForward3D instance
    /// </summary>
    /// <param name="settings">Renderer settings object to use when rendering</param>
    /// <param name="gd">GraphicsDevice to create renderer with</param>
    public RendererForward3D(RendererSettings settings, GraphicsDevice gd)
    : base(settings, gd) {
        forward3D = ShaderManager.LoadShader("3d_forward");
    }

    /// <inheritdoc/>
    public override void RenderScene(Scene inputScene) {
        // don't render non-3D scenes!
        if (inputScene is not Scene3D scene) return;

        // set up param arrays for lights in scene
        int i = 0;
        foreach (Light3D light in scene.GetAllLightsToRender()) {
            // don't render lighting
            if (!Settings.EnableLighting) break;

            // exit early if max lights have been reached
            if (i >= MaxLightsPerPass) break;

            if (light.Enabled) {
                lightPositions[i] = new Vector4(
                    light.Transform.GlobalPosition,
                    light.IsGlobal ? 1 : 0
                );
                lightColors[i] = light.Color.ToVector3();
                lightDirections[i] = light.Transform.Forward;
                lightIntensities[i] = light.Intensity;
                lightSizeParams[i] = new Vector4(
                    light.Range,
                    light.SpotInnerAngle,
                    light.SpotOuterAngle,
                    0
                );

                i++;
            }
        }

        // pass shader params
        forward3D.Parameters["AmbientColor"].SetValue(inputScene.AmbientColor.ToVector3());
        forward3D.Parameters["Positions"].SetValue(lightPositions);
        forward3D.Parameters["Colors"].SetValue(lightColors);
        forward3D.Parameters["Intensities"].SetValue(lightIntensities);
        forward3D.Parameters["Directions"].SetValue(lightDirections);
        forward3D.Parameters["SizeParams"].SetValue(lightSizeParams);
        forward3D.Parameters["NumLights"].SetValue(i);

        // render scene itself to main layer
        GraphicsDevice.SetRenderTarget(SceneRenderLayer.RenderTarget);
        GraphicsDevice.Clear(Color.Transparent);
        foreach (IActor3D actor in scene.GetDrawableActors()) {
            actor.Draw(scene.Camera);
        }

        // render post processing back onto the main layer
        RenderPostProcessing(SceneRenderLayer);

        // render debug info for the scene
        foreach (IActor3D actor in scene.GetActorsInViewport(scene.Camera.ViewBounds)) {
            if (!EngineSettings.ShowDebugDrawing) break;

            if (actor is IDebugDrawable3D debug) {
                debug.DebugDraw(scene.Camera);
            }
        }
    }
}
