using Embyr;
using Embyr.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Platformer;

public class Game : Embyr.Game {
    protected override GameSetupParams SetupGame() {
        MainScene scene = new("main_scene");

        ActionBindingPreset binds = new("default");
        binds.AddActionBind("left", Keys.A);
        binds.AddActionBind("right", Keys.D);
        binds.AddActionBind("jump", Keys.Space);

        return new GameSetupParams() {
            InitialScene = scene,
            WindowTitle = "Platformer Sample !!",
            CanvasRes = new Point(300, 200),
            WindowRes = new Point(900, 600),
            EnableVSync = true,
            RenderClearColor = Color.Gray,
            RenderPipeline = RenderPipeline.Deferred2D,
            DefaultBindingPreset = binds
        };
    }

    protected override RendererSetupParams SetupRenderer() {
        RendererSettings settings = new() {
            VolumetricScalar = 0.2f,
            EnableLighting = true,
            EnablePostProcessing = true
        };

        PostProcessingEffect[] fx = [
        ];

        return new RendererSetupParams() {
            RendererSettings = settings,
            PostProcessingEffects = fx
        };
    }
}
