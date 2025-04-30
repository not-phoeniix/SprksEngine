using Embyr.Physics;

namespace Embyr.Scenes;

/// <summary>
/// Interface to describe any autonomous agent actor
/// </summary>
public interface IAgent : IActor {
    /// <summary>
    /// Gets the physics component of this agent
    /// </summary>
    public PhysicsComponent Physics { get; }

    /// <summary>
    /// Method that calculates all steering forces for this agent
    /// </summary>
    /// <param name="dt">Time passed since last frame</param>
    public void UpdateBehavior(float dt);
}

