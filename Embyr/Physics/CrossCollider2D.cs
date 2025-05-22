using Embyr.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Embyr.Physics;

/// <summary>
/// A 2D collider that has separated vertical and horizontal rectangle colliders, useful for platformer player controllers. Inherits from Collider2D.
/// </summary>
public class CrossCollider2D : Collider2D {
    private readonly BoxCollider2D verticalCollider;
    private readonly BoxCollider2D horizontalCollider;

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
    /// Creates a new instance of a CrossCollider2D object
    /// </summary>
    /// <param name="actor">Actor to attach collider to</param>
    /// <param name="verticalSize">Size of vertical rectangle collider</param>
    /// <param name="horizontalSize">Size of the horizontal rectangle collider</param>
    public CrossCollider2D(IActor2D actor, Vector2 verticalSize, Vector2 horizontalSize)
    : base(actor.Transform) {
        this.verticalCollider = new BoxCollider2D(actor, verticalSize);
        this.horizontalCollider = new BoxCollider2D(actor, horizontalSize);
    }

    /// <inheritdoc/>
    public override bool Intersects(Collider2D other) {
        if (other is BoxCollider2D rect) {
            return Intersects(rect);
        }

        if (other is CrossCollider2D cross) {
            return Intersects(cross);
        }

        return false;
    }

    private bool Intersects(BoxCollider2D other) {
        return verticalCollider.Intersects(other) || horizontalCollider.Intersects(other);
    }

    private bool Intersects(CrossCollider2D other) {
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
    public override Vector2 GetDisplacementVector(Collider2D other) {
        other = other.GetMostSpecificCollidingChild(this);

        if (other == null || !other.Collidable) {
            return Vector2.Zero;
        }

        if (other is BoxCollider2D rect) {
            return GetDisplacementVector(rect);
        }

        return Vector2.Zero;
    }

    private Vector2 GetDisplacementVector(BoxCollider2D other) {
        bool horizCollision = horizontalCollider.Intersects(other);
        bool vertCollision = verticalCollider.Intersects(other);

        Vector2 displacement = Vector2.Zero;

        if (vertCollision) {
            float yMin = MathF.Max(horizontalCollider.Min.Y, other.Min.Y);
            float yMax = MathF.Min(horizontalCollider.Max.Y, other.Max.Y);

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
