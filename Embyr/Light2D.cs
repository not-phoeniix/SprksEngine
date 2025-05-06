using Embyr.Scenes;
using Microsoft.Xna.Framework;

namespace Embyr;

/// <summary>
/// A light object, exists in the world and shines light onto shaded places
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
    /// Gets/sets the angular width of light (in radians)
    /// </summary>
    public float AngularWidth { get; set; }

    /// <summary>
    /// Gets/sets the angular falloff of the light
    /// </summary>
    public float AngularFalloff { get; set; }

    /// <summary>
    /// Gets/sets the rotation of the light
    /// </summary>
    public float Rotation { get; set; }

    /// <summary>
    /// Gets/sets the radius/distance of a light in pixels
    /// </summary>
    public float Radius { get; set; }

    /// <summary>
    /// Creates a new Light with default values
    /// </summary>
    public Light2D() {
        Transform = new Transform2D();
        LinearFalloff = 20;
        AngularWidth = MathF.PI * 2;
        AngularFalloff = 0;
        Radius = 50;
        Rotation = 0;
    }
}
