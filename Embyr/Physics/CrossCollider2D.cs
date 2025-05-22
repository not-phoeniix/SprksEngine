using Embyr.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Embyr.Physics;

public class CrossCollider2D : Collider2D {
    private readonly RectCollider2D verticalCollider;
    private readonly RectCollider2D horizontalCollider;

    public override Vector2 Min => Vector2.Min(verticalCollider.Min, horizontalCollider.Min);
    public override Vector2 Max => Vector2.Max(verticalCollider.Max, horizontalCollider.Max);

    public Vector2 VerticalSize {
        get => verticalCollider.Size;
        set => verticalCollider.Size = value;
    }

    public Vector2 HorizontalSize {
        get => horizontalCollider.Size;
        set => horizontalCollider.Size = value;
    }

    public CrossCollider2D(IActor2D actor, Vector2 verticalSize, Vector2 horizontalSize)
    : base(actor.Transform) {
        this.verticalCollider = new RectCollider2D(actor, verticalSize);
        this.horizontalCollider = new RectCollider2D(actor, horizontalSize);
    }

    public override bool Intersects(Collider2D other) {
        if (other is RectCollider2D rect) {
            return Intersects(rect);
        }

        if (other is CrossCollider2D cross) {
            return Intersects(cross);
        }

        return false;
    }

    private bool Intersects(RectCollider2D other) {
        return verticalCollider.Intersects(other) || horizontalCollider.Intersects(other);
    }

    private bool Intersects(CrossCollider2D other) {
        return verticalCollider.Intersects(other.verticalCollider) ||
               horizontalCollider.Intersects(other.verticalCollider) ||
               verticalCollider.Intersects(other.horizontalCollider) ||
               horizontalCollider.Intersects(other.horizontalCollider);
    }

    public override bool Intersects(Rectangle other) {
        return verticalCollider.Intersects(other) || horizontalCollider.Intersects(other);
    }

    public override Vector2 GetDisplacementVector(Collider2D other) {
        other = other.GetMostSpecificCollidingChild(this);

        if (other == null || !other.Collidable) {
            return Vector2.Zero;
        }

        if (other is RectCollider2D rect) {
            return GetDisplacementVector(rect);
        }

        return Vector2.Zero;
    }

    private Vector2 GetDisplacementVector(RectCollider2D other) {
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

    public override void DebugDraw(SpriteBatch sb) {
        horizontalCollider.DebugDraw(sb);
        verticalCollider.DebugDraw(sb);
    }
}
