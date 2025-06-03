using Embyr.Scenes;
using Embyr.Tiles;
using Microsoft.Xna.Framework.Graphics;

namespace LightingTest2D;

public class Tile : Tile<TileType> {
    public Tile(TileType type, Texture2D texture, Scene2D scene)
    : base(type, $"{type}_tile", texture, true, scene) { }
}
