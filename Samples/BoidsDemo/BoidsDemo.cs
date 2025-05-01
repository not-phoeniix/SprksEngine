using Microsoft.Xna.Framework;

namespace BoidsDemo;

public class BoidsDemo : Embyr.Game {
    protected override SetupParams Setup() {
        MainScene scene = new("main_scene");

        return new SetupParams() {
            InitialScene = scene,
            WindowTitle = "Boids :D",
            EnableVSync = true,
            RenderClearColor = Color.Black
        };
    }
}
