using System;
using Embyr;
using Embyr.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BoidsDemo;

public class Boid : PhysicsActor, IAgent {
    private readonly Rectangle containingRect;
    private readonly Light light;

    public override bool ShouldBeSaved => false;

    public Boid(Rectangle containingRect, Vector2 position, Scene scene)
    : base("boid", position, new Rectangle(-2, -2, 4, 4), 1, 400, scene) {
        Physics.EnableGravity = false;
        Physics.EnableCollisions = false;
        this.containingRect = containingRect;

        light = new Light() {
            Color = new Color(
                Random.Shared.NextSingle() * 0.5f + 0.5f,
                Random.Shared.NextSingle() * 0.5f + 0.5f,
                Random.Shared.NextSingle() * 0.5f + 0.5f
            ),
            Transform = new Transform() {
                Parent = Transform,
                Position = Vector2.Zero
            },
            Intensity = 0.3f,
            Radius = 20,
            LinearFalloff = 5
        };

        OnAdded += (scene) => scene.AddLight(light);
        OnRemoved += (scene) => scene.RemoveLight(light);
    }

    public Vector2 UpdateBehavior(float dt) {
        if (Input.IsLeftMouseDown()) {
            return this.Seek(Input.MouseWorldPos) * 0.4f;
        }

        Vector2 flockingForce = this.Flock(5, 3, 15, 2, 40, 1) * 0.2f;
        Vector2 wander = this.Wander(0.3f, 50) * 0.04f;
        Vector2 stayInRect = this.StayInRect(containingRect) * 0.5f;

        return flockingForce + wander + stayInRect;
    }

    public override void Draw(SpriteBatch sb) {
        float rot = MathF.Atan2(Physics.Direction.Y, Physics.Direction.X) + MathF.PI / 2.0f;

        Vector2 t = Transform.GlobalPosition + Vector2.Rotate(new Vector2(0, -5), rot);
        Vector2 bl = Transform.GlobalPosition + Vector2.Rotate(new Vector2(-3, 5), rot);
        Vector2 br = Transform.GlobalPosition + Vector2.Rotate(new Vector2(3, 5), rot);

        sb.DrawLineCentered(t, bl, 2, Color.White);
        sb.DrawLineCentered(bl, br, 2, Color.White);
        sb.DrawLineCentered(br, t, 2, Color.White);
    }
}
