namespace Sprks;

/// <summary>
/// Interface describing an object that contains a 2D transform, most base-level object in the engine
/// </summary>
public interface ITransform2D {
    /// <summary>
    /// Gets the transform for this object
    /// </summary>
    public Transform2D Transform { get; }
}
