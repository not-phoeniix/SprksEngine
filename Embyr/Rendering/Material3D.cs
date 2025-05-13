using Embyr.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Embyr.Rendering;

/// <summary>
/// A 3D Material to apply to a GameMesh when rendering
/// </summary>
public class Material3D {
    /// <summary>
    /// Static default diffuse shader for all materials
    /// </summary>
    public static readonly Effect DefaultDiffuseShader = ShaderManager.I.LoadShader("3d_forward");

    /// <summary>
    /// Static default shader used for unlit debug drawing
    /// </summary>
    public static readonly Effect DefaultSingleColorShader = ShaderManager.I.LoadShader("3d_single_color");

    private float roughness = 0.0f;

    /// <summary>
    /// Shader associated with this material
    /// </summary>
    public Effect Shader { get; }

    /// <summary>
    /// Gets/sets color of the material's surface
    /// </summary>
    public Color SurfaceColor { get; set; }

    /// <summary>
    /// Gets/sets the roughness of this material, clamped to be between 0.0f and 1.0f
    /// </summary>
    public float Roughness {
        get => roughness;
        set => roughness = Math.Clamp(value, 0, 1);
    }

    /// <summary>
    /// Creates a new Material3D object
    /// </summary>
    /// <param name="shader">Optional shader to use for rendering this material, defaults to DefaultDiffuseShader</param>
    public Material3D(Effect? shader = null) {
        this.Shader = shader ?? DefaultDiffuseShader;
        SurfaceColor = Color.White;
    }

    /// <summary>
    /// Applies this material to the graphics devices and draws a model mesh part to the set target
    /// </summary>
    /// <param name="part">Part to draw</param>
    /// <param name="primitiveType">Primitive type to use when rendering mesh part</param>
    internal void ApplyAndDraw(ModelMeshPart part, PrimitiveType primitiveType) {
        Shader.Parameters["SurfaceColor"]?.SetValue(SurfaceColor.ToVector3());
        Shader.Parameters["Gamma"]?.SetValue(EngineSettings.Gamma);
        Shader.Parameters["Roughness"]?.SetValue(Roughness);

        foreach (EffectPass pass in Shader.CurrentTechnique.Passes) {
            pass.Apply();
            Shader.GraphicsDevice.DrawIndexedPrimitives(
                primitiveType,
                part.VertexOffset,
                part.StartIndex,
                part.PrimitiveCount
            );
        }
    }
}
