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
    public Point Size { get; set; }

    /// <summary>
    /// Gets/sets the center offset of this box collider
    /// </summary>
    public Point Offset { get; set; }

    /// <inheritdoc/>
    public override Vector2 Min {
        get {
            return Actor.Transform.GlobalPosition - (Size.ToVector2() / 2) + Offset.ToVector2();
        }
    }

    /// <inheritdoc/>
    public override Vector2 Max {
        get {
            return Actor.Transform.GlobalPosition + (Size.ToVector2() / 2) + Offset.ToVector2();
        }
    }

    /// <summary>
    /// Creates a new BoxColliderComponent2D instance
    /// </summary>
    /// <param name="actor">Actor to attach to collider</param>
    internal BoxCollider2D(Actor2D actor) : base(actor) {
        this.Size = new Point(2, 2);
        this.Offset = Point.Zero;
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
        Vector2 min = Min;
        Vector2 max = Max;
        Vector2 otherMin = other.Min;
        Vector2 otherMax = other.Max;

        return min.X <= otherMax.X &&
               min.Y <= otherMax.Y &&
               max.X >= otherMin.X &&
               max.Y >= otherMin.Y;
    }

    /// <inheritdoc/>
    public override bool Intersects(Rectangle other) {
        Vector2 min = Min;
        Vector2 max = Max;

        return min.X <= other.Right &&
               min.Y <= other.Bottom &&
               max.X >= other.Left &&
               max.Y >= other.Top;
    }

    /// <inheritdoc/>
    public override bool Contains(Vector2 point) {
        Vector2 min = Min;
        Vector2 max = Max;

        return point.X >= min.X &&
               point.Y >= min.Y &&
               point.X <= max.X &&
               point.Y <= max.Y;
    }

    /// <inheritdoc/>
    public override float GetOverlappingArea(Collider2D other) {
        if (!Intersects(other)) return 0;

        if (other is BoxCollider2D box) {
            return GetOverlappingArea(box);
        }

        if (other is CrossCollider2D cross) {
            return cross.GetOverlappingArea(this);
        }

        return 0;
    }

    private float GetOverlappingArea(BoxCollider2D other) {
        Vector2 overlap = GetOverlappingSize(other);
        return overlap.X * overlap.Y;
    }

    private Vector2 GetOverlappingSize(BoxCollider2D other) {
        if (!Intersects(other)) return Vector2.Zero;

        Vector2 min = Min;
        Vector2 max = Max;
        Vector2 otherMin = other.Min;
        Vector2 otherMax = other.Max;

        Vector2 overlapMax = Vector2.Min(max, otherMax);
        Vector2 overlapMin = Vector2.Max(min, otherMin);

        return overlapMax - overlapMin;
    }

    /// <inheritdoc/>
    public override Vector2 GetDisplacementVector(Collider2D other) {
        if (other == null || !other.Collidable) {
            return Vector2.Zero;
        }

        if (other is BoxCollider2D rect) {
            return GetDisplacementVector(rect);
        }

        if (other is CrossCollider2D cross) {
            return cross.GetDisplacementVector(this);
        }

        return Vector2.Zero;
    }

    private Vector2 GetDisplacementVector(BoxCollider2D other) {
        Vector2 overlap = GetOverlappingSize(other);

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

    /// <inheritdoc/>
    public override void DebugDraw(SpriteBatch sb) {
        sb.DrawRectOutline(
            new Rectangle(
                Vector2.Floor(Min).ToPoint(),
                Vector2.Floor(Max - Min).ToPoint()
            ),
            1,
            Color.Red
        );
    }
}
