using Sprks.Physics;
using Sprks.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sprks;

/// <summary>
/// A 2D grid/map of tiles, inherits from Actor2D
/// </summary>
/// <typeparam name="T">Tile's type enum, contains all possible tile type values</typeparam>
public sealed class TileMap<T> : Actor2D where T : Enum {
    private static readonly int chunkSize = 8;

    private readonly NList2D<Tile<T>?> tiles;
    private readonly NList2D<BoxCollider2D> chunks;
    private readonly BoxCollider2D collider;

    /// <summary>
    /// Gets/sets the simulation distance for updating this tile map
    /// </summary>
    public float SimulationDistance { get; set; }

    /// <summary>
    /// Gets the inclusive minimum tilespace coordinates for tiles within this map
    /// </summary>
    public Point Min => tiles.Min;

    /// <summary>
    /// Gets the exclusive maximum tilespace coordinates for tiles within this map
    /// </summary>
    public Point Max => tiles.Max;

    public Tile<T>? this[int x, int y] {
        get => tiles.InBounds(x, y) ? tiles[x, y] : null;
        set => AddTile(value, x, y);
    }

    /// <summary>
    /// Creates a new TileMap
    /// </summary>
    /// <param name="position">Position of TileMap in the world</param>
    /// <param name="simulationDistance">Simulation distance for TileMap updating</param>
    /// <param name="scene">Scene to place tilemap into</param>
    public TileMap(Vector2 position, float simulationDistance, Scene2D scene)
    : base(position, scene) {
        this.SimulationDistance = simulationDistance;
        this.Transform = new Transform2D(position);
        this.tiles = new NList2D<Tile<T>?>();
        this.chunks = new NList2D<BoxCollider2D>();
        this.collider = AddComponent<BoxCollider2D>();
        collider.Collidable = false;
    }

    /// <inheritdoc/>
    public override void Update(float dt) {
        base.Update(dt);

        Rectangle tilespaceSim = GetTilespaceSimulatedRect();
        for (int y = tilespaceSim.Top; y <= tilespaceSim.Bottom; y++) {
            for (int x = tilespaceSim.Left; x <= tilespaceSim.Right; x++) {
                // don't update if index is out of bounds
                if (!tiles.InBounds(x, y)) continue;

                Tile<T>? tile = tiles[x, y];
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

    /// <inheritdoc/>
    public override void PhysicsUpdate(float fdt) {
        base.PhysicsUpdate(fdt);

        Rectangle tilespaceSim = GetTilespaceSimulatedRect();
        for (int y = tilespaceSim.Top; y <= tilespaceSim.Bottom; y++) {
            for (int x = tilespaceSim.Left; x <= tilespaceSim.Right; x++) {
                // don't update if index is out of bounds
                if (!tiles.InBounds(x, y)) continue;

                Tile<T>? tile = tiles[x, y];
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

    /// <inheritdoc/>
    public override void Draw(SpriteBatch sb) {
        base.Draw(sb);

        Rectangle tilespaceView = GetTilespaceViewRect();
        tilespaceView.Inflate(1, 1);
        for (int y = tilespaceView.Top; y <= tilespaceView.Bottom; y++) {
            for (int x = tilespaceView.Left; x <= tilespaceView.Right; x++) {
                // don't draw if index is out of bounds
                if (!tiles.InBounds(x, y)) continue;

                tiles[x, y]?.Draw(sb);
            }
        }
    }

    /// <inheritdoc/>
    public override void DebugDraw(SpriteBatch sb) {
        base.DebugDraw(sb);

        Rectangle tilespaceView = GetTilespaceViewRect();
        for (int y = tilespaceView.Top; y <= tilespaceView.Bottom; y++) {
            for (int x = tilespaceView.Left; x <= tilespaceView.Right; x++) {
                if (!tiles.InBounds(x, y)) continue;
                tiles[x, y]?.DebugDraw(sb);
            }
        }

        for (int x = chunks.Min.X; x < chunks.Max.X; x++) {
            for (int y = chunks.Min.Y; y < chunks.Max.Y; y++) {
                if (!chunks.InBounds(x, y)) continue;
                chunks[x, y]?.DebugDraw(sb);
            }
        }
    }

    /// <summary>
    /// Adds a tile to this map
    /// </summary>
    /// <param name="tile">Tile to add, removes if null</param>
    /// <param name="x">X index to add, can be negative</param>
    /// <param name="y">Y index to add, can be negative</param>
    public void AddTile(Tile<T>? tile, int x, int y) {
        RemoveTile(x, y, false);

        Point chunkIndex = new(
            (int)MathF.Floor((float)x / chunkSize),
            (int)MathF.Floor((float)y / chunkSize)
        );

        Point chunkPixelSize = new(chunkSize * Tile<T>.PixelSize);

        if (!chunks.TryGetData(chunkIndex.X, chunkIndex.Y, out BoxCollider2D? chunk) || chunk == null) {
            chunk = new BoxCollider2D(this) {
                Collidable = false,
                Size = chunkPixelSize,
                Offset = new Point(chunkPixelSize.X / 2 - (Tile<T>.PixelSize / 2)) + chunkPixelSize * chunkIndex
            };
            collider.AddChild(chunk);

            // expand chunk by 2 pixels in every direction to
            // make sure tile collisions at the edge of a chunk
            // aren't ignored
            chunk.Size += new Point(4);
            chunk.Offset -= new Point(2);

            chunks.Add(chunk, chunkIndex.X, chunkIndex.Y);
        }

        if (tile != null) {
            tiles.Add(tile, x, y);

            tile.Transform.Parent = Transform;
            tile.Transform.Position = new Vector2(
                x * Tile<T>.PixelSize,
                y * Tile<T>.PixelSize
            );

            chunk.AddChild(tile.Collider);
        }

        for (int x2 = x - 1; x2 <= x + 1; x2++) {
            for (int y2 = y - 1; y2 <= y + 1; y2++) {
                if (tiles.InBounds(x2, y2)) {
                    tiles[x2, y2]?.UpdateEdges(this, null);
                }
            }
        }

        bool shouldRecalculate =
            tile == null ||
            !collider.Contains(tile.Collider.Min) ||
            !collider.Contains(tile.Collider.Max);

        if (shouldRecalculate) {
            RecalculateBounds();
        }
    }

    /// <summary>
    /// Adds a tile to this map
    /// </summary>
    /// <param name="tile">Tile to add, removes if null</param>
    /// <param name="pos">X/Y index point to add tile to</param>
    public void AddTile(Tile<T>? tile, Point pos) {
        AddTile(tile, pos.X, pos.Y);
    }

    /// <summary>
    /// Removes a tile from this map and returns it
    /// </summary>
    /// <param name="x">X index to add, can be negative</param>
    /// <param name="y">Y index to add, can be negative</param>
    /// <returns>Reference to the tile removed at that index, can be null</returns>
    public Tile<T>? RemoveTile(int x, int y) {
        return RemoveTile(x, y, true);
    }

    /// <summary>
    /// Removes a tile from this map and returns it
    /// </summary>
    /// <param name="pos">X/Y index point to add tile to</param>
    /// <returns>Reference to the tile removed at that index, can be null</returns>
    public Tile<T>? RemoveTile(Point pos) {
        return RemoveTile(pos.X, pos.Y);
    }

    private Tile<T>? RemoveTile(int x, int y, bool recalculate) {
        // detatch existing tile if it exists
        if (tiles.TryGetData(x, y, out Tile<T>? existingTile) && existingTile != null) {
            Transform.RemoveChild(existingTile.Transform);
            existingTile.Collider.RemoveFromParent();
            tiles[x, y] = null;

            if (recalculate) {
                for (int x2 = x - 1; x2 <= x + 1; x2++) {
                    for (int y2 = y - 1; y2 <= y + 1; y2++) {
                        if (tiles.InBounds(x2, y2)) {
                            tiles[x2, y2]?.UpdateEdges(this, null);
                        }
                    }
                }

                RecalculateBounds();
            }

            return existingTile;
        }

        return null;
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

    private void RecalculateBounds() {
        Vector2 min = new(float.PositiveInfinity);
        Vector2 max = new(float.NegativeInfinity);

        for (int x = tiles.Min.X; x < tiles.Max.X; x++) {
            for (int y = tiles.Min.Y; y < tiles.Max.Y; y++) {
                Tile<T>? tile = tiles[x, y];
                if (tile != null) {
                    min = Vector2.Min(min, tile.Collider.Min);
                    max = Vector2.Max(max, tile.Collider.Max);
                }
            }
        }

        Vector2 center = (min + max) / 2.0f;
        collider.Offset = Vector2.Floor(center - Transform.GlobalPosition).ToPoint();
        collider.Size = Vector2.Floor(max - min).ToPoint();
    }

    /// <summary>
    /// Converts a rectangle in pixel-space to tile-space
    /// </summary>
    /// <param name="rect">Rectangle to convert</param>
    /// <param name="padding">Number of tile padding to expand</param>
    /// <returns>Rectangle converted into tile-space</returns>
    public Rectangle PixelToTileSpace(Rectangle rect, int padding = 0) {
        rect.Location -= Vector2.Floor(
            Transform.GlobalPosition
        ).ToPoint();

        return new Rectangle(
            (int)MathF.Floor(rect.X / Tile<T>.PixelSize) - padding,
            (int)MathF.Floor(rect.Y / Tile<T>.PixelSize) - padding,
            (int)MathF.Ceiling(rect.Size.X / Tile<T>.PixelSize) + padding * 2,
            (int)MathF.Ceiling(rect.Size.Y / Tile<T>.PixelSize) + padding * 2
        );
    }

    /// <summary>
    /// Converts a Vector2 position in pixel-space to a tile-space Point
    /// </summary>
    /// <param name="pos">Pixel-space position to convert</param>
    /// <returns>Point coordinate converted into tile-space</returns>
    public Point PixelToTileSpace(Vector2 pos) {
        pos -= Transform.GlobalPosition - new Vector2(Tile<T>.PixelSize / 2);
        return new Point(
            (int)MathF.Floor(pos.X / Tile<T>.PixelSize),
            (int)MathF.Floor(pos.Y / Tile<T>.PixelSize)
        );
    }

    /// <summary>
    /// Converts a Point position in tile-space to a pixel-space Vector2
    /// </summary>
    /// <param name="pos">Tile-space position to convert</param>
    /// <returns>Vector2 coordinate converted into pixel-space</returns>
    public Vector2 TileToPixelSpace(Point pos) {
        Vector2 vec = pos.ToVector2() * Tile<T>.PixelSize;
        vec += Transform.GlobalPosition - new Vector2(Tile<T>.PixelSize / 2);
        return vec;
    }
}
