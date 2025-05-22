using Embyr.Physics;
using Microsoft.Xna.Framework;

namespace Embyr.Scenes;

/// <summary>
/// Interface that represents an actor in a 2D scene, implements ITransform2D
/// </summary>
public interface IActor2D : IActor, ITransform2D, IDrawable2D {
    /// <summary>
    /// Gets the bounds of this actor
    /// </summary>
    public Collider2D Collider { get; }
}
