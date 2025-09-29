namespace Sprks;

/// <summary>
/// Objects that can be drawn with debug information
/// </summary>
public interface IDebugDrawable3D {
    /// <summary>
    /// Draws debug information (hitboxes, bounds, vectors, etc)
    /// </summary>
    /// <param name="camera">Camera of scene to draw with</param>
    public void DebugDraw(Camera3D camera);
}
