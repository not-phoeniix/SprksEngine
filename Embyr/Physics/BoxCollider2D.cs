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
        return GetAsRect().Intersects(other.GetAsRect());
    }

    /// <inheritdoc/>
    public override bool Intersects(Rectangle other) {
        return GetAsRect().Intersects(other);
    }

    /// <inheritdoc/>
    public override bool Contains(Vector2 point) {
        return GetAsRect().Contains(point);
    }

    /// <inheritdoc/>
    public override Vector2 GetDisplacementVector(Collider2D other) {
        if (other == null || !other.Collidable) {
            return Vector2.Zero;
        }

        if (other is BoxCollider2D rect) {
            return GetDisplacementVector(rect);
        }

        return Vector2.Zero;
    }

    private Vector2 GetDisplacementVector(BoxCollider2D other) {
        Vector2 overlap = Rectangle.Intersect(
            GetAsRect(),
            other.GetAsRect()
        ).Size.ToVector2();

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
        sb.DrawRectOutline(GetAsRect(), 1, Color.Red);
    }

    /// <summary>
    /// Gets a rectangle representation of this box collider
    /// </summary>
    /// <returns>Rectangle box collider</returns>
    internal Rectangle GetAsRect() {
        return new Rectangle(
            Vector2.Floor(Min).ToPoint(),
            Vector2.Ceiling(Max - Min).ToPoint()
        );
    }
}
