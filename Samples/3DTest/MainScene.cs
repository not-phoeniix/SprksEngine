using System.Diagnostics;
using Embyr;
using Embyr.Rendering;
using Embyr.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace _3DTest;

public class MainScene(string name) : Scene3D(name) {
    private TestActor parentActor;
    private TestActor childActor;

    public override void LoadContent() {
        base.LoadContent();

        // Camera.Transform.GlobalPosition = new Vector3(0, 0, 10);
        Camera.PerspectiveFOV = MathHelper.ToRadians(90);

        parentActor = new(
            "test actor!",
            new Vector3(0, 0, 0),
            new Material3D() {
                SurfaceColor = Color.Red
            },
            this
        );
        AddActor(parentActor);

        childActor = new TestActor(
            "test actor 2!",
            new Vector3(3, 3, 3),
            new Material3D() {
                SurfaceColor = Color.Blue
            },
            this
        );
        childActor.Transform.Parent = parentActor.Transform;
        childActor.Transform.Scale = new Vector3(0.5f, 0.5f, 0.5f);
        AddActor(childActor);

        float ambientGray = 0.03f;
        AmbientColor = new Color(ambientGray, ambientGray, ambientGray);
    }

    public override void Update(float dt) {
        parentActor.Transform.Rotation += new Vector3(0, dt, 0);
        childActor.Transform.Rotation += new Vector3(dt, dt, dt);

        if (Input.IsLeftMouseDown()) {
            Vector2 delta = Input.MousePosDelta;

            Camera.Transform.GlobalRotation += new Vector3(
                delta.Y * dt,
                -delta.X * dt,
                0
            );
        }

        Camera.Transform.GlobalPosition += Camera.Transform.Forward * -Input.MoveDirection.Y * dt * 8;
        Camera.Transform.GlobalPosition += Camera.Transform.Right * -Input.MoveDirection.X * dt * 8;

        if (Input.IsKeyDown(Keys.LeftShift)) {
            Camera.Transform.GlobalPosition += new Vector3(0, -dt * 8, 0);
        }

        if (Input.IsKeyDown(Keys.Space)) {
            Camera.Transform.GlobalPosition += new Vector3(0, dt * 8, 0);
        }

        if (Input.IsKeyDown(Keys.RightShift)) {
            Camera.LookAt(Vector3.Zero);
        }

        base.Update(dt);
    }
}
