using Embyr.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Embyr.Physics;

/// <summary>
/// A 2D collider in a box shape, utilizes AABB, good for general fast collisions. Inherits from Collider2D.
/// </summary>
public class BoxCollider2D : Collider2D {
    /// <summary>
    /// Gets/sets the size of this box collider
    /// </summary>
    public Vector2 Size { get; set; }

    /// <inheritdoc/>
    public override Vector2 Min => Transform.GlobalPosition - (Size / 2);

    /// <inheritdoc/>
    public override Vector2 Max => Transform.GlobalPosition + (Size / 2);

    /// <summary>
    /// Creates a new BoxCollider2D instance
    /// </summary>
    /// <param name="actor">Actor to attach to collider</param>
    /// <param name="size">The size of the collider</param>
    public BoxCollider2D(IActor2D actor, Vector2 size)
    : base(actor.Transform) {
        this.Size = size;
    }

    /// <summary>
    /// Creates a new BoxCollider2D instance
    /// </summary>
    /// <param name="transform">Transform to attach to collider</param>
    /// <param name="size">The size of the collider</param>
    public BoxCollider2D(Transform2D transform, Vector2 size)
    : base(transform) {
        this.Size = size;
    }

    /// <inheritdoc/>
    public override bool Intersects(Collider2D other) {
        if (other is BoxCollider2D rect) {
            return Intersects(rect);
        }

        if (other is CrossCollider2D cross) {
            return cross.Intersects(this);
        }

        return false;
    }

    private bool Intersects(BoxCollider2D other) {
        Vector2 thisMin = Transform.GlobalPosition - Size / 2;
        Vector2 thisMax = Transform.GlobalPosition + Size / 2;
        Vector2 otherMin = other.Transform.GlobalPosition - other.Size / 2;
        Vector2 otherMax = other.Transform.GlobalPosition + other.Size / 2;

        return thisMin.X + CollisionTolerance <= otherMax.X &&
               thisMin.Y + CollisionTolerance <= otherMax.Y &&
               thisMax.X - CollisionTolerance >= otherMin.X &&
               thisMax.Y - CollisionTolerance >= otherMin.Y;
    }

    /// <inheritdoc/>
    public override bool Intersects(Rectangle other) {
        Vector2 thisMin = Transform.GlobalPosition - Size / 2;
        Vector2 thisMax = Transform.GlobalPosition + Size / 2;
        Vector2 otherMin = new(other.Left, other.Top);
        Vector2 otherMax = new(other.Right, other.Bottom);

        return thisMin.X <= otherMax.X &&
               thisMin.Y <= otherMax.Y &&
               thisMax.X >= otherMin.X &&
               thisMax.Y >= otherMin.Y;
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
        Vector2 overlap = GetOverlapSize(other);

        if (overlap.X >= overlap.Y) {
            // ~~~ vertical displacement ~~~

            overlap.X = 0;

            // if top edge is above of other's top edge,
            //   offset upwards, otherwise offset down
            if (Min.Y < other.Min.Y) {
                overlap.Y *= -1;
            }

        } else {
            // ~~~ resolve collisions horizontally ~~~

            overlap.Y = 0;

            // if left edge is to the left of other's left edge,
            //   offset to the left, otherwise offset right
            if (Min.X < other.Min.X) {
                overlap.X *= -1;
            }
        }

        return overlap;
    }

    // effectively returning the size of the union of two
    //   rect colliders, similar to Rectangle.Union
    private Vector2 GetOverlapSize(BoxCollider2D other) {
        Vector2 thisMin = Min;
        Vector2 thisMax = Max;
        Vector2 otherMin = other.Min;
        Vector2 otherMax = other.Max;

        float xMin = MathF.Max(thisMin.X, otherMin.X);
        float yMin = MathF.Max(thisMin.Y, otherMin.Y);
        float xMax = MathF.Min(thisMax.X, otherMax.X);
        float yMax = MathF.Min(thisMax.Y, otherMax.Y);

        return new Vector2(xMax - xMin, yMax - yMin);
    }

    /// <inheritdoc/>
    public override void DebugDraw(SpriteBatch sb) {
        sb.DrawRectOutline(
            new Rectangle(
                Vector2.Floor(Min).ToPoint(),
                Vector2.Ceiling(Size).ToPoint()
            ),
            1,
            Color.Red
        );
    }
}
