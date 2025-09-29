using Microsoft.Xna.Framework.Graphics;

namespace Sprks;

/// <summary>
/// Objects that can be drawn with debug information
/// </summary>
public interface IDebugDrawable2D {
    /// <summary>
    /// Draws debug information (hitboxes, bounds, vectors, etc)
    /// </summary>
    /// <param name="sb">SpriteBatch to draw with</param>
    public void DebugDraw(SpriteBatch sb);
}
