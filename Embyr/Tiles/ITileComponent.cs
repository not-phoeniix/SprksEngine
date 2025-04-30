namespace Embyr.Tiles;

/// <summary>
/// A tile component that can be instantiated in the tile
/// class that contains special extra functionality for tiles
/// </summary>
public interface ITileComponent<T> : IDrawable, IDebugDrawable where T : Enum {
    /// <summary>
    /// Gets the tile associated with this tile component
    /// </summary>
    public Tile<T> Tile { get; }

    /// <summary>
    /// Updates this tile component logic
    /// </summary>
    /// <param name="dt">Time passed since last frame</param>
    public void Update(float dt);

    /// <summary>
    /// Updates the physics logic of this component
    /// </summary>
    /// <param name="fdt">Time passed since last physics update</param>
    public void PhysicsUpdate(float fdt);
}
