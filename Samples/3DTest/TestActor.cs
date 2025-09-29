using Sprks;
using Sprks.Rendering;
using Sprks.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace _3DTest;

public class TestActor : Actor3D, IDebugDrawable3D {
    public override BoundingBox Bounds => new(
        Transform.GlobalPosition - Transform.GlobalScale,
        Transform.GlobalPosition + Transform.GlobalScale
    );

    public TestActor(string name, Vector3 position, GameMesh mesh, Material3D material, Scene3D scene)
    : base(name, position, mesh, material, scene) {
    }

    public override void Draw(Camera3D camera) {
        base.Draw(camera);
    }

    public override void Update(float deltaTime) {
        base.Update(deltaTime);
    }

    public void DebugDraw(Camera3D camera) {
        // Bounds.RenderBoundingBox(camera, Color.Red);
    }
}
