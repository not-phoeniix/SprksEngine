using Embyr.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Embyr.Physics;

public abstract class Collider2D : IDebugDrawable2D {
    private readonly List<Collider2D> children;

    public Collider2D? Parent { get; private set; }

    public abstract Vector2 Min { get; }
    public abstract Vector2 Max { get; }

    protected Transform2D Transform { get; }

    public bool Collidable { get; set; }

    public Collider2D(Transform2D transform) {
        this.Transform = transform;
        this.children = new List<Collider2D>();
        this.Collidable = true;
    }

    public void AddChild(Collider2D child) {
        if (!children.Contains(child)) {
            child.RemoveFromParent();
            children.Add(child);
            child.Parent = this;
        }
    }

    public bool RemoveChild(Collider2D child) {
        if (children.Remove(child)) {
            child.Parent = null;
            return true;
        }

        return false;
    }

    public void RemoveFromParent() {
        Parent?.RemoveChild(this);
    }

    public bool CollidesWithRecursive(Collider2D other) {
        return GetMostSpecificCollidingChild(other) != null;
    }

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

    public abstract bool Intersects(Collider2D other);
    public abstract bool Intersects(Rectangle other);
    public abstract Vector2 GetDisplacementVector(Collider2D other);
    public abstract void DebugDraw(SpriteBatch sb);
}
