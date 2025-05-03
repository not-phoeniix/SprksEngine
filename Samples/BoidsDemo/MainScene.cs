using System;
using Embyr;
using Embyr.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace BoidsDemo;

public class MainScene(string name) : Scene(name) {
    public override void LoadContent() {
        Rectangle boidBounds = new(
            -200,
            -100,
            400,
            200
        );

        for (int i = 0; i < 200; i++) {
            Vector2 randPos = new(
                Random.Shared.Next(boidBounds.Left, boidBounds.Right),
                Random.Shared.Next(boidBounds.Top, boidBounds.Bottom)
            );

            Boid boid = new(boidBounds, randPos, this);
            AddActor(boid);
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
