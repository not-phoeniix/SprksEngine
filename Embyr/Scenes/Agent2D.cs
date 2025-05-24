using Embyr.Physics;
using Microsoft.Xna.Framework;

namespace Embyr.Scenes;

/// <summary>
/// Interface to describe any autonomous agent actor
/// </summary>
public abstract class Agent2D : Actor2D {
    /// <summary>
    /// Gets the physics component of this agent
    /// </summary>
    public PhysicsComponent2D Physics { get; private set; }

    /// <summary>
    /// Creates a new Agent2D
    /// </summary>
    /// <param name="name"></param>
    /// <param name="position"></param>
    /// <param name="scene"></param>
    public Agent2D(string name, Vector2 position, Scene2D scene)
    : base(name, position, scene) { }

    public override void PhysicsUpdate(float deltaTime) {
        // get reference to physics component on the first physics frame
        if (Physics != null) {
            Physics = GetComponent<PhysicsComponent2D>();
            if (Physics == null) {
                throw new Exception("Agent does not have PhysicsComponent2D attached, cannot update agent!");
            }
        }

        Physics!.ApplyForce(UpdateBehavior(deltaTime));

        base.PhysicsUpdate(deltaTime);
    }

    /// <summary>
    /// Method that calculates all steering forces for this agent
    /// </summary>
    /// <param name="dt">Time passed since last frame</param>
    /// <returns>Sum of all behavior forces to apply</returns>
    public abstract Vector2 UpdateBehavior(float dt);
}

