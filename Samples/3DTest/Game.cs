using Embyr;
using Embyr.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace _3DTest;

public class Game : Embyr.Game {
    private RendererSettings rSettings;

    protected override GameSetupParams SetupGame() {
        EngineSettings.ShowDebugDrawing = true;
        EngineSettings.Gamma = 1.7f;

        MainScene scene = new("main_scene");

        return new GameSetupParams() {
            RenderPipeline = RenderPipeline.Forward3D,
            InitialScene = scene,
            CanvasRes = new Point(300, 200),
            WindowTitle = "3D test !!",
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
            // new BloomPostProcessingEffect(GraphicsDevice) {
            //     LuminanceThreshold = 0.9f,
            //     NumBlurPasses = 4
            // }
        ];

        return new RendererSetupParams() {
            RendererSettings = rSettings,
            PostProcessingEffects = fx
        };
    }
}
