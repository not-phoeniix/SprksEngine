using Microsoft.Xna.Framework;

namespace Embyr.Scenes;

/// <summary>
/// Interface that represents an actor in a 2D scene, implements ITransform2D
/// </summary>
public interface IActor2D : IActor, ITransform2D {
    /// <summary>
    /// Gets the bounds of this actor
    /// </summary>
    public Rectangle Bounds { get; }
}
