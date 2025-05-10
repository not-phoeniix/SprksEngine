using System.Diagnostics;
using Embyr;
using Embyr.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace _3DTest;

public class MainScene(string name) : Scene3D(name) {
    public override void LoadContent() {
        TestActor actor = new("test actor!", new Vector3(0, 0, 0), this);
        AddActor(actor);

        float ambientGray = 0.03f;
        AmbientColor = new Color(ambientGray, ambientGray, ambientGray);

        base.LoadContent();

        // Camera.Transform.GlobalPosition = new Vector3(0, 0, 10);
        Camera.PerspectiveFOV = MathHelper.ToRadians(90);
    }

    public override void Update(float dt) {
        // Camera.Transform.GlobalRotation = Quaternion.CreateFromRotationMatrix(
        //     Matrix.CreateLookAt(
        //         Camera.Transform.GlobalPosition,
        //         actor.Transform.GlobalPosition,
        //         Vector3.Up
        //     )
        // );

        if (Input.IsKeyDown(Keys.Left)) {
            // Camera.Transform.GlobalRotation += new Vector3(0, dt, 0);
        }

        if (Input.IsKeyDown(Keys.Right)) {
            // Camera.Transform.GlobalRotation += new Vector3(0, -dt, 0);
        }

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

        // Camera.LookAt(actor.Transform.GlobalPosition);

        Debug.WriteLine($"forward: {Camera.Transform.Forward.X} {Camera.Transform.Forward.Y} {Camera.Transform.Forward.Z}");
        Debug.WriteLine($"pos: {Camera.Transform.GlobalPosition.X} {Camera.Transform.GlobalPosition.Y} {Camera.Transform.GlobalPosition.Z}");

        base.Update(dt);
    }
}
