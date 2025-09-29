using Sprks.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sprks.Physics;

/// <summary>
/// Abstract parent representation of a 2D physics collider, inherits from ActorComponent2D
/// </summary>
public abstract class Collider2D : ActorComponent2D {
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
    /// Gets/sets whether or not this collider can be collided with
    /// </summary>
    public bool Collidable { get; set; }

    /// <summary>
    /// Creates a new instance of a collider object
    /// </summary>
    /// <param name="actor">Actor to attach collider to</param>
    internal Collider2D(Actor2D actor) : base(actor) {
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
    /// <returns>Reference to most specific colliding child, null if no collisions occur or component is disabled</returns>
    public virtual Collider2D? GetMostSpecificCollidingChild(Collider2D other) {
        // no collisions occur if component is disabled or
        //   doesn't immediately intersect with other collider
        if (!Enabled || !Intersects(other)) return null;

        if (children.Count != 0) {
            Collider2D? largestAreaCollider = null;
            float largestArea = 0;

            foreach (Collider2D child in children) {
                float area = child.GetOverlappingArea(other);

                if (area > largestArea) {
                    largestArea = area;
                    largestAreaCollider = child;
                }
            }

            if (largestAreaCollider != null) {
                return largestAreaCollider.GetMostSpecificCollidingChild(other) ?? largestAreaCollider;
            }
        }

        if (Collidable) {
            return this;
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
    /// Gets whether or not this collider contains a point
    /// </summary>
    /// <param name="point">Point to check if lies within</param>
    /// <returns>Whether or not this collider contains the point</returns>
    public abstract bool Contains(Vector2 point);

    /// <summary>
    /// Gets the overlap area between this and another collider
    /// </summary>
    /// <param name="other">Other collider to get overlapping area from</param>
    /// <returns>Overlapping area as a float</returns>
    public abstract float GetOverlappingArea(Collider2D other);

    /// <summary>
    /// Gets the displacement vector of two overlapping colliders, used for collision resolution
    /// </summary>
    /// <param name="other">Other collider to displace away from</param>
    /// <returns>Displacement Vector2 to separate this collider from the other</returns>
    public abstract Vector2 GetDisplacementVector(Collider2D other);

    /// <inheritdoc/>
    public override sealed void Update(float deltaTime) { }

    /// <inheritdoc/>
    public override sealed void PhysicsUpdate(float deltaTime) { }

    /// <inheritdoc/>
    public override sealed void Draw(SpriteBatch sb) { }
}
