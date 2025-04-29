using Microsoft.Xna.Framework;

namespace Embyr.Scenes;

public interface ITransform {
    public Vector2 Position { get; set; }
    public Vector2 CenterPosition { get; set; }
    public Rectangle Bounds { get; }
}
