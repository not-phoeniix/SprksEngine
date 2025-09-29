namespace Sprks;

/// <summary>
/// A 3D light object, exists in the world and shines light onto shaded places
/// </summary>
public class Light3D : Light, ITransform3D {
    /// <summary>
    /// Gets the transform for this light
    /// </summary>
    public Transform3D Transform { get; init; }

    /// <summary>
    /// Gets/sets linear range of this light
    /// </summary>
    public float Range { get; set; }

    /// <summary>
    /// Gets/sets the inner angle of a spotlight
    /// </summary>
    public float SpotInnerAngle { get; set; }

    /// <summary>
    /// Gets/sets the outer angle of a spotlight
    /// </summary>
    public float SpotOuterAngle { get; set; }

    /// <summary>
    /// Creates a new Light with default values
    /// </summary>
    public Light3D() : base() {
        Transform = new Transform3D();
        Range = 100;
        SpotInnerAngle = MathF.PI * 2;
        SpotOuterAngle = MathF.PI * 2;
    }
}
