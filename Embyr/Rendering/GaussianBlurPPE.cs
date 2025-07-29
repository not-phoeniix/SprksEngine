using Microsoft.Xna.Framework.Graphics;

namespace Embyr.Rendering;

/// <summary>
/// Gaussian blur, inherits from PostProcessingEffect
/// </summary>
public class GaussianBlurPPE : PostProcessingEffect {
    private readonly Effect fxBlur;

    private int numPasses;

    /// <summary>
    /// Gets/sets the number of blur passes to apply when rendering
    /// </summary>
    public int NumPasses {
        get => numPasses;
        set {
            value = Math.Max(value, 0);

            if (value != numPasses) {
                ClearPasses();
                SetupPasses(value);
            }

            numPasses = value;
        }
    }

    /// <summary>
    /// Creates a new GaussianBlurPostProcessingEffect
    /// </summary>
    /// <param name="gd">GraphicsDevice to create effect with</param>
    public GaussianBlurPPE(GraphicsDevice gd) : base(gd) {
        fxBlur = ShaderManager.I.LoadShader("Blurs/gaussian_blur_separated");
        numPasses = 1;
        SetupPasses(numPasses);
    }

    private void SetupPasses(int numPasses) {
        for (int i = 0; i < numPasses; i++) {
            // two passes !!! one vertical and one horizontal, same shader though <3

            AddPass(new Pass(
                fxBlur,
                GraphicsDevice,
                static s => s.Parameters["IsVertical"].SetValue(false)
            ));

            AddPass(new Pass(
                fxBlur,
                GraphicsDevice,
                static s => s.Parameters["IsVertical"].SetValue(true)
            ));
        }
    }
}
