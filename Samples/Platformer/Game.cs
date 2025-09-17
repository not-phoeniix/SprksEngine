using Embyr;
using Embyr.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Platformer;

public class Game : Embyr.Game {
    protected override GameSetupParams SetupGame() {
        ActionBindingPreset binds = new("default");
        binds.AddActionBind("left", Keys.A);
        binds.AddActionBind("right", Keys.D);
        binds.AddActionBind("jump", Keys.Space);

        return new GameSetupParams() {
            InitialSceneType = typeof(MainScene),
            WindowTitle = "Platformer Sample !!",
            CanvasRes = new Point(300, 200),
            WindowRes = new Point(900, 600),
            EnableVSync = true,
            RenderPipeline = RenderPipeline.Deferred2D,
            DefaultBindingPreset = binds
        };
    }

    protected override RendererSetupParams SetupRenderer() {
        RendererSettings settings = new() {
            VolumetricScalar = 0.1f,
            EnableLighting = true,
            EnablePostProcessing = true,
            ClearColor = Color.Black
        };

        PostProcessingEffect[] fx = [];

        return new RendererSetupParams() {
            RendererSettings = settings,
            PostProcessingEffects = fx
        };
    }
}
