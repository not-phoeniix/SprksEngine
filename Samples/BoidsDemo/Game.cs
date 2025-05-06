using System;
using Embyr;
using Embyr.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace BoidsDemo;

public class Game : Embyr.Game {
    private RendererSettings rSettings;

    protected override GameSetupParams SetupGame() {
        MainScene scene = new("main_scene");

        return new GameSetupParams() {
            RenderPipeline = RenderPipeline.Deferred2D,
            InitialScene = scene,
            WindowTitle = "Boids :D",
            EnableVSync = true,
            RenderClearColor = Color.Black
        };
    }

    protected override RendererSetupParams SetupRenderer() {
        rSettings = new RendererSettings() {
            VolumetricScalar = 0.05f,
            EnablePostProcessing = true,
            EnableLighting = true
        };

        PostProcessingEffect[] fx = [
            new BloomPostProcessingEffect(GraphicsDevice) {
                LuminanceThreshold = 0.95f,
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
