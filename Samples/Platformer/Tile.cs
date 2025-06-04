using Embyr.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Platformer;

public enum TileType {
    Platform
}

public class Tile : Embyr.Tiles.Tile<TileType> {
    public Tile(TileType type, Texture2D spritesheet, Scene2D scene)
    : base(type, "tile", spritesheet, null, true, scene) { }
}
