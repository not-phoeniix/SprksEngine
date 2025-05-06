using System;
using Embyr;
using Embyr.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace BoidsDemo;

public class Game : Embyr.Game {
    protected override SetupParams Setup() {
        MainScene scene = new("main_scene");

        return new SetupParams() {
            RenderPipeline = RenderPipeline.Deferred2D,
            InitialScene = scene,
            WindowTitle = "Boids :D",
            EnableVSync = true,
            RenderClearColor = Color.Black
        };
    }

    protected override PostProcessingEffect[] SetupPostProcessingEffects() {
        return new PostProcessingEffect[] {
            new BloomPostProcessingEffect(GraphicsDevice) {
                LuminanceThreshold = 0.9f,
                NumBlurPasses = 4
            }
        };
    }
}
