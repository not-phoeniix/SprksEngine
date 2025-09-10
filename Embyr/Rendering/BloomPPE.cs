using Microsoft.Xna.Framework.Graphics;

namespace Embyr.Rendering;

/// <summary>
/// HDR bloom post processing effect with 5 pixel radius gaussian blur,
/// inherits from <c>PostProcessingEffect</c>
/// </summary>
public class BloomPPE : PostProcessingEffect {
    private readonly Effect fxBloomThreshold;
    private readonly Effect fxBlur;
    private readonly Effect fxBloomCombine;
    private int numBlurPasses;

    /// <summary>
    /// Gets/sets the luminance threshold of bloom
    /// </summary>
    public float LuminanceThreshold { get; set; }

    /// <summary>
    /// Gets/sets the number of blur passes to use for bloom
    /// </summary>
    public int NumBlurPasses {
        get => numBlurPasses;
        set {
            value = Math.Max(value, 0);

            if (value != numBlurPasses) {
                ClearPasses();
                SetupPasses(value);
            }

            numBlurPasses = value;
        }
    }

    /// <summary>
    /// Creates a new BloomPostProcessingEffect
    /// </summary>
    /// <param name="gd">GraphicsDevice to create bloom with</param>
    /// <param name="luminanceThreshold">HDR luminance threshold to apply bloom to</param>
    /// <param name="numBlurPasses">Number of blur passes to use when blurring bloom</param>
    public BloomPPE(GraphicsDevice gd, float luminanceThreshold = 1.0f, int numBlurPasses = 2) : base(gd) {
        fxBloomThreshold = ShaderManager.LoadShader("PostProcessing/bloom_threshold");
        fxBlur = ShaderManager.LoadShader("Blurs/gaussian_blur_separated");
        fxBloomCombine = ShaderManager.LoadShader("PostProcessing/bloom_combine");

        LuminanceThreshold = luminanceThreshold;
        this.numBlurPasses = numBlurPasses;
        SetupPasses(numBlurPasses);
    }

    private void SetupPasses(int numBlurPasses) {
        Pass thresholdPass = new(fxBloomThreshold, GraphicsDevice, PassLuminance);
        AddPass(thresholdPass);

        for (int i = 0; i < numBlurPasses; i++) {
            Pass blurVertical = new(
                fxBlur,
                GraphicsDevice,
                static shader => shader.Parameters["IsVertical"].SetValue(true)
            );

            Pass blurHorizontal = new(
                fxBlur,
                GraphicsDevice,
                static shader => shader.Parameters["IsVertical"].SetValue(false)
            );

            AddPass(blurVertical);
            AddPass(blurHorizontal);
        }

        Pass combinePass = new(fxBloomCombine, GraphicsDevice, PassInitialTexture);
        AddPass(combinePass);
    }

    private void PassLuminance(Effect shader) {
        shader.Parameters["Threshold"].SetValue(LuminanceThreshold);
    }

    private void PassInitialTexture(Effect shader) {
        shader.Parameters["InitialTexture"].SetValue(InputRenderTarget);
    }
}
