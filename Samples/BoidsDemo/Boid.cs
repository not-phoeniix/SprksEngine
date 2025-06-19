using System;
using Embyr;
using Embyr.Physics;
using Embyr.Scenes;
using Embyr.Tools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BoidsDemo;

public class Boid : Agent2D {
    private static readonly float wanderTime = 0.3f;
    private static readonly float wanderRadius = 50;

    private readonly Rectangle containingRect;
    private readonly float cohesionStrength;
    private readonly float alignStrength;

    public Boid(Rectangle containingRect, Vector2 position, Scene2D scene)
    : base(position, scene) {
        PhysicsComponent2D physics = AddComponent<PhysicsComponent2D>();
        physics.EnableGravity = false;
        physics.EnableCollisions = false;
        physics.MaxSpeed = Random.Shared.NextSingle(180, 240);

        this.containingRect = containingRect;
        this.cohesionStrength = Random.Shared.NextSingle(0.5f, 2);
        this.alignStrength = Random.Shared.NextSingle(1.5f, 3);
    }

    public override Vector2 UpdateBehavior(float dt) {
        if (Input.IsLeftMouseDown()) {
            return this.Seek(Input.MouseWorldPos) * 0.4f;
        }

        Vector2 flockingForce = this.Flock(5, 2, 20, cohesionStrength, 20, alignStrength) * 0.1f;
        Vector2 wander = this.Wander(wanderTime, wanderRadius, MathF.PI / 6) * 0.1f;
        Vector2 stayInRect = this.StayInRect(containingRect) * 0.5f;

        return flockingForce + wander + stayInRect;
    }

    public override void Draw(SpriteBatch sb) {
        float rot = MathF.Atan2(Physics.Direction.Y, Physics.Direction.X) + MathF.PI / 2.0f;

        Vector2 t = Transform.GlobalPosition + Vector2.Rotate(new Vector2(0, -3), rot);
        Vector2 bl = Transform.GlobalPosition + Vector2.Rotate(new Vector2(-1.5f, 2), rot);
        Vector2 br = Transform.GlobalPosition + Vector2.Rotate(new Vector2(1.5f, 2), rot);

        sb.DrawLineCentered(t, bl, 1, Color.White);
        sb.DrawLineCentered(bl, br, 1, Color.White);
        sb.DrawLineCentered(br, t, 1, Color.White);
    }

    public override void DebugDraw(SpriteBatch sb) {
        base.DebugDraw(sb);

        sb.DrawLine(
            Transform.GlobalPosition,
            this.CalcFuturePosition(wanderTime),
            1,
            Color.White
        );

        sb.DrawCircleOutline(
            this.CalcFuturePosition(wanderTime),
            wanderRadius,
            20,
            1,
            Color.White
        );
    }
}
