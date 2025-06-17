using Microsoft.Xna.Framework.Graphics;

namespace Embyr.Rendering;

/// <summary>
/// HDR bloom post processing effect with 5 pixel radius gaussian blur,
/// inherits from <c>PostProcessingEffect</c>
/// </summary>
public class BloomPostProcessingEffect : PostProcessingEffect {
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
                SetupPasses(Width, Height, value);
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
    public BloomPostProcessingEffect(GraphicsDevice gd, float luminanceThreshold = 1.0f, int numBlurPasses = 2) : base(gd) {
        fxBloomThreshold = ShaderManager.I.LoadShader("PostProcessing/bloom_threshold");
        fxBlur = ShaderManager.I.LoadShader("Blurs/gaussian_blur_separated");
        fxBloomCombine = ShaderManager.I.LoadShader("PostProcessing/bloom_combine");

        LuminanceThreshold = luminanceThreshold;
        this.numBlurPasses = numBlurPasses;
        SetupPasses(Width, Height, numBlurPasses);
    }

    private void SetupPasses(int width, int height, int numBlurPasses) {
        Pass thresholdPass = new(
            fxBloomThreshold,
            GraphicsDevice,
            (shader) => {
                shader.Parameters["Threshold"].SetValue(LuminanceThreshold);
            },
            width,
            height
        );

        AddPass(thresholdPass);

        for (int i = 0; i < numBlurPasses; i++) {
            Pass blurVertical = new(
                fxBlur,
                GraphicsDevice,
                s => s.Parameters["IsVertical"].SetValue(true),
                width,
                height
            );

            Pass blurHorizontal = new(
                fxBlur,
                GraphicsDevice,
                s => s.Parameters["IsVertical"].SetValue(false),
                width,
                height
            );

            AddPass(blurVertical);
            AddPass(blurHorizontal);
        }

        Pass combinePass = new(
            fxBloomCombine,
            GraphicsDevice,
            (shader) => {
                shader.Parameters["InitialTexture"].SetValue(InputRenderTarget);
            },
            width,
            height
        );

        AddPass(combinePass);
    }
}
