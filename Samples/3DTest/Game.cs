using Sprks;
using Sprks.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace _3DTest;

public class Game : Sprks.Game {
    private RendererSettings rSettings;

    protected override GameSetupParams SetupGame() {
        EngineSettings.ShowDebugDrawing = true;
        EngineSettings.Gamma = 1.7f;

        ActionBindingPreset binds = ActionBindingPreset.MakeDefault();
        binds.AddActionBind("forward", Keys.W);
        binds.AddActionBind("forward", Buttons.LeftThumbstickUp);
        binds.AddActionBind("left", Keys.A);
        binds.AddActionBind("left", Buttons.LeftThumbstickLeft);
        binds.AddActionBind("right", Keys.D);
        binds.AddActionBind("right", Buttons.LeftThumbstickRight);
        binds.AddActionBind("backward", Keys.S);
        binds.AddActionBind("backward", Buttons.LeftThumbstickDown);
        binds.AddActionBind("up", Keys.Space);
        binds.AddActionBind("up", Buttons.A);
        binds.AddActionBind("down", Keys.LeftShift);
        binds.AddActionBind("down", Buttons.RightStick);

        return new GameSetupParams() {
            RenderPipeline = RenderPipeline.Forward3D,
            InitialSceneType = typeof(MainScene),
            CanvasRes = new Point(300, 200),
            WindowTitle = "3D test !!",
            EnableVSync = true,
            DefaultBindingPreset = binds
        };
    }

    protected override RendererSetupParams SetupRenderer() {
        rSettings = new RendererSettings() {
            VolumetricScalar = 0.05f,
            EnablePostProcessing = true,
            EnableLighting = true,
            ClearColor = Color.Black
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
