using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Embyr.Scenes;

/// <summary>
/// An abstract base class that implements all <c>IActor</c> members.
/// </summary>
public abstract class Actor2D : IActor, ITransform2D, IDrawable2D, IDebugDrawable2D {
    private readonly List<ActorComponent2D> components;

    /// <summary>
    /// Gets the transform of this actor
    /// </summary>
    public Transform2D Transform { get; init; }

    /// <summary>
    /// Gets the scene this actor is created within
    /// </summary>
    public Scene Scene { get; }

    /// <summary>
    /// Gets/sets the name of this actor
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets/sets the action called when this actor is added to a scene
    /// </summary>
    public event Action<Scene>? OnAdded;

    /// <summary>
    /// Event called when this actor is removed from a scene
    /// </summary>
    public event Action<Scene>? OnRemoved;

    /// <summary>
    /// Creates a new Actor2D object
    /// </summary>
    /// <param name="position">Initial position</param>
    /// <param name="scene">Scene to place this actor in</param>
    public Actor2D(
        Vector2 position,
        Scene2D scene
    ) {
        this.Transform = new Transform2D(position);
        this.Name = GetType().Name;
        this.Scene = scene;
        components = new List<ActorComponent2D>();
    }

    /// <summary>
    /// Adds a component to this actor
    /// </summary>
    /// <typeparam name="T">Type of component to add</typeparam>
    /// <returns>Reference to newly created component</returns>
    protected T AddComponent<T>() where T : ActorComponent2D {
        ConstructorInfo? ctor = typeof(T).GetConstructor(
            BindingFlags.NonPublic | BindingFlags.Instance,
            [typeof(Actor2D)]
        );
        if (ctor == null) {
            throw new Exception("Component does not have a valid constructor, cannot add component to actor!");
        }

        T? component = ctor.Invoke([this]) as T;
        if (component == null) {
            throw new NullReferenceException("Created component returned null, cannot add component to actor!");
        }

        components.Add(component);

        return component;
    }

    /// <summary>
    /// Gets the first component of this actor of a given type
    /// </summary>
    /// <typeparam name="T">Type of component to get</typeparam>
    /// <returns>Reference to first component of a given type, null if not found</returns>
    public T? GetComponent<T>() where T : ActorComponent2D {
        foreach (ActorComponent2D c in components) {
            if (c is T t) return t;
        }

        return null;
    }

    /// <summary>
    /// Updates general logic for this actor
    /// </summary>
    /// <param name="deltaTime">Time passed since last frame</param>
    public virtual void Update(float deltaTime) {
        foreach (ActorComponent2D c in components) {
            c.Update(deltaTime);
        }
    }

    /// <summary>
    /// Updates physics calculations for this actor
    /// </summary>
    /// <param name="deltaTime">Time passed since last physics update</param>
    public virtual void PhysicsUpdate(float deltaTime) {
        foreach (ActorComponent2D c in components) {
            c.PhysicsUpdate(deltaTime);
        }
    }

    /// <summary>
    /// Draws this actor into the scene
    /// </summary>
    /// <param name="sb">SpriteBatch to draw with</param>
    public virtual void Draw(SpriteBatch sb) {
        foreach (ActorComponent2D c in components) {
            c.Draw(sb);
        }
    }

    /// <summary>
    /// Draws debug information for this actor
    /// </summary>
    /// <param name="sb">SpriteBatch to draw with</param>
    public virtual void DebugDraw(SpriteBatch sb) {
        foreach (ActorComponent2D c in components) {
            c.DebugDraw(sb);
        }
    }

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
}
