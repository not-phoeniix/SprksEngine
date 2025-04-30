using Embyr.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Embyr.Tiles;

public class TileMap<T> : IActor where T : Enum {
    private readonly NList2D<Tile<T>> tiles;

    public Scene Scene { get; }

    /// <summary>
    /// Gets the transform of this TileMap
    /// </summary>
    public Transform Transform { get; }

    /// <summary>
    /// Gets the total bounds of this TileMap
    /// </summary>
    public Rectangle Bounds => new(
        tiles.Min,
        tiles.Max - tiles.Min
    );

    public event Action<Scene> OnAdded;
    public event Action<Scene> OnRemoved;

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

    public Tile<T> this[int x, int y] {
        get => tiles[x, y];
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
        this.Transform = new Transform(position);
    }

    public void Update(float dt) {
        Rectangle tilespaceSim = GetTilespaceSimulatedRect();
        for (int y = tilespaceSim.Top; y <= tilespaceSim.Bottom; y++) {
            for (int x = tilespaceSim.Left; x <= tilespaceSim.Right; x++) {
                Tile<T> tile = tiles[x, y];
                if (tile != null) {
                    float dSqr = Vector2.DistanceSquared(
                        tile.Transform.GlobalPosition,
                        Scene.Camera.Position
                    );
                    if (dSqr >= SimulationDistance * SimulationDistance) {
                        tile.Update(dt);
                    }
                }
            }
        }
    }

    public void PhysicsUpdate(float fdt) {
        Rectangle tilespaceSim = GetTilespaceSimulatedRect();
        for (int y = tilespaceSim.Top; y <= tilespaceSim.Bottom; y++) {
            for (int x = tilespaceSim.Left; x <= tilespaceSim.Right; x++) {
                Tile<T> tile = tiles[x, y];
                if (tile != null) {
                    float dSqr = Vector2.DistanceSquared(
                        tile.Transform.GlobalPosition,
                        Scene.Camera.Position
                    );
                    if (dSqr >= SimulationDistance * SimulationDistance) {
                        tile.PhysicsUpdate(fdt);
                    }
                }
            }
        }
    }

    public void Draw(SpriteBatch sb) {
        Rectangle tilespaceView = GetTilespaceViewRect();
        for (int y = tilespaceView.Top; y <= tilespaceView.Bottom; y++) {
            for (int x = tilespaceView.Left; x <= tilespaceView.Right; x++) {
                tiles[x, y]?.Draw(sb);
            }
        }
    }

    public void InvokeOnAdded(Scene scene) {
        OnAdded?.Invoke(scene);
    }

    public void InvokeOnRemoved(Scene scene) {
        OnRemoved?.Invoke(scene);
    }

    private Rectangle GetTilespaceViewRect() {
        Rectangle camView = Scene.Camera.ViewBounds;
        return PixelToTileSpace(camView);
    }

    private Rectangle GetTilespaceSimulatedRect() {
        Rectangle simView = new(
            Scene.Camera.Position.ToPoint() - new Point((int)SimulationDistance),
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
            (int)MathF.Ceiling((rect.Right - rect.Left) / Tile<T>.PixelSize) + padding * 2,
            (int)MathF.Ceiling((rect.Bottom - rect.Top) / Tile<T>.PixelSize) + padding * 2
        );
    }
}
