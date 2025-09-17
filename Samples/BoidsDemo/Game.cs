using System;
using Embyr;
using Embyr.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace BoidsDemo;

public class Game : Embyr.Game {
    private RendererSettings rSettings;

    protected override GameSetupParams SetupGame() {
        return new GameSetupParams() {
            RenderPipeline = RenderPipeline.Deferred2D,
            InitialSceneType = typeof(MainScene),
            WindowTitle = "Boids :D",
            EnableVSync = true,
        };
    }

    protected override RendererSetupParams SetupRenderer() {
        rSettings = new RendererSettings() {
            VolumetricScalar = 0.02f,
            EnablePostProcessing = true,
            EnableLighting = true,
            ClearColor = Color.Black
        };

        PostProcessingEffect[] fx = [
            new BloomPPE(GraphicsDevice) {
                LuminanceThreshold = 0.9f,
                NumBlurPasses = 4
            }
        ];

        return new RendererSetupParams() {
            RendererSettings = rSettings,
            PostProcessingEffects = fx
        };
    }

    protected override void Update(GameTime gameTime) {
        if (Input.IsKeyDownOnce(Keys.L)) {
            rSettings.EnableLighting = !rSettings.EnableLighting;
        }

        if (Input.IsKeyDownOnce(Keys.P)) {
            rSettings.EnablePostProcessing = !rSettings.EnablePostProcessing;
        }

        base.Update(gameTime);
    }
}
