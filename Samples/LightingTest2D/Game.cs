using Embyr;
using Embyr.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace LightingTest2D;

public class Game : Embyr.Game {
    protected override GameSetupParams SetupGame() {
        ActionBindingPreset binds = ActionBindingPreset.MakeDefault();
        binds.AddActionBind("left", Keys.A);
        binds.AddActionBind("right", Keys.D);
        binds.AddActionBind("up", Keys.W);
        binds.AddActionBind("down", Keys.S);

        return new GameSetupParams() {
            InitialSceneType = typeof(MainScene),
            WindowTitle = "2D lighting test...",
            CanvasRes = new Point(320, 180),
            WindowRes = new Point(1280, 720),
            EnableVSync = true,
            RenderPipeline = RenderPipeline.Deferred2D,
            DefaultBindingPreset = binds
        };
    }

    protected override RendererSetupParams SetupRenderer() {
        RendererSettings settings = new() {
            VolumetricScalar = 0.1f,
            EnablePostProcessing = true,
            EnableLighting = true,
            Depth3DScalar = 0.01f,
            ClearColor = Color.Black,
            Gamma = 2.2f
        };

        PostProcessingEffect[] fx = [
            new BloomPPE(GraphicsDevice) {
                LuminanceThreshold = 2.0f,
                NumBlurPasses = 4
            },
        ];

        return new RendererSetupParams() {
            RendererSettings = settings,
            PostProcessingEffects = fx
        };
    }
}
