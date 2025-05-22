using Embyr.Physics;
using Embyr.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Embyr.Tiles;

/// <summary>
/// A 2D grid/map of tiles, implements <c>IActor</c>
/// </summary>
/// <typeparam name="T">Tile's type enum, contains all possible tile type values</typeparam>
public class TileMap<T> : IActor2D, IDebugDrawable2D where T : Enum {
    private readonly NList2D<Tile<T>> tiles;

    public Scene Scene { get; }

    /// <summary>
    /// Gets the transform of this TileMap
    /// </summary>
    public Transform2D Transform { get; }

    /// <summary>
    /// Gets the collider for this TileMap
    /// </summary>
    public Collider2D Collider { get; }

    /// <summary>
    /// Event executed when this tile map is added to a scene
    /// </summary>
    public event Action<Scene>? OnAdded;

    /// <summary>
    /// Event executed when this tile map is removed from a scene
    /// </summary>
    public event Action<Scene>? OnRemoved;

    /// <summary>
    /// Gets/sets the name of this TileMap actor
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets/sets the simulation distance for updating this tile map
    /// </summary>
    public float SimulationDistance { get; set; }

    /// <summary>
    /// Gets whether or not this tilemap actor should be saved, always false
    /// </summary>
    public bool ShouldBeSaved => false;

    public Tile<T>? this[int x, int y] {
        get => tiles.InBounds(x, y) ? tiles[x, y] : null;
        set => tiles[x, y] = value;
    }

    /// <summary>
    /// Creates a new TileMap
    /// </summary>
    /// <param name="position">Position of TileMap in the world</param>
    /// <param name="simulationDistance">Simulation distance for TileMap updating</param>
    /// <param name="name">Name of actor in scene</param>
    /// <param name="scene">Scene to place tilemap into</param>
    public TileMap(Vector2 position, float simulationDistance, string name, Scene scene) {
        this.Scene = scene;
        this.Name = name;
        this.SimulationDistance = simulationDistance;
        this.tiles = new NList2D<Tile<T>>();
        this.Transform = new Transform2D(position);
        this.Collider = new BoxCollider2D(this, Vector2.Zero) {
            Collidable = false
        };
    }

    /// <summary>
    /// Updates general logic for this TileMap within its update radius, relative to scene camera
    /// </summary>
    /// <param name="dt">Time passed since last frame</param>
    public void Update(float dt) {
        Rectangle tilespaceSim = GetTilespaceSimulatedRect();
        for (int y = tilespaceSim.Top; y <= tilespaceSim.Bottom; y++) {
            for (int x = tilespaceSim.Left; x <= tilespaceSim.Right; x++) {
                // don't update if index is out of bounds
                if (!tiles.InBounds(x, y)) continue;

                Tile<T> tile = tiles[x, y];
                if (tile != null) {
                    float dSqr = Vector2.DistanceSquared(
                        tile.Transform.GlobalPosition,
                        ((Scene2D)Scene).Camera.Position
                    );
                    if (dSqr >= SimulationDistance * SimulationDistance) {
                        tile.Update(dt);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Updates physics logic of this TileMap within its update radius, relative to scene camera
    /// </summary>
    /// <param name="fdt">Time passed since last physics update</param>
    public void PhysicsUpdate(float fdt) {
        Rectangle tilespaceSim = GetTilespaceSimulatedRect();
        for (int y = tilespaceSim.Top; y <= tilespaceSim.Bottom; y++) {
            for (int x = tilespaceSim.Left; x <= tilespaceSim.Right; x++) {
                // don't update if index is out of bounds
                if (!tiles.InBounds(x, y)) continue;

                Tile<T> tile = tiles[x, y];
                if (tile != null) {
                    float dSqr = Vector2.DistanceSquared(
                        tile.Transform.GlobalPosition,
                        ((Scene2D)Scene).Camera.Position
                    );
                    if (dSqr >= SimulationDistance * SimulationDistance) {
                        tile.PhysicsUpdate(fdt);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Draws this tilemap to the screen
    /// </summary>
    /// <param name="sb">SpriteBatch to draw with</param>
    public void Draw(SpriteBatch sb) {
        Rectangle tilespaceView = GetTilespaceViewRect();
        for (int y = tilespaceView.Top; y <= tilespaceView.Bottom; y++) {
            for (int x = tilespaceView.Left; x <= tilespaceView.Right; x++) {
                // don't draw if index is out of bounds
                if (!tiles.InBounds(x, y)) continue;

                tiles[x, y]?.Draw(sb);
            }
        }
    }

    /// <summary>
    /// Draws debug information for this TileMap
    /// </summary>
    /// <param name="sb">SpriteBatch to draw with</param>
    public void DebugDraw(SpriteBatch sb) {
        Rectangle tilespaceView = GetTilespaceViewRect();
        for (int y = tilespaceView.Top; y <= tilespaceView.Bottom; y++) {
            for (int x = tilespaceView.Left; x <= tilespaceView.Right; x++) {
                if (!tiles.InBounds(x, y)) continue;
                tiles[x, y]?.DebugDraw(sb);
            }
        }

        Collider.DebugDraw(sb);
    }

    /// <summary>
    /// Adds a tile to this map
    /// </summary>
    /// <param name="tile">Tile to add</param>
    /// <param name="x">X index to add, can be negative</param>
    /// <param name="y">Y index to add, can be negative</param>
    public void AddTile(Tile<T> tile, int x, int y) {
        tiles.Add(tile, x, y);

        if (tile != null) {
            tile.Transform.Parent = Transform;
            tile.Transform.Position = new Vector2(
                x * Tile<T>.PixelSize,
                y * Tile<T>.PixelSize
            );
            Collider.AddChild(tile.Collider);
        }

        for (int x2 = x - 1; x2 <= x + 1; x2++) {
            for (int y2 = y - 1; y2 <= y + 1; y2++) {
                if (!tiles.InBounds(x2, y2)) continue;
                tiles[x2, y2]?.UpdateEdges(this, null);
            }
        }

        ((BoxCollider2D)Collider).Size = (tiles.Size.ToVector2() + new Vector2(2)) * Tile<T>.PixelSize;
    }

    /// <summary>
    /// Adds a tile to this map
    /// </summary>
    /// <param name="tile">Tile to add</param>
    /// <param name="pos">X/Y index point to add tile to</param>
    public void AddTile(Tile<T> tile, Point pos) {
        AddTile(tile, pos.X, pos.Y);
    }

    public void InvokeOnAdded(Scene scene) {
        OnAdded?.Invoke(scene);
    }

    public void InvokeOnRemoved(Scene scene) {
        OnRemoved?.Invoke(scene);
    }

    private Rectangle GetTilespaceViewRect() {
        Rectangle camView = ((Scene2D)Scene).Camera.ViewBounds;
        return PixelToTileSpace(camView);
    }

    private Rectangle GetTilespaceSimulatedRect() {
        Rectangle simView = new(
            ((Scene2D)Scene).Camera.Position.ToPoint() - new Point((int)SimulationDistance),
            new Point((int)SimulationDistance * 2)
        );

        return PixelToTileSpace(simView);
    }

    /// <summary>
    /// Converts a rectangle in pixel-space to tile-space
    /// </summary>
    /// <param name="rect">Rectangle to convert</param>
    /// <param name="padding">Number of tile padding to expand</param>
    /// <returns>Rectangle converted into tile-space</returns>
    public static Rectangle PixelToTileSpace(Rectangle rect, int padding = 0) {
        return new Rectangle(
            (int)MathF.Floor(rect.X / Tile<T>.PixelSize) - padding,
            (int)MathF.Floor(rect.Y / Tile<T>.PixelSize) - padding,
            (int)MathF.Ceiling(rect.Size.X / Tile<T>.PixelSize) + padding * 2,
            (int)MathF.Ceiling(rect.Size.Y / Tile<T>.PixelSize) + padding * 2
        );
    }
}
