namespace Embyr.Rendering;

/// <summary>
/// Sealed data class that holds all possible renderer settings across all renderers
/// </summary>
public sealed class RendererSettings {
    /// <summary>
    /// Get/sets whether or not to enable the drawing of lights
    /// </summary>
    public bool EnableLighting { get; set; } = true;

    /// <summary>
    /// Gets/sets whether or not to enable post processing
    /// </summary>
    public bool EnablePostProcessing { get; set; } = true;

    /// <summary>
    /// Gets/sets the volumetric scalar factor for lighting
    /// </summary>
    public float VolumetricScalar { get; set; } = 0.0f;
}
