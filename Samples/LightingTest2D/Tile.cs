using Embyr.Scenes;
using Embyr.Tiles;
using Microsoft.Xna.Framework.Graphics;

namespace LightingTest2D;

public class Tile : Tile<TileType> {
    public Tile(TileType type, Texture2D texture, Texture2D? normal, Scene2D scene)
    : base(type, $"{type}_tile", texture, normal, true, scene) {
        ObstructsLight = true;
    }
}
