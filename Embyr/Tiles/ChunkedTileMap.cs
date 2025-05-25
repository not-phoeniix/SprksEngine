using Embyr.Scenes;
using Microsoft.Xna.Framework;

namespace Embyr.Tiles;

public class ChunkedTileMap<T> : TileMap<T> where T : Enum {
    public ChunkedTileMap(string name, Vector2 position, float simulationDistance, Scene2D scene)
    : base(name, position, simulationDistance, scene) { }
}
