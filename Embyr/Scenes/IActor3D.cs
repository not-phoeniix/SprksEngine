using Microsoft.Xna.Framework;

namespace Embyr.Scenes;

/// <summary>
/// Interface that represents an actor in a 3D scene, implements ITransform3D
/// </summary>
public interface IActor3D : IActor, ITransform3D, IDrawable3D {
    /// <summary>
    /// Gets the bounds of this actor
    /// </summary>
    public BoundingBox Bounds { get; }
}
