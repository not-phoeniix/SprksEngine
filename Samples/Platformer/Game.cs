using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Platformer;

public class Game : Embyr.Game {
    protected override SetupParams Setup() {
        MainScene scene = new("main_scene");

        return new SetupParams() {
            InitialScene = scene,
            WindowTitle = "Platformer Sample !!",
            CanvasRes = new Point(150, 100),
            WindowRes = new Point(900, 600),
            EnableVSync = true,
            RenderClearColor = Color.Gray
        };
    }
}
