using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Embyr.Rendering;

/// <summary>
/// Tone mapping and gamma correction post process, inherits from PostProcessingEffect
/// </summary>
public class ToneMapGammaPPE : PostProcessingEffect {
    private readonly Effect fxToneMap;

    /// <summary>
    /// Gets/sets the gamma to correct to as a final step for this effect
    /// </summary>
    public float Gamma { get; set; }

    /// <summary>
    /// Gets/sets whether or not to enable tonemaping, when false only gamma is applied
    /// </summary>
    public bool EnableTonemapping { get; set; }

    /// <summary>
    /// Creates a new GaussianBlurPostProcessingEffect
    /// </summary>
    /// <param name="gd">GraphicsDevice to create effect with</param>
    public ToneMapGammaPPE(GraphicsDevice gd) : base(gd) {
        fxToneMap = ShaderManager.I.LoadShader("PostProcessing/tone_map_gamma");
        Gamma = 2.2f;
        EnableTonemapping = true;

        AddPass(new Pass(
            fxToneMap,
            gd,
            PassShaderParams,
            SurfaceFormat.Color
        ));
    }

    private void PassShaderParams(Effect shader) {
        shader.Parameters["Gamma"].SetValue(Gamma);
        shader.Parameters["EnableTonemapping"].SetValue(EnableTonemapping);
    }
}
