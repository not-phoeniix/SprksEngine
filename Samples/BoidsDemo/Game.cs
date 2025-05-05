using Microsoft.Xna.Framework;

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
}
