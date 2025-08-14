using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Embyr.UI;

/// <summary>
/// Describes an image to draw as a UI element
/// </summary>
public struct ImageProperties {
    /// <summary>
    /// Texture/image itself to draw
    /// </summary>
    public required Texture2D Texture;

    /// <summary>
    /// Color tint of image
    /// </summary>
    public Color Color;

    /// <summary>
    /// Optional source rectangle to draw only a part of an image
    /// </summary>
    public Rectangle? SourceRect;

    /// <summary>
    /// Optional manual size of image, X/Y represent Width/Height to resize image to
    /// </summary>
    public Point? ManualSize;

    /// <summary>
    /// Creates a new ImageProperties instance
    /// </summary>
    public ImageProperties() {
        Color = Color.White;
        SourceRect = null;
        ManualSize = null;
    }
}
