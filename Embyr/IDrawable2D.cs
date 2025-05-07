using Microsoft.Xna.Framework.Graphics;

namespace Embyr;

/// <summary>
/// Objects that can be drawn to the screen
/// </summary>
public interface IDrawable2D {
    /// <summary>
    /// Draws this object to the screen
    /// </summary>
    /// <param name="sb">SpriteBatch to draw with</param>
    public void Draw(SpriteBatch sb);
}
