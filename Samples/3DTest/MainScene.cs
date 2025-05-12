using System.Diagnostics;
using Embyr;
using Embyr.Rendering;
using Embyr.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace _3DTest;

public class MainScene(string name) : Scene3D(name) {
    private readonly float cameraLookSpeed = 400;
    private TestActor parentActor;
    private TestActor childActor;

    public override void LoadContent() {
        base.LoadContent();

        Camera.PerspectiveFOV = MathHelper.ToRadians(90);
        Camera.Transform.GlobalPosition = new Vector3(0, 4, -5);
        Camera.LookAt(Vector3.Zero);

        EngineSettings.Gamma = 1.7f;

        GameMesh cube = ContentHelper.I.Load<GameMesh>("cube");
        GameMesh sphere = ContentHelper.I.Load<GameMesh>("sphere");

        parentActor = new TestActor(
            "test actor!",
            new Vector3(0, 0, 0),
            cube,
            new Material3D() {
                SurfaceColor = new Color(0.01f, 0.01f, 0.01f),
                Roughness = 0.99f
            },
            this
        );
        AddActor(parentActor);

        childActor = new TestActor(
            "test actor 2!",
            new Vector3(3, 3, 3),
            cube,
            new Material3D() {
                SurfaceColor = Color.BlueViolet,
                Roughness = 0.0f
            },
            this
        );
        childActor.Transform.Parent = parentActor.Transform;
        childActor.Transform.Scale = new Vector3(0.5f, 0.5f, 0.5f);
        AddActor(childActor);

        TestActor sphereActor = new(
            "sphere actor!",
            new Vector3(5, 0, 0),
            sphere,
            new Material3D() {
                SurfaceColor = Color.SkyBlue,
                Roughness = 0.9f
            },
            this
        );
        AddActor(sphereActor);

        TestActor sphereActorTwo = new(
            "sphere actor two!",
            new Vector3(-5, 0, 0),
            sphere,
            new Material3D() {
                SurfaceColor = Color.Green,
                Roughness = 0.3f
            },
            this
        );
        AddActor(sphereActorTwo);

        float ambientGray = 0.01f;
        AmbientColor = new Color(ambientGray, ambientGray, ambientGray);
    }

    public override void Update(float dt) {
        parentActor.Transform.Rotation += new Vector3(0, dt, 0) * 0.4f;
        childActor.Transform.Rotation += new Vector3(dt, dt, dt) * 0.8f;

        if (Input.IsLeftMouseDown()) {
            Vector2 delta = Input.MousePosDelta;

            Camera.Transform.GlobalRotation += new Vector3(
                delta.Y * dt / EngineSettings.GameCanvasResolution.X * cameraLookSpeed,
                -delta.X * dt / EngineSettings.GameCanvasResolution.Y * cameraLookSpeed,
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
