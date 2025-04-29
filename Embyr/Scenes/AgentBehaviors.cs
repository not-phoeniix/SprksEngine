using Microsoft.Xna.Framework;
using Embyr.Physics;

namespace Embyr.Scenes;

/// <summary>
/// Autonomous Agent behavior methods, Contains
/// methods for basic AI and various steering behaviors
/// </summary>
public static class AgentBehaviors {
    /// <summary>
    /// Calculates a seeking force towards a target
    /// </summary>
    /// <param name="targetPos">Position to seek</param>
    /// <param name="agent">Physics component agent to base forces upon</param>
    /// <returns>Vector2 force to apply that seeks target</returns>
    public static Vector2 Seek(this IAgent agent, Vector2 targetPos) {
        Vector2 direction = targetPos - agent.CenterPosition;
        if (direction != Vector2.Zero) direction.Normalize();
        Vector2 desiredVelocity = direction * agent.Physics.MaxSpeed;
        return desiredVelocity - agent.Physics.Velocity;
    }

    /// <summary>
    /// Calculates a seeking force towards a target
    /// </summary>
    /// <param name="target">Actor to seek</param>
    /// <param name="agent">Physics component agent to base forces upon</param>
    /// <returns>Vector2 force to apply that seeks target</returns>
    public static Vector2 Seek(this IAgent agent, IActor target) {
        return Seek(agent, target.CenterPosition);
    }

    /// <summary>
    /// Calculates a fleeing force towards a target
    /// </summary>
    /// <param name="targetPos">Position to flee from</param>
    /// <param name="agent">Physics component agent to base forces upon</param>
    /// <returns>Vector2 force to apply that flees from target</returns>
    public static Vector2 Flee(this IAgent agent, Vector2 targetPos) {
        Vector2 direction = agent.CenterPosition - targetPos;
        if (direction != Vector2.Zero) direction.Normalize();
        Vector2 desiredVelocity = direction * agent.Physics.MaxSpeed;
        return desiredVelocity - agent.Physics.Velocity;
    }

    /// <summary>
    /// Calculates a fleeing force towards a target
    /// </summary>
    /// <param name="target">Actor to flee from</param>
    /// <param name="agent">Physics component agent to base forces upon</param>
    /// <returns>Vector2 force to apply that flees from target</returns>
    public static Vector2 Flee(this IAgent agent, IActor target) {
        return Flee(agent, target.CenterPosition);
    }

    /// <summary>
    /// Wanders in a random direction
    /// </summary>
    /// <param name="time">Time to seek in the future for</param>
    /// <param name="radius">Radius to seek, the bigger the more it'll turn</param>
    /// <returns>Force to apply to wander in random direction</returns>
    public static Vector2 Wander(this IAgent agent, float time, float radius, float angleRange = MathF.PI / 15) {
        Vector2 targetPos = CalcFuturePosition(time, agent);

        // positive/negative values to randomize between each frame
        float randomValue = Random.Shared.NextSingle();
        agent.Physics.WanderAngle += (randomValue * 2 - 1) * angleRange;

        targetPos.X += MathF.Cos(agent.Physics.WanderAngle) * radius;
        targetPos.Y += MathF.Sin(agent.Physics.WanderAngle) * radius;

        return Seek(agent, targetPos);
    }

    /// <summary>
    /// Arrives at a position, slowing down the closer it gets
    /// </summary>
    /// <param name="targetPos">Position to arrive at</param>
    /// <param name="slowingDistance">Distance where slowing starts</param>
    /// <param name="agent">Physics component agent to base forces upon</param>
    /// <returns>Force to apply to arrive at position</returns>
    public static Vector2 Arrival(this IAgent agent, Vector2 targetPos, float slowingDistance) {
        // initial float calculations
        float dSqr = Vector2.DistanceSquared(targetPos, agent.CenterPosition);
        float rampedSpeed = agent.Physics.MaxSpeed * (dSqr / (slowingDistance * slowingDistance));
        float clippedSpeed = MathF.Min(rampedSpeed, agent.Physics.MaxSpeed);

        // calculate scale of velocity depending on distance
        //   (prevents divide by zero error)
        float vScale = clippedSpeed * clippedSpeed;
        if (dSqr == 0) {
            vScale = 0;
        } else {
            vScale /= dSqr;
        }

        // calculates force and returns
        Vector2 targetOffset = targetPos - agent.CenterPosition;
        Vector2 desiredVelocity = vScale * targetOffset;
        return desiredVelocity - agent.Physics.Velocity;
    }

    /// <summary>
    /// Arrives at an actor, slowing down the closer it gets
    /// </summary>
    /// <param name="target">Actor to arrive at</param>
    /// <param name="slowingDistance">Distance where slowing starts</param>
    /// <param name="agent">Physics component agent to base forces upon</param>
    /// <returns>Force to apply to arrive at entity position</returns>
    public static Vector2 Arrival(this IAgent agent, IActor target, float slowingDistance) {
        return Arrival(agent, target.CenterPosition, slowingDistance);
    }

    /// <summary>
    /// Applies force to stay within a rectangle, return zero if inside rect
    /// </summary>
    /// <param name="rect">Rectangle to check and seek towards if outside of</param>
    /// <param name="agent">Physics component agent to base forces upon</param>
    /// <returns>Force to apply to stay within a rectangle</returns>
    public static Vector2 StayInRect(this IAgent agent, Rectangle rect) {
        if (!rect.Contains(agent.CenterPosition)) {
            return Seek(agent, rect.Center.ToVector2());
        }

        return Vector2.Zero;
    }

    /// <summary>
    /// Separates from a set amount of other agents
    /// </summary>
    /// <param name="radius">Pixel radius around agent to apply separation</param>
    /// <param name="container">Container of agents to separate from</param>
    /// <param name="agent">Physics component agent to base forces upon</param>
    /// <returns>Force to apply to separate from nearby entities</returns>
    public static Vector2 Separate(this IAgent agent, float radius, AgentContainer container) {
        if (container.Count <= 1) return Vector2.Zero;

        Vector2 force = Vector2.Zero;

        foreach (IAgent neighbor in container) {
            float dSqr = Vector2.DistanceSquared(agent.CenterPosition, neighbor.CenterPosition);

            if (dSqr > float.Epsilon && dSqr <= radius * radius) {
                force += Flee(neighbor, agent) * (1 / dSqr);
            }
        }

        return force;
    }

    /// <summary>
    /// Separates from a position depending on distance
    /// </summary>
    /// <param name="radius">Pixel radius around agent to apply separation</param>
    /// <param name="position">Position to separate from</param>
    /// <param name="agent">Physics component agent to base forces upon</param>
    /// <returns>Force to apply to separate from position</returns>
    public static Vector2 Separate(this IAgent agent, float radius, Vector2 position) {
        float distSquared = Vector2.DistanceSquared(agent.CenterPosition, position);

        if (distSquared > float.Epsilon && distSquared <= radius * radius) {
            return Flee(agent, position) * (1 / distSquared);
        }

        return Vector2.Zero;
    }

    /// <summary>
    /// Separates from an actor depending on distance
    /// </summary>
    /// <param name="radius">Pixel radius around agent to apply separation</param>
    /// <param name="target">Actor to separate from</param>
    /// <param name="agent">Physics component agent to base forces upon</param>
    /// <returns>Force to apply to separate from entity</returns>
    public static Vector2 Separate(this IAgent agent, float radius, IActor target) {
        return Separate(agent, radius, target.CenterPosition);
    }

    /// <summary>
    /// Attracts this agent to the center position of other agents
    /// </summary>
    /// <param name="radius">Pixel radius around agent to apply cohesion</param>
    /// <param name="container">Container of agents to apply cohesion around</param>
    /// <param name="agent">Physics component agent to base forces upon</param>
    /// <returns>Force to apply to attract agent to other agents</returns>
    public static Vector2 Cohesion(this IAgent agent, float radius, AgentContainer container) {
        if (container.Count <= 1) return Vector2.Zero;

        int numAgents = 0;
        Vector2 centerPoint = Vector2.Zero;

        foreach (IAgent target in container) {
            float distSquared = Vector2.DistanceSquared(agent.CenterPosition, target.CenterPosition);

            if (distSquared <= radius * radius) {
                centerPoint += target.CenterPosition;
                numAgents++;
            }
        }

        centerPoint /= numAgents;

        return Seek(agent, centerPoint);
    }

    /// <summary>
    /// Aligns direction with other agents
    /// </summary>
    /// <param name="radius">Pixel radius around agent to apply alignment</param>
    /// <param name="container">Container of agents to apply alignment with</param>
    /// <param name="agent">Physics component agent to base forces upon</param>
    /// <returns>Force to apply to align direction with other agents</returns>
    public static Vector2 Alignment(float radius, AgentContainer container, IAgent agent) {
        if (container.Count <= 1) return Vector2.Zero;

        Vector2 direction = Vector2.Zero;

        foreach (IAgent target in container) {
            float distSquared = Vector2.DistanceSquared(agent.CenterPosition, target.CenterPosition);

            if (distSquared > float.Epsilon && distSquared <= radius * radius) {
                direction += target.Physics.Direction;
            }
        }

        if (direction != Vector2.Zero) {
            direction = Vector2.Normalize(direction) * agent.Physics.MaxSpeed;
        }

        return direction - agent.Physics.Velocity;
    }

    /// <summary>
    /// Singular flock behavior that has combines separation, alignment, and cohesion for efficiency
    /// </summary>
    /// <param name="separateRadius">Radius for separation to apply</param>
    /// <param name="separateStrength">Strength multiplier for separation force</param>
    /// <param name="cohesionRadius">Radius for cohesion to apply</param>
    /// <param name="cohesionStrength">Strength multiplier for cohesion force</param>
    /// <param name="alignRadius">Radius for alignment to apply</param>
    /// <param name="alignStrength">Strength multiplier for alignment force</param>
    /// <param name="container">Container of other agents to flock with</param>
    /// <param name="agent">Physics component agent to base forces upon</param>
    /// <returns>Vector2 force to apply that flocks with other agents in the specified container</returns>
    public static Vector2 Flock(
        float separateRadius,
        float separateStrength,
        float cohesionRadius,
        float cohesionStrength,
        float alignRadius,
        float alignStrength,
        AgentContainer container,
        IAgent agent
    ) {
        if (container.Count <= 1) {
            return Vector2.Zero;
        }

        // initial variable setup, pre-iteration
        Vector2 separation = Vector2.Zero;
        Vector2 cohesion = Vector2.Zero;
        Vector2 alignment = Vector2.Zero;
        Vector2 avgPosition = Vector2.Zero;
        Vector2 avgDirection = Vector2.Zero;
        int numCohesion = 0;
        int numAlignment = 0;

        // iteration across all agents in container for variable math
        foreach (IAgent target in container) {
            float dSqr = Vector2.DistanceSquared(agent.CenterPosition, target.CenterPosition);

            // skip iteration if distance is basically zero
            //   (skip flocking with self)
            if (dSqr <= float.Epsilon) {
                continue;
            }

            // separation radius check
            if (dSqr <= separateRadius * separateRadius) {
                separation += Flee(target, agent);
            }

            // cohesion radius check
            if (dSqr <= cohesionRadius * cohesionRadius) {
                avgPosition += target.CenterPosition;
                numCohesion++;
            }

            // alignment radius check
            if (dSqr <= alignRadius * alignRadius) {
                avgDirection += target.Physics.Direction;
                numAlignment++;
            }
        }

        // cohesion force calculation
        if (numCohesion > 0) {
            avgPosition /= numCohesion;
            cohesion = Seek(agent, avgPosition);
        }

        // alignment force calculation
        if (numAlignment > 0) {
            // this *should* be normalized ,,,, hopefully
            avgDirection /= numAlignment;
            avgDirection *= agent.Physics.MaxSpeed;
            alignment = avgDirection - agent.Physics.Velocity;
        }

        return (separateStrength * separation) +
               (cohesionStrength * cohesion) +
               (alignStrength * alignment);
    }

    private static Vector2 CalcFuturePosition(float time, IAgent agent) {
        return agent.Physics.Velocity * time + agent.CenterPosition;
    }
}
