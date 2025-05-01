using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Embyr.Scenes;

/// <summary>
/// An abstract base class that implements all <c>IActor</c> members.
/// </summary>
public abstract class Actor : IActor, IDebugDrawable {
    #region // Fields & Properties

    /// <summary>
    /// Gets the transform of this actor
    /// </summary>
    public Transform Transform { get; init; }

    /// <summary>
    /// Gets the scene this actor is created within
    /// </summary>
    public Scene Scene { get; }

    /// <summary>
    /// Gets/sets the name of this actor
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets whether or not this actor should be saved into the world file
    /// </summary>
    public abstract bool ShouldBeSaved { get; }

    /// <summary>
    /// Gets/sets the action called when this actor is added to a scene
    /// </summary>
    public event Action<Scene>? OnAdded;

    /// <summary>
    /// Event called when this actor is removed from a scene
    /// </summary>
    public event Action<Scene>? OnRemoved;

    /// <summary>
    /// Gets the bounds of this actor in the world
    /// </summary>
    public abstract Rectangle Bounds { get; }

    #endregion

    /// <summary>
    /// Creates a new Entity object
    /// </summary>
    /// <param name="name">Name of this actor</param>
    /// <param name="position">Initial position</param>
    /// <param name="scene">Scene to place this actor in</param>
    public Actor(
        string name,
        Vector2 position,
        Scene scene
    ) {
        this.Transform = new Transform(position);
        this.Name = name;
        this.Scene = scene;
    }

    #region // Methods

    /// <summary>
    /// Updates general logic for this actor
    /// </summary>
    /// <param name="deltaTime">Time passed since last frame</param>
    public virtual void Update(float deltaTime) { }

    /// <summary>
    /// Updates physics calculations for this actor
    /// </summary>
    /// <param name="deltaTime">Time passed since last fixed update call</param>
    public virtual void PhysicsUpdate(float deltaTime) { }

    /// <summary>
    /// Draws this actor into the scene, must be defined in child classes
    /// </summary>
    /// <param name="sb">SpriteBatch to draw with</param>
    public abstract void Draw(SpriteBatch sb);

    /// <summary>
    /// Draws debug information for this actor
    /// </summary>
    /// <param name="sb">SpriteBatch to draw with</param>
    public virtual void DebugDraw(SpriteBatch sb) { }

    /// <summary>
    /// Executes method group for when this actor is added to a scene
    /// </summary>
    /// <param name="scene">Scene to add to</param>
    public void InvokeOnAdded(Scene scene) {
        OnAdded?.Invoke(scene);
    }

    /// <summary>
    /// Executes method group for when this actor is removed from a scene
    /// </summary>
    /// <param name="scene">Scene to remove from</param>
    public void InvokeOnRemoved(Scene scene) {
        OnRemoved?.Invoke(scene);
    }

    #endregion
}
