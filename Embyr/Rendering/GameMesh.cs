using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Embyr.Rendering;

/// <summary>
/// A 3D mesh to draw to the world, contains 3D data and shaders
/// </summary>
public class GameMesh {
    private readonly Model model;
    internal readonly Effect shader;
    private readonly GraphicsDevice gd;

    /// <summary>
    /// Creates a new GameMesh instance
    /// </summary>
    /// <param name="model">3D model to draw</param>
    /// <param name="shader">Shader to draw with</param>
    public GameMesh(Model model, Effect shader) {
        this.model = model;
        this.shader = shader;
        this.gd = shader.GraphicsDevice ?? throw new NullReferenceException("Cannot initialize game mesh with null GraphicsDevice!");
    }

    /// <summary>
    /// Draws this game mesh to the screen
    /// </summary>
    /// <param name="transform">3D transform to render mesh at</param>
    /// <param name="camera">Camera to view mesh with</param>
    public void Draw(Transform3D transform, Camera3D camera) {
        shader.Parameters["World"].SetValue(transform.WorldMatrix);
        shader.Parameters["WorldInverseTranspose"]?.SetValue(transform.WorldInverseTranspose);
        shader.Parameters["View"].SetValue(camera.ViewMatrix);
        shader.Parameters["Projection"].SetValue(camera.ProjectionMatrix);

        foreach (ModelMesh mesh in model.Meshes) {
            foreach (ModelMeshPart part in mesh.MeshParts) {
                // drawing code copied from ModelMesh.Draw(); method, used to
                //   speed up rendering and be more customizable
                if (part.PrimitiveCount > 0) {
                    gd.SetVertexBuffer(part.VertexBuffer);
                    gd.Indices = part.IndexBuffer;

                    foreach (EffectPass pass in shader.CurrentTechnique.Passes) {
                        pass.Apply();
                        gd.DrawIndexedPrimitives(
                            PrimitiveType.TriangleList,
                            part.VertexOffset,
                            part.StartIndex,
                            part.PrimitiveCount
                        );
                    }
                }
            }
        }
    }
}
