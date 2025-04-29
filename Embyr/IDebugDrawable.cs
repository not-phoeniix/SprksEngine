using Microsoft.Xna.Framework.Graphics;

namespace Embyr;

/// <summary>
/// Objects that can be drawn with debug information
/// </summary>
public interface IDebugDrawable {
    /// <summary>
    /// Draws debug information (hitboxes, bounds, vectors, etc)
    /// </summary>
    /// <param name="sb">SpriteBatch to draw with</param>
    public void DebugDraw(SpriteBatch sb);
}
