using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Embyr.Rendering;

public class BloomPostProcessingEffect : PostProcessingEffect {
    private readonly Effect fxBloomThreshold;
    private readonly Effect fxBloomBlur;
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

    public BloomPostProcessingEffect(GraphicsDevice gd, float luminanceThreshold = 0.95f, int numBlurPasses = 2) : base(gd) {
        fxBloomThreshold = ShaderManager.I.LoadShader("bloom_threshold", ShaderManager.ShaderProfile.OpenGL);
        fxBloomBlur = ShaderManager.I.LoadShader("bloom_blur", ShaderManager.ShaderProfile.OpenGL);
        fxBloomCombine = ShaderManager.I.LoadShader("bloom_combine", ShaderManager.ShaderProfile.OpenGL);

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
            Pass blurPass = new(
                fxBloomBlur,
                GraphicsDevice,
                null,
                width,
                height
            );

            AddPass(blurPass);
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
