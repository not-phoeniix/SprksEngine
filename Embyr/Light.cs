using Microsoft.Xna.Framework;

namespace Embyr;

/// <summary>
/// Describes a light in a scene
/// </summary>
public abstract class Light {
    /// <summary>
    /// Gets/sets whether or not light is enabled
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets whether or not this light is a global sun light
    /// </summary>
    public bool IsGlobal { get; init; }

    /// <summary>
    /// Gets/sets the color of this light
    /// </summary>
    public Color Color { get; set; }

    /// <summary>
    /// Gets/sets the intensity of light
    /// </summary>
    public float Intensity { get; set; }

    /// <summary>
    /// Creates a new instance of a light
    /// </summary>
    public Light() {
        Color = Color.White;
        Intensity = 1;
        Enabled = true;
        IsGlobal = false;
    }
}
