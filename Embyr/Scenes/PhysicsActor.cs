using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Embyr.Physics;

namespace Embyr.Scenes;

/// <summary>
/// An abstract base class that implements all <c>IActor</c> members along with a physics component.
/// </summary>
public abstract class PhysicsActor : Actor2D {
    #region // Fields & Properties

    /// <summary>
    /// Entity's physics component
    /// </summary>
    public PhysicsComponent2D Physics { get; }

    /// <summary>
    /// Gets the bounds of this physics actor in the world
    /// </summary>
    public override Rectangle Bounds => Physics.Bounds;

    #endregion

    /// <summary>
    /// Creates a new Entity object
    /// </summary>
    /// <param name="name">Name of this actor</param>
    /// <param name="position">Initial position</param>
    /// <param name="verticalCollisionBox">
    /// Local-to-sprite collision box that defines where/how
    /// vertical collision takes place with this object
    /// </param>
    /// <param name="horizontalCollisionBox">
    /// Local-to-sprite collision box that defines where/how
    /// horizontal collision takes place with this object
    /// </param>
    /// <param name="mass">Mass of actor</param>
    /// <param name="maxSpeed">Maximum velocity actor can achieve</param>
    /// <param name="scene">Scene to place this actor in</param>
    public PhysicsActor(
        string name,
        Vector2 position,
        Rectangle verticalCollisionBox,
        Rectangle horizontalCollisionBox,
        float mass,
        float maxSpeed,
        Scene2D scene
    ) : base(name, position, scene) {
        Physics = new PhysicsComponent2D(
            Transform,
            verticalCollisionBox,
            horizontalCollisionBox,
            mass,
            maxSpeed
        );
    }

    /// <summary>
    /// Creates a new Entity object
    /// </summary>
    /// <param name="name">Name of this actor</param>
    /// <param name="position">Initial position</param>
    /// <param name="spriteBounds">Local-to-sprite bounds used for collision</param>
    /// <param name="mass">Mass of actor</param>
    /// <param name="maxSpeed">Maximum velocity actor can achieve</param>
    /// <param name="scene">Scene to place this actor in</param>
    public PhysicsActor(
        string name,
        Vector2 position,
        Rectangle spriteBounds,
        float mass,
        float maxSpeed,
        Scene2D scene
    ) : base(name, position, scene) {
        Physics = new PhysicsComponent2D(
            Transform,
            spriteBounds,
            mass,
            maxSpeed
        );
    }

    #region // Methods

    /// <summary>
    /// Updates general logic for this actor
    /// </summary>
    /// <param name="deltaTime">Time passed since last frame</param>
    public override void Update(float deltaTime) {
        Physics.UpdateTransform();
    }

    /// <summary>
    /// Updates physics calculations for this actor
    /// </summary>
    /// <param name="deltaTime">Time passed since last fixed update call</param>
    public override void PhysicsUpdate(float deltaTime) {
        Physics.Update((Scene2D)Scene, deltaTime);
    }

    /// <summary>
    /// Draws debug information for this actor
    /// </summary>
    /// <param name="sb">SpriteBatch to draw with</param>
    public override void DebugDraw(SpriteBatch sb) {
        Physics.DebugDraw(sb);
    }

    #endregion
}
