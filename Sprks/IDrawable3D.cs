using Microsoft.Xna.Framework.Graphics;

namespace Sprks;

/// <summary>
/// Objects that can be drawn to the screen
/// </summary>
public interface IDrawable3D {
    /// <summary>
    /// Draws this object to the screen
    /// </summary>
    /// <param name="camera">Scene camera to draw in accordance to</param
    public void Draw(Camera3D camera);
}
