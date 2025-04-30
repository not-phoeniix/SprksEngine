using Embyr.Scenes;
using Microsoft.Xna.Framework;

namespace Embyr;

/// <summary>
/// A light object, exists in the world and shines light onto shaded places
/// </summary>
public class Light : ITransform {
    /// <summary>
    /// Gets/sets whether or not this light is enabled or not
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets whether or not this light is a global sun light
    /// </summary>
    public bool IsGlobal { get; init; }

    /// <summary>
    /// Gets the transform for this light
    /// </summary>
    public Transform Transform { get; init; }

    /// <summary>
    /// Gets/sets the color of light
    /// </summary>
    public Color Color { get; set; }

    /// <summary>
    /// Gets/sets the intensity/radius of light in pixels
    /// </summary>
    public float Intensity { get; set; }

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
    public Light() {
        Transform = new Transform();
        Color = Color.White;
        Intensity = 1;
        LinearFalloff = 20;
        AngularWidth = MathF.PI * 2;
        AngularFalloff = 0;
        Radius = 50;
        Rotation = 0;
        Enabled = true;
        IsGlobal = false;
    }
}
