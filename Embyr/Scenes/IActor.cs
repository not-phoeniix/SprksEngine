using Microsoft.Xna.Framework;

namespace Embyr.Scenes;

/// <summary>
/// Interface that represents an actor object within the world, depends on IDrawable
/// </summary>
public interface IActor : IDrawable, ITransform {
    /// <summary>
    /// Event called when actor is removed from scene
    /// </summary>
    public event Action<Scene> OnRemoved;

    /// <summary>
    /// Event called when actor is added to the scene
    /// </summary>
    public event Action<Scene> OnAdded;

    /// <summary>
    /// Gets the bounds of this actor
    /// </summary>
    public Rectangle Bounds { get; }

    /// <summary>
    /// Gets/sets reference to scene actor is spawned in
    /// </summary>
    public Scene Scene { get; }

    /// <summary>
    /// Gets/sets the name of this actor in the scene
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets whether or not this actor should be saved to world file
    /// </summary>
    public bool ShouldBeSaved { get; }

    /// <summary>
    /// Updates this actor within a scene
    /// </summary>
    /// <param name="dt">Time passed since last frame</param>
    public void Update(float dt);

    /// <summary>
    /// Updates the fixed timestep physics of this actor within a scene
    /// </summary>
    /// <param name="dt">Fixed delta time that has passed since last PhysicsUpdate</param>
    public void PhysicsUpdate(float dt);

    /// <summary>
    /// Executes methods to call when actor is removed from scene
    /// </summary>
    /// <param name="scene">Scene being removed from</param>
    public void InvokeOnRemoved(Scene scene);

    /// <summary>
    /// Executes methods to call when actor is added to scene
    /// </summary>
    /// <param name="scene">Scene being added to</param>
    public void InvokeOnAdded(Scene scene);
}
