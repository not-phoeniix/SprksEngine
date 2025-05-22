using Embyr.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Embyr.Physics;

/// <summary>
/// Abstract parent representation of a 2D physics collider
/// </summary>
public abstract class Collider2D : IDebugDrawable2D {
    private readonly List<Collider2D> children;

    /// <summary>
    /// Gets the reference to the parent of this collider
    /// </summary>
    public Collider2D? Parent { get; private set; }

    /// <summary>
    /// Gets the minimum bounding coordinates of this collider as a Vector2
    /// </summary>
    public abstract Vector2 Min { get; }

    /// <summary>
    /// Gets the maximum bounding coordinates of this collider as a Vector2
    /// </summary>
    public abstract Vector2 Max { get; }

    /// <summary>
    /// Gets the transform associated with this collider
    /// </summary>
    protected Transform2D Transform { get; }

    /// <summary>
    /// Gets/sets whether or not this collider can be collided with
    /// </summary>
    public bool Collidable { get; set; }

    /// <summary>
    /// Creates a new instance of a collider object
    /// </summary>
    /// <param name="transform">Transform to associate collider with</param>
    public Collider2D(Transform2D transform) {
        this.Transform = transform;
        this.children = new List<Collider2D>();
        this.Collidable = true;
    }

    /// <summary>
    /// Adds a child to this collider
    /// </summary>
    /// <param name="child">Child to add to this collider</param>
    public void AddChild(Collider2D child) {
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
    public bool RemoveChild(Collider2D child) {
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
    public Collider2D? GetMostSpecificCollidingChild(Collider2D other) {
        if (Intersects(other)) {
            if (children.Count != 0) {
                foreach (Collider2D child in children) {
                    Collider2D? collision = child.GetMostSpecificCollidingChild(other);
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
    public abstract bool Intersects(Collider2D other);

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
    public abstract Vector2 GetDisplacementVector(Collider2D other);

    /// <summary>
    /// Draws debug information for this collider
    /// </summary>
    /// <param name="sb">SpriteBatch to draw with</param>
    public abstract void DebugDraw(SpriteBatch sb);
}
