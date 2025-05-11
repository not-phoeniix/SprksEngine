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
    /// Shader associated with this material
    /// </summary>
    public Effect Shader { get; }

    /// <summary>
    /// Gets/sets color of the material's surface
    /// </summary>
    public Color SurfaceColor { get; set; }

    /// <summary>
    /// Gets/sets the world matrix for rendering, comes from a Transform3D
    /// </summary>
    public Matrix World { get; set; }

    /// <summary>
    /// Gets/sets the world inverse transpose matrix for rendering, comes from a Transform3D
    /// </summary>
    public Matrix WorldInverseTranspose { get; set; }

    /// <summary>
    /// Gets/sets the view matrix for rendering, comes from a Camera3D
    /// </summary>
    public Matrix View { get; set; }

    /// <summary>
    /// Gets/sets the projection matrix for rendering, comes from a Camera3D
    /// </summary>
    public Matrix Projection { get; set; }

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
        Shader.Parameters["SurfaceColor"].SetValue(SurfaceColor.ToVector3());
        Shader.Parameters["World"].SetValue(World);
        Shader.Parameters["WorldInverseTranspose"].SetValue(WorldInverseTranspose);
        Shader.Parameters["View"].SetValue(View);
        Shader.Parameters["Projection"].SetValue(Projection);

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
