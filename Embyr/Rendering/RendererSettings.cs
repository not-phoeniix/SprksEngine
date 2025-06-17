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

    /// <summary>
    /// Gets/sets the value of pseudo-3D depth scaling for 2D lighting with normals
    /// </summary>
    public float Depth3DScalar { get; set; } = 0.01f;

    /// <summary>
    /// Gets/sets the gamma value of the scene
    /// </summary>
    public float Gamma { get; set; } = 2.2f;
}
