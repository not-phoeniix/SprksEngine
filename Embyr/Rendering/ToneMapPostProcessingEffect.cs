using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Embyr.Rendering;

/// <summary>
/// Tone mapping and gamma correction post process, inherits from PostProcessingEffect
/// </summary>
public class ToneMapGammaPostProcessingEffect : PostProcessingEffect {
    private readonly Effect fxToneMap;

    /// <summary>
    /// Gets/sets the gamma to correct to as a final step for this effect
    /// </summary>
    public float Gamma { get; set; }

    /// <summary>
    /// Creates a new GaussianBlurPostProcessingEffect
    /// </summary>
    /// <param name="gd">GraphicsDevice to create effect with</param>
    public ToneMapGammaPostProcessingEffect(GraphicsDevice gd) : base(gd) {
        fxToneMap = ShaderManager.I.LoadShader("PostProcessing/tone_map_gamma");

        AddPass(new Pass(
            fxToneMap,
            gd,
            s => s.Parameters["Gamma"].SetValue(Gamma),
            Width,
            Height,
            SurfaceFormat.Color
        ));
    }
}
