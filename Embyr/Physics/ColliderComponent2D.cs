using Embyr.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Embyr.Physics;

/// <summary>
/// Abstract parent representation of a 2D physics collider, inherits from ActorComponent2D
/// </summary>
public abstract class ColliderComponent2D : ActorComponent2D {
    private readonly List<ColliderComponent2D> children;

    protected static readonly float CollisionTolerance = 0.001f;

    /// <summary>
    /// Gets the reference to the parent of this collider
    /// </summary>
    public ColliderComponent2D? Parent { get; private set; }

    /// <summary>
    /// Gets the minimum bounding coordinates of this collider as a Vector2
    /// </summary>
    public abstract Vector2 Min { get; }

    /// <summary>
    /// Gets the maximum bounding coordinates of this collider as a Vector2
    /// </summary>
    public abstract Vector2 Max { get; }

    /// <summary>
    /// Gets/sets whether or not this collider can be collided with
    /// </summary>
    public bool Collidable { get; set; }

    /// <summary>
    /// Creates a new instance of a collider object
    /// </summary>
    /// <param name="transform">Transform to associate collider with</param>
    internal ColliderComponent2D(Actor2D actor) : base(actor) {
        this.children = new List<ColliderComponent2D>();
        this.Collidable = true;
    }

    /// <summary>
    /// Adds a child to this collider
    /// </summary>
    /// <param name="child">Child to add to this collider</param>
    public void AddChild(ColliderComponent2D child) {
        if (!children.Contains(child)) {
            child.RemoveFromParent();
            children.Add(child);
            child.Parent = this;
        }
    }

    /// <summary>
    /// Removes a child from this collider's children
    /// </summary>
    /// <param name="child">Child to remove</param>
    /// <returns>True if child was successfully removed, false if otherwise</returns>
    public bool RemoveChild(ColliderComponent2D child) {
        if (children.Remove(child)) {
            child.Parent = null;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Removes this collider from its parent
    /// </summary>
    public void RemoveFromParent() {
        Parent?.RemoveChild(this);
    }

    /// <summary>
    /// Gets the reference to the most specific colliding child collider in the hierarchy of this collider, can return null if not colliding
    /// </summary>
    /// <param name="other">Other collider to check collisions with</param>
    /// <returns>Reference to most specific colliding child, null if no collisions occur</returns>
    public ColliderComponent2D? GetMostSpecificCollidingChild(ColliderComponent2D other) {
        if (Intersects(other)) {
            if (children.Count != 0) {
                foreach (ColliderComponent2D child in children) {
                    ColliderComponent2D? collision = child.GetMostSpecificCollidingChild(other);
                    if (collision != null) {
                        return collision;
                    }
                }
            }

            if (Collidable) {
                return this;
            }
        }

        return null;
    }

    /// <summary>
    /// Gets whether or not an intersection occured with just this top-level collider, does not check children and ignores "Collidable"
    /// </summary>
    /// <param name="other">Collider to check if intersecting</param>
    /// <returns>Whether or not an intersection exists</returns>
    public abstract bool Intersects(ColliderComponent2D other);

    /// <summary>
    /// Gets whether or not an intersection occured with just this top-level collider, does not check children and ignores "Collidable"
    /// </summary>
    /// <param name="other">Rectangle to check if intersecting</param>
    /// <returns>Whether or not an intersection exists</returns>
    public abstract bool Intersects(Rectangle other);

    /// <summary>
    /// Gets the displacement vector of two overlapping colliders, used for collision resolution
    /// </summary>
    /// <param name="other">Other collider to displace away from</param>
    /// <returns>Displacement Vector2 to separate this collider from the other</returns>
    public abstract Vector2 GetDisplacementVector(ColliderComponent2D other);

    /// <inheritdoc/>
    public override sealed void Update(float deltaTime) { }

    /// <inheritdoc/>
    public override sealed void PhysicsUpdate(float deltaTime) { }

    /// <inheritdoc/>
    public override sealed void Draw(SpriteBatch sb) { }
}
