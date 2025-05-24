using Embyr.Scenes;
using Microsoft.Xna.Framework.Graphics;

namespace Embyr;

/// <summary>
/// Abstract parent of all 2D actor components in the engine, implements IDrawable2D and IDebugDrawable2D
/// </summary>
public abstract class ActorComponent2D : IDrawable2D, IDebugDrawable2D {
    /// <summary>
    /// Gets a reference to the actor this component is attached to
    /// </summary>
    protected Actor2D Actor { get; private set; }

    /// <summary>
    /// Gets/sets whether or not this component is enabled
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Creates a new ActorComponent2D instance
    /// </summary>
    /// <param name="actor">Actor this component is attached to</param>
    public ActorComponent2D(Actor2D actor) {
        this.Actor = actor;
        this.Enabled = true;
    }

    /// <summary>
    /// Updates this component
    /// </summary>
    /// <param name="deltaTime">Time passed since last frame</param>
    public abstract void Update(float deltaTime);

    /// <summary>
    /// Updates physics for this component
    /// </summary>
    /// <param name="deltaTime">Time passed since last physics update</param>
    public abstract void PhysicsUpdate(float deltaTime);

    /// <summary>
    /// Draws this component to the screen
    /// </summary>
    /// <param name="sb">SpriteBatch to draw with</param>
    public abstract void Draw(SpriteBatch sb);

    /// <summary>
    /// Draws debug information for this component to the screen
    /// </summary>
    /// <param name="sb">SpriteBatch to draw with</param>
    public abstract void DebugDraw(SpriteBatch sb);
}
