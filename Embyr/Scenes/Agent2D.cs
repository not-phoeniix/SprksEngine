using Embyr.Physics;
using Microsoft.Xna.Framework;

namespace Embyr.Scenes;

/// <summary>
/// Interface to describe any autonomous agent actor
/// </summary>
public abstract class Agent2D : Actor2D {
    private PhysicsComponent2D? physics;

    /// <summary>
    /// Gets the physics component of this agent
    /// </summary>
    public PhysicsComponent2D Physics {
        get {
            physics ??= GetComponent<PhysicsComponent2D>();

            if (physics == null) {
                throw new Exception("Agent does not have PhysicsComponent2D attached, cannot update agent!");
            }

            return physics;
        }
    }

    /// <summary>
    /// Creates a new Agent2D
    /// </summary>
    /// <param name="position">Position to place agent at</param>
    /// <param name="scene">Scene to spawn agent into</param>
    public Agent2D(Vector2 position, Scene2D scene)
    : base(position, scene) { }

    public override void PhysicsUpdate(float deltaTime) {
        Physics.ApplyForce(UpdateBehavior(deltaTime));
        base.PhysicsUpdate(deltaTime);
    }

    /// <summary>
    /// Method that calculates all steering forces for this agent
    /// </summary>
    /// <param name="dt">Time passed since last frame</param>
    /// <returns>Sum of all behavior forces to apply</returns>
    public abstract Vector2 UpdateBehavior(float dt);
}

