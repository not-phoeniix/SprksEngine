namespace Embyr;

/// <summary>
/// A 2D light object, exists in the world and shines light onto shaded places
/// </summary>
public class Light2D : Light, ITransform2D {
    /// <summary>
    /// Gets the transform for this light
    /// </summary>
    public Transform2D Transform { get; init; }

    /// <summary>
    /// Gets/sets the linear falloff of the light
    /// </summary>
    public float LinearFalloff { get; set; }

    /// <summary>
    /// Gets/sets the inner angle of light for spotlights
    /// </summary>
    public float InnerAngle { get; set; }

    /// <summary>
    /// Gets/sets the outer angle of the light for spotlights
    /// </summary>
    public float OuterAngle { get; set; }

    /// <summary>
    /// Gets/sets the radius/distance of a light in pixels
    /// </summary>
    public float Radius { get; set; }

    /// <summary>
    /// Creates a new Light with default values
    /// </summary>
    public Light2D() : base() {
        Transform = new Transform2D();
        LinearFalloff = 20;
        InnerAngle = MathF.Tau;
        OuterAngle = MathF.Tau;
        Radius = 50;
    }
}
