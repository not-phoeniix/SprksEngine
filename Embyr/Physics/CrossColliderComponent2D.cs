using Embyr.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Embyr.Physics;

/// <summary>
/// A 2D collider that has separated vertical and horizontal rectangle colliders, useful for platformer player controllers. Inherits from Collider2D.
/// </summary>
public class CrossColliderComponent2D : ColliderComponent2D {
    private readonly BoxColliderComponent2D verticalCollider;
    private readonly BoxColliderComponent2D horizontalCollider;

    /// <inheritdoc/>
    public override Vector2 Min => Vector2.Min(verticalCollider.Min, horizontalCollider.Min);

    /// <inheritdoc/>
    public override Vector2 Max => Vector2.Max(verticalCollider.Max, horizontalCollider.Max);

    /// <summary>
    /// Gets the size of the vertical collider
    /// </summary>
    public Vector2 VerticalSize {
        get => verticalCollider.Size;
        set => verticalCollider.Size = value;
    }

    /// <summary>
    /// Gets the size of the horizontal collider
    /// </summary>
    public Vector2 HorizontalSize {
        get => horizontalCollider.Size;
        set => horizontalCollider.Size = value;
    }

    /// <summary>
    /// Creates a new instance of a CrossColliderComponent2D object
    /// </summary>
    /// <param name="actor">Actor to attach collider to</param>
    public CrossColliderComponent2D(Actor2D actor) : base(actor) {
        this.verticalCollider = new BoxColliderComponent2D(actor);
        this.horizontalCollider = new BoxColliderComponent2D(actor);
    }

    /// <inheritdoc/>
    public override bool Intersects(ColliderComponent2D other) {
        if (other is BoxColliderComponent2D rect) {
            return Intersects(rect);
        }

        if (other is CrossColliderComponent2D cross) {
            return Intersects(cross);
        }

        return false;
    }

    private bool Intersects(BoxColliderComponent2D other) {
        return verticalCollider.Intersects(other) || horizontalCollider.Intersects(other);
    }

    private bool Intersects(CrossColliderComponent2D other) {
        return verticalCollider.Intersects(other.verticalCollider) ||
               horizontalCollider.Intersects(other.verticalCollider) ||
               verticalCollider.Intersects(other.horizontalCollider) ||
               horizontalCollider.Intersects(other.horizontalCollider);
    }

    /// <inheritdoc/>
    public override bool Intersects(Rectangle other) {
        return verticalCollider.Intersects(other) || horizontalCollider.Intersects(other);
    }

    /// <inheritdoc/>
    public override Vector2 GetDisplacementVector(ColliderComponent2D other) {
        other = other.GetMostSpecificCollidingChild(this);

        if (other == null || !other.Collidable) {
            return Vector2.Zero;
        }

        if (other is BoxColliderComponent2D rect) {
            return GetDisplacementVector(rect);
        }

        return Vector2.Zero;
    }

    private Vector2 GetDisplacementVector(BoxColliderComponent2D other) {
        bool horizCollision = horizontalCollider.Intersects(other);
        bool vertCollision = verticalCollider.Intersects(other);

        Vector2 displacement = Vector2.Zero;

        if (vertCollision) {
            float yMin = MathF.Max(verticalCollider.Min.Y, other.Min.Y);
            float yMax = MathF.Min(verticalCollider.Max.Y, other.Max.Y);

            // invert offset if this collider above
            //   the other collider
            displacement.Y = yMax - yMin;
            if (Min.Y < other.Min.Y) {
                displacement.Y *= -1;
            }
        }

        if (horizCollision) {
            float xMin = MathF.Max(horizontalCollider.Min.X, other.Min.X);
            float xMax = MathF.Min(horizontalCollider.Max.X, other.Max.X);

            // invert offset if this collider is to the
            //   left of the other collider
            displacement.X = xMax - xMin;
            if (Min.X < other.Min.X) {
                displacement.X *= -1;
            }
        }

        return displacement;
    }

    /// <inheritdoc/>
    public override void DebugDraw(SpriteBatch sb) {
        horizontalCollider.DebugDraw(sb);
        verticalCollider.DebugDraw(sb);
    }
}
