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

    public override bool ShouldBeSaved => false;

    public Player(Vector2 position, Scene2D scene)
    : base("Player", position, null, 1, 700, 2f, scene) {
        light = new Light2D() {
            Color = Color.White,
            Radius = 30,
            Transform = new Transform2D() {
                Parent = Transform,
                Position = Vector2.Zero
            }
        };

        // TODO: make a component system!
        Collider = new RectCollider2D(this, new Vector2(10));
        sprite = new SpriteComponent2D(this, ContentHelper.I.Load<Texture2D>("player"));

        OnAdded += (scene) => scene.AddLight(light);
        OnRemoved += (scene) => scene.RemoveLight(light);
    }

    public override void PhysicsUpdate(float deltaTime) {
        Vector2 movement = Input.GetComposite2D(
            "left",
            "up",
            "right",
            "down",
            true
        ) * 250;

        Physics.ApplyForce(movement);
        Physics.ApplyFriction(160);

        if (Input.IsKeyDown(Keys.Right)) {
            Transform.GlobalRotation += deltaTime * 5;
        }

        if (Input.IsKeyDown(Keys.Left)) {
            Transform.GlobalRotation -= deltaTime * 5;
        }

        base.PhysicsUpdate(deltaTime);
    }

    public override void Draw(SpriteBatch sb) {
        sprite.Draw(sb);
    }
}
