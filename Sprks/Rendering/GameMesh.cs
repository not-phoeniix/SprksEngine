using Sprks.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sprks.Rendering;

/// <summary>
/// A 3D mesh to draw to the world, contains 3D data and shaders
/// </summary>
public class GameMesh {
    private readonly Model model;
    private readonly GraphicsDevice gd;

    /// <summary>
    /// Creates a new GameMesh instance
    /// </summary>
    /// <param name="model">3D model to draw</param>
    /// <param name="shader">Shader to draw with</param>
    public GameMesh(Model model, GraphicsDevice gd) {
        this.model = model;
        this.gd = gd ?? throw new NullReferenceException("Cannot initialize game mesh with null GraphicsDevice!");
    }

    /// <summary>
    /// Draws this game mesh to the screen
    /// </summary>
    /// <param name="transform">3D transform to render mesh at</param>
    /// <param name="camera">Camera to view mesh with</param>
    /// <param name="material">To use when drawing this mesh</param>
    /// <param name="primitiveType">Primitive type to use when rendering geometry</param>
    public void Draw(Transform3D transform, Camera3D camera, Material3D material, PrimitiveType primitiveType = PrimitiveType.TriangleList) {
        foreach (ModelMesh mesh in model.Meshes) {
            foreach (ModelMeshPart part in mesh.MeshParts) {
                // drawing code copied from ModelMesh.Draw(); method, used to
                //   speed up rendering and be more customizable
                if (part.PrimitiveCount > 0) {
                    material.Shader.Parameters["World"].SetValue(transform.WorldMatrix);
                    material.Shader.Parameters["WorldInverseTranspose"].SetValue(transform.WorldInverseTranspose);
                    material.Shader.Parameters["View"].SetValue(camera.ViewMatrix);
                    material.Shader.Parameters["Projection"].SetValue(camera.ProjectionMatrix);
                    material.Shader.Parameters["CamWorldPos"]?.SetValue(camera.Transform.GlobalPosition);

                    material.ApplyAndDraw(part, primitiveType);
                }
            }
        }
    }
}
