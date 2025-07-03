using Embyr;
using Embyr.Scenes;
using Microsoft.Xna.Framework.Graphics;

namespace LightingTest2D;

public class Tile : Tile<TileType> {
    public Tile(TileType type, Texture2D texture, Texture2D normal, Scene2D scene)
    : base(type, texture, normal, true, scene) {
        ObstructsLight = true;
    }
}
