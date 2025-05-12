using System;
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
    private Transform3D orbitLightAnchor;
    private Light3D orbitLight;

    public override void LoadContent() {
        base.LoadContent();

        Camera.PerspectiveFOV = MathHelper.ToRadians(90);
        Camera.Transform.GlobalPosition = new Vector3(0, 4, -5);
        Camera.LookAt(Vector3.Zero);

        EngineSettings.Gamma = 1.7f;

        float ambientGray = 0.01f;
        AmbientColor = new Color(ambientGray, ambientGray, ambientGray);

        GameMesh cube = ContentHelper.I.Load<GameMesh>("cube");
        GameMesh sphere = ContentHelper.I.Load<GameMesh>("sphere");

        TestActor floor = new(
            "floor",
            new Vector3(0, -2, 0),
            cube,
            new Material3D() {
                SurfaceColor = new Color(0.8f, 0.8f, 0.8f),
                Roughness = 0.9f
            },
            this
        );
        floor.Transform.GlobalScale = new Vector3(100, 0.1f, 100);
        AddActor(floor);

        parentActor = new TestActor(
            "test actor!",
            new Vector3(0, 0, 0),
            cube,
            new Material3D() {
                SurfaceColor = new Color(0.05f, 0.05f, 0.05f),
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
                Roughness = 0.6f
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
                Roughness = 0.2f
            },
            this
        );
        AddActor(sphereActorTwo);

        Light3D globalLight = new() {
            Color = Color.White,
            Intensity = 0.4f,
            IsGlobal = true
        };
        globalLight.Transform.GlobalRotation = new Vector3(0.92f, 0.21f, 0);
        AddLight(globalLight);

        orbitLight = new Light3D() {
            Color = Color.Red,
            Intensity = 1.0f,
            Range = 20,
            SpotInnerAngle = MathF.PI / 6,
            SpotOuterAngle = MathF.PI / 4,
        };
        orbitLightAnchor = new Transform3D(Vector3.Zero);
        orbitLight.Transform.Parent = orbitLightAnchor;
        orbitLight.Transform.Position = new Vector3(10, 4, 0);
        orbitLight.Transform.LookAt(Vector3.Zero);
        AddLight(orbitLight);
    }

    public override void Update(float dt) {
        parentActor.Transform.Rotation += new Vector3(0, dt, 0) * 0.4f;
        childActor.Transform.Rotation += new Vector3(dt, dt, dt) * 0.8f;
        orbitLightAnchor.GlobalRotation += new Vector3(0, dt, 0) * 0.3f;

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
