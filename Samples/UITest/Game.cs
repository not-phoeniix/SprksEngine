using System;
using Embyr;
using Embyr.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace UITest;

public class Game : Embyr.Game {
    protected override GameSetupParams SetupGame() {
        return new GameSetupParams() {
            InitialSceneType = typeof(MainScene),
            WindowTitle = "UI Test :D",
            CanvasRes = new Point(480, 270),
            WindowRes = new Point(1280, 720),
            IsFullscreen = false,
            EnableVSync = true,
            RenderPipeline = RenderPipeline.Deferred2D,
            DefaultBindingPreset = ActionBindingPreset.MakeDefault()
        };
    }

    protected override RendererSetupParams SetupRenderer() {
        RendererSettings settings = new() {
            VolumetricScalar = 0,
            EnablePostProcessing = false,
            EnableLighting = false,
            Depth3DScalar = 0.01f,
            ClearColor = Color.Black,
            Gamma = 2.2f
        };

        return new RendererSetupParams() {
            RendererSettings = settings,
            PostProcessingEffects = Array.Empty<PostProcessingEffect>()
        };
    }
}
