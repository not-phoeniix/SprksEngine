using Embyr;
using Embyr.Rendering;
using Embyr.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace _3DTest;

public class TestActor : Actor3D {
    public override BoundingBox Bounds => new(
        Transform.GlobalPosition - Vector3.One,
        Transform.GlobalPosition + Vector3.One
    );

    public override bool ShouldBeSaved => false;

    public TestActor(string name, Vector3 position, Material3D material, Scene3D scene)
    : base(name, position, ContentHelper.I.Load<GameMesh>("cube"), material, scene) {
    }

    public override void Draw(Camera3D camera) {
        base.Draw(camera);
    }

    public override void Update(float deltaTime) {
        base.Update(deltaTime);
    }
}
