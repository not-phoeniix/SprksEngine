using Embyr;
using Embyr.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LightingTest2D;

public class Game : Embyr.Game {
    protected override GameSetupParams SetupGame() {
        MainScene scene = new("main_scene");

        ActionBindingPreset binds = ActionBindingPreset.MakeDefault();

        return new GameSetupParams() {
            InitialScene = scene,
            WindowTitle = "2D lighting test...",
            CanvasRes = new Point(320, 180),
            WindowRes = new Point(1280, 720),
            EnableVSync = true,
            RenderClearColor = Color.Black,
            RenderPipeline = RenderPipeline.Deferred2D,
            DefaultBindingPreset = binds
        };
    }

    protected override RendererSetupParams SetupRenderer() {
        RendererSettings settings = new() {
            VolumetricScalar = 0.3f,
            EnablePostProcessing = true,
            EnableLighting = true,
            Depth3DScalar = 0.01f
        };

        PostProcessingEffect[] fx = [
            new BloomPostProcessingEffect(GraphicsDevice) {
                LuminanceThreshold = 1.5f,
                NumBlurPasses = 4
            },
        ];

        return new RendererSetupParams() {
            RendererSettings = settings,
            PostProcessingEffects = fx
        };
    }
}
