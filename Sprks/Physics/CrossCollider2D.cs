using Sprks.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sprks.Physics;

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
    /// Gets/sets the size of the vertical collider
    /// </summary>
    public Point VerticalSize {
        get => verticalCollider.Size;
        set => verticalCollider.Size = value;
    }

    /// <summary>
    /// Gets/sets the size of the horizontal collider
    /// </summary>
    public Point HorizontalSize {
        get => horizontalCollider.Size;
        set => horizontalCollider.Size = value;
    }

    /// <summary>
    /// Gets/sets offset of the vertical collider
    /// </summary>
    public Point VerticalOffset {
        get => verticalCollider.Offset;
        set => verticalCollider.Offset = value;
    }

    /// <summary>
    /// Gets/sets offset of the horizontal collider
    /// </summary>
    public Point HorizontalOffset {
        get => horizontalCollider.Offset;
        set => horizontalCollider.Offset = value;
    }

    /// <summary>
    /// Creates a new instance of a CrossColliderComponent2D object
    /// </summary>
    /// <param name="actor">Actor to attach collider to</param>
    internal CrossCollider2D(Actor2D actor) : base(actor) {
        this.verticalCollider = new BoxCollider2D(actor);
        this.horizontalCollider = new BoxCollider2D(actor);
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
    public override bool Contains(Vector2 point) {
        return verticalCollider.Contains(point) || horizontalCollider.Contains(point);
    }

    /// <inheritdoc/>
    public override float GetOverlappingArea(Collider2D other) {
        if (!Intersects(other)) return 0;

        if (other is BoxCollider2D box) {
            return GetOverlappingArea(box);
        }

        // cross x cross collision has yet to be implemented <//3
        // TODO: cross collider x cross collider overlapping area

        return 0;
    }

    private float GetOverlappingArea(BoxCollider2D other) {
        return verticalCollider.GetOverlappingArea(other) +
               horizontalCollider.GetOverlappingArea(other);
    }

    /// <inheritdoc/>
    public override Vector2 GetDisplacementVector(Collider2D other) {
        if (other == null || !other.Collidable) {
            return Vector2.Zero;
        }

        if (other is BoxCollider2D rect) {
            return GetDisplacementVector(rect);
        }

        // cross x cross collision has yet to be implemented <//3
        // TODO: cross collider x cross collider displacement

        return Vector2.Zero;
    }

    private Vector2 GetDisplacementVector(BoxCollider2D other) {
        bool horizCollision = other.Intersects(horizontalCollider);
        bool vertCollision = other.Intersects(verticalCollider);

        if (vertCollision) {
            float yMin = MathF.Max(verticalCollider.Min.Y, other.Min.Y);
            float yMax = MathF.Min(verticalCollider.Max.Y, other.Max.Y);

            // invert offset if this collider above
            //   the other collider
            // Vector2 displacement = new(0, MathF.Ceiling(yMax - yMin));
            Vector2 displacement = new(0, yMax - yMin);
            if (Min.Y < other.Min.Y) {
                displacement.Y *= -1;
            }

            return displacement;
        }

        if (horizCollision) {
            float xMin = MathF.Max(horizontalCollider.Min.X, other.Min.X);
            float xMax = MathF.Min(horizontalCollider.Max.X, other.Max.X);

            // invert offset if this collider is to the
            //   left of the other collider
            // Vector2 displacement = new(MathF.Ceiling(xMax - xMin), 0);
            Vector2 displacement = new(xMax - xMin, 0);
            if (Actor.Transform.GlobalPosition.X < other.Actor.Transform.GlobalPosition.X) {
                displacement.X *= -1;
            }

            return displacement;
        }

        return Vector2.Zero;
    }

    /// <inheritdoc/>
    public override void DebugDraw(SpriteBatch sb) {
        horizontalCollider.DebugDraw(sb);
        verticalCollider.DebugDraw(sb);
    }
}
