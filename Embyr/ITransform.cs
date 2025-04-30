namespace Embyr;

/// <summary>
/// Interface describing an object that contains a transform, most base-level object in the engine
/// </summary>
public interface ITransform {
    /// <summary>
    /// Gets the transform for this object
    /// </summary>
    public Transform Transform { get; }
}
