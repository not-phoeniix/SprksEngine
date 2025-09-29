using System;
using Sprks;
using Sprks.Scenes;
using Sprks.Tools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace BoidsDemo;

public class MainScene : Scene2D {
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
            Intensity = 0.02f,
            Transform = new Transform2D() {
                ZIndex = 100
            }
        });

        for (int i = 0; i < 30; i++) {
            Vector2 randPos = new(
                Random.Shared.Next(boidBounds.Left, boidBounds.Right),
                Random.Shared.Next(boidBounds.Top, boidBounds.Bottom)
            );

            AddLight(new Light2D() {
                Transform = new Transform2D() {
                    Position = randPos,
                    ZIndex = 20,
                },
                Color = new Color(
                    Random.Shared.NextSingle(0.7f, 1.0f),
                    Random.Shared.NextSingle(0.2f, 0.9f),
                    Random.Shared.NextSingle(0.6f, 1.0f)
                ),
                Radius = Random.Shared.NextSingle(70, 120),
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
