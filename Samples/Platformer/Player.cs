using System;
using System.Diagnostics;
using Embyr;
using Embyr.Physics;
using Embyr.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Platformer;

public class Player : PhysicsActor2D {
    private readonly Light2D light;
    private readonly SpriteComponent2D sprite;

    private static readonly float jumpMaxTime = 0.3f;

    public override bool ShouldBeSaved => false;

    public Player(Vector2 position, Scene2D scene)
    : base("Player", position, null, 1, 700, 2f, scene) {
        light = new Light2D() {
            Color = Color.White,
            Radius = 70,
            LinearFalloff = 40,
            Transform = new Transform2D() {
                Parent = Transform,
                Position = Vector2.Zero
            }
        };

        // TODO: make a component system!
        Collider = new BoxCollider2D(this, new Vector2(6));
        sprite = new SpriteComponent2D(this, ContentHelper.I.Load<Texture2D>("player"));

        Physics.GroundFrictionScale = 20.0f;
        Physics.OnCollide += () => Debug.WriteLine("wow");

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
            Physics.ApplyImpulse(new Vector2(0, -100));
        }

        base.PhysicsUpdate(deltaTime);
    }

    public override void Draw(SpriteBatch sb) {
        sprite.Color = Physics.OnGround ? Color.White : Color.Red;
        sprite.Draw(sb);
    }
}
