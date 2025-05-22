using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Embyr.Physics;

namespace Embyr.Scenes;

/// <summary>
/// An abstract base class that implements all <c>IActor2D</c> members along with a physics component.
/// </summary>
public abstract class PhysicsActor2D : Actor2D {
    #region // Fields & Properties

    /// <summary>
    /// Entity's physics component
    /// </summary>
    public PhysicsComponent2D Physics { get; }

    #endregion

    /// <summary>
    /// Creates a new Entity object
    /// </summary>
    /// <param name="name">Name of this actor</param>
    /// <param name="position">Initial position</param>
    /// <param name="collider">Collider to attach to this actor</param>
    /// <param name="mass">Mass of actor</param>
    /// <param name="maxSpeed">Maximum velocity actor can achieve</param>
    /// <param name="minSpeed">Minimum velocity of actor before snapping to zero</param>
    /// <param name="scene">Scene to place this actor in</param>
    public PhysicsActor2D(
        string name,
        Vector2 position,
        Collider2D collider,
        float mass,
        float maxSpeed,
        float minSpeed,
        Scene2D scene
    ) : base(name, position, collider, scene) {
        Physics = new PhysicsComponent2D(
            this,
            mass,
            maxSpeed,
            minSpeed
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
