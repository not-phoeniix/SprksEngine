using System.Collections;
using Microsoft.Xna.Framework;
using Embyr.Physics;

namespace Embyr.Scenes;

/// <summary>
/// Represents container structure of autonomous agents that deals with
/// internal calculations to help with steering behavior efficiency
/// </summary>
public sealed class AgentContainer : IEnumerable<IAgent2D> {
    private readonly List<IAgent2D> agents;

    /// <summary>
    /// Gets the number of agents stored in this container
    /// </summary>
    public int Count => agents.Count;

    /// <summary>
    /// Gets the average center position of all agents in this container
    /// </summary>
    public Vector2 AverageCenterPos { get; private set; }

    /// <summary>
    /// Gets the average velocity of all agents in this container
    /// </summary>
    public Vector2 AverageVelocity { get; private set; }

    /// <summary>
    /// Gets the normalized average direction of all agents in this container
    /// </summary>
    public Vector2 AverageDirection { get; private set; }

    /// <summary>
    /// Gets the enumerable contents of agent type <c>T</c> objects inside this container
    /// </summary>
    /// <returns>Enumerable of agent objects</returns>
    public IEnumerator<IAgent2D> GetEnumerator() {
        return agents.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }

    /// <summary>
    /// Creates a new empty AgentContainer
    /// </summary>
    public AgentContainer() {
        agents = new List<IAgent2D>();
    }

    /// <summary>
    /// Processes all internal agent data averages at once, iterating across all agents
    /// </summary>
    public void ProcessAvgs() {
        AverageCenterPos = Vector2.Zero;
        AverageVelocity = Vector2.Zero;
        AverageDirection = Vector2.Zero;

        foreach (IAgent2D agent in agents) {
            AverageCenterPos += agent.Transform.GlobalPosition;
            AverageVelocity += agent.Physics.Velocity;
            AverageDirection += agent.Physics.Direction;
        }

        AverageCenterPos /= agents.Count;
        AverageVelocity /= agents.Count;
        AverageDirection /= agents.Count;
    }

    /// <summary>
    /// Adds a new agent to this container
    /// </summary>
    /// <param name="agent">Agent to add</param>
    public void Add(IAgent2D agent) {
        agents.Add(agent);
    }

    /// <summary>
    /// Removes an agent from this container
    /// </summary>
    /// <param name="agent">Agent to remove</param>
    /// <returns>True if successfully removed, false if not</returns>
    public bool Remove(IAgent2D agent) {
        return agents.Remove(agent);
    }

    /// <summary>
    /// Clears this container, removing all internal agents
    /// </summary>
    public void Clear() {
        agents.Clear();
    }
}
