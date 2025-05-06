using Embyr;
using Embyr.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Platformer;

public class Player : PhysicsActor {
    private readonly Light light;
    private readonly Texture2D sprite;

    public override bool ShouldBeSaved => false;

    public Player(Vector2 position, Scene scene)
    : base("Player", position, new Rectangle(-4, -4, 8, 8), 1, 500, scene) {
        light = new Light() {
            Color = Color.White,
            Radius = 20,
            Transform = new Transform() {
                Parent = Transform,
                Position = Vector2.Zero
            }
        };

        sprite = ContentHelper.I.Load<Texture2D>("player");

        OnAdded += (scene) => scene.AddLight(light);
        OnRemoved += (scene) => scene.RemoveLight(light);
    }

    public override void PhysicsUpdate(float deltaTime) {
        Vector2 movement = new Vector2(
            Input.MoveDirection.X,
            0
        ) * 60f;

        Physics.ApplyForce(movement);

        base.PhysicsUpdate(deltaTime);
    }

    public override void Draw(SpriteBatch sb) {
        sb.Draw(sprite, Bounds, Color.White);
    }
}
