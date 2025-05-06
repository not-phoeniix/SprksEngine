using System;
using Embyr;
using Embyr.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace BoidsDemo;

public class MainScene(string name) : Scene2D(name) {
    public override void LoadContent() {
        Rectangle boidBounds = new(
            -200,
            -100,
            400,
            200
        );

        for (int i = 0; i < 400; i++) {
            Vector2 randPos = new(
                Random.Shared.Next(boidBounds.Left, boidBounds.Right),
                Random.Shared.Next(boidBounds.Top, boidBounds.Bottom)
            );

            Boid boid = new(boidBounds, randPos, this);
            AddActor(boid);
        }

        AddLight(new Light2D() {
            IsGlobal = true,
            Intensity = 0.1f
        });

        for (int i = 0; i < 100; i++) {
            Vector2 randPos = new(
                Random.Shared.Next(boidBounds.Left, boidBounds.Right),
                Random.Shared.Next(boidBounds.Top, boidBounds.Bottom)
            );

            AddLight(new Light2D() {
                Transform = new Transform2D(randPos),
                Color = new Color(
                    Random.Shared.NextSingle() * 0.5f + 0.5f,
                    Random.Shared.NextSingle() * 0.5f + 0.5f,
                    Random.Shared.NextSingle() * 0.5f + 0.5f
                ),
                Radius = 100,
                Intensity = 0.3f
            });
        }

        base.LoadContent();
    }

    public override void Update(float dt) {
        base.Update(dt);

        if (Input.IsKeyDownOnce(Keys.Space)) {
            EngineSettings.ShowDebugDrawing = !EngineSettings.ShowDebugDrawing;
        }
    }
}
