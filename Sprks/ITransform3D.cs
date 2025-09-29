namespace Sprks;

/// <summary>
/// Interface describing an object that contains a 3D transform, most base-level object in the engine
/// </summary>
public interface ITransform3D {
    /// <summary>
    /// Gets the transform for this object
    /// </summary>
    public Transform3D Transform { get; }
}
