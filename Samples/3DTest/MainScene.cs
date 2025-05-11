using System.Diagnostics;
using Embyr;
using Embyr.Rendering;
using Embyr.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace _3DTest;

public class MainScene(string name) : Scene3D(name) {
    private TestActor parentActor;
    private float timeSum;

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

        TestActor actorTwo = new(
            "test actor 2!",
            new Vector3(3, 3, 3),
            new Material3D() {
                SurfaceColor = Color.Blue
            },
            this
        );
        // actorTwo.Transform.Parent = parentActor.Transform;
        // actorTwo.Transform.Position = new Vector3(5, 5, 5);
        actorTwo.Transform.Scale = new Vector3(0.5f, 0.5f, 0.5f);
        AddActor(actorTwo);

        float ambientGray = 0.03f;
        AmbientColor = new Color(ambientGray, ambientGray, ambientGray);
    }

    public override void Update(float dt) {
        timeSum += dt;

        // Camera.Transform.GlobalRotation = Quaternion.CreateFromRotationMatrix(
        //     Matrix.CreateLookAt(
        //         Camera.Transform.GlobalPosition,
        //         actor.Transform.GlobalPosition,
        //         Vector3.Up
        //     )
        // );

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
