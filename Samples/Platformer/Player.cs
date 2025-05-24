using System;
using System.Diagnostics;
using Embyr;
using Embyr.Physics;
using Embyr.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Platformer;

public class Player : Actor2D {
    private readonly Light2D light;
    public readonly PhysicsComponent2D Physics;

    public Player(Vector2 position, Scene2D scene)
    : base("Player", position, scene) {
        light = new Light2D() {
            Color = Color.White,
            Radius = 70,
            LinearFalloff = 40,
            Transform = new Transform2D() {
                Parent = Transform,
                Position = Vector2.Zero
            }
        };

        BoxColliderComponent2D box = AddComponent<BoxColliderComponent2D>();
        box.Size = new Vector2(6);

        Physics = AddComponent<PhysicsComponent2D>();
        Physics.MaxSpeed = 700;
        Physics.MinSpeed = 1;
        Physics.GroundFrictionScale = 20.0f;

        SpriteComponent2D sprite = AddComponent<SpriteComponent2D>();
        sprite.Texture = ContentHelper.I.Load<Texture2D>("player");

        OnAdded += (scene) => scene.AddLight(light);
        OnRemoved += (scene) => scene.RemoveLight(light);
    }

    public override void PhysicsUpdate(float deltaTime) {
        float inputDir = Input.GetComposite1D("left", "right");

        // A/D movement
        if (Physics.OnGround) {
            Physics.ApplyForce(new Vector2(inputDir * 1500, 0));
        } else {
            Physics.ApplyForce(new Vector2(inputDir * 100, 0));
        }

        // jump logic
        if (Input.IsActionOnce("jump") && Physics.OnGround) {
            Physics.ApplyImpulse(new Vector2(0, -150));
        }

        base.PhysicsUpdate(deltaTime);
    }
}
