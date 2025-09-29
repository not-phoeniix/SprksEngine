using Sprks.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Platformer;

public enum TileType {
    Platform
}

public class Tile : Sprks.Tile<TileType> {
    public Tile(TileType type, Texture2D spritesheet, Texture2D normals, Scene2D scene)
    : base(type, spritesheet, normals, true, scene) {
        ObstructsLight = true;
    }
}
