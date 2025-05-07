using Embyr;
using Embyr.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace _3DTest;

public class TestActor : Actor3D {
    private float timeSum;

    public override BoundingBox Bounds => new(
        Transform.GlobalPosition - Vector3.One,
        Transform.GlobalPosition + Vector3.One
    );

    public override bool ShouldBeSaved => false;

    public TestActor(string name, Vector3 position, Scene3D scene)
    : base(name, position, ContentHelper.I.Load<Model>("cube"), scene) {
    }

    public override void Draw(Camera3D camera) {
        base.Draw(camera);
    }

    public override void Update(float deltaTime) {
        timeSum += deltaTime;

        // Transform.GlobalPosition = new Vector3(timeSum * 2, 0, 0);

        base.Update(deltaTime);
    }
}
