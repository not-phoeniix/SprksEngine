using Microsoft.Xna.Framework;

namespace Embyr.UI;

/// <summary>
/// A structure that contains the styling rules for any MenuElement
/// </summary>
public struct ElementStyle {
    public static ElementStyle EmptyTransparent() => EmptyTransparent(null, Color.Black);

    public static ElementStyle EmptyTransparent(Font font, Color textColor) => new() {
        BackgroundColor = Color.Transparent,
        Color = textColor,
        HoverColor = Color.Transparent,
        ActiveColor = Color.Transparent,
        InactiveColor = Color.Transparent,
        BorderColor = Color.Transparent,
        Font = font
    };

    /// <summary>
    /// Color of background, everything within border
    /// </summary>
    public Color BackgroundColor { get; set; }

    /// <summary>
    /// Color of foreground content (typically text)
    /// </summary>
    public Color Color { get; set; }

    /// <summary>
    /// Color when an element is hovered over
    /// </summary>
    public Color HoverColor { get; set; }

    /// <summary>
    /// Color of element when it is "active" or selected,
    /// determined by individual element class
    /// </summary>
    public Color ActiveColor { get; set; }

    /// <summary>
    /// Color of element when it's "inactive," used
    /// for things like disabled elements or things
    /// like typeboxes before they begin typing
    /// </summary>
    public Color InactiveColor { get; set; }

    /// <summary>
    /// Color of border
    /// </summary>
    public Color BorderColor { get; set; }

    /// <summary>
    /// Pixel size of border surrounding this element
    /// </summary>
    public int BorderSize { get; set; }

    /// <summary>
    /// Gets/sets the font that text is rendered with
    /// </summary>
    public Font Font { get; set; }

    /// <summary>
    /// Creates a new ElementStyle structure
    /// </summary>
    public ElementStyle() {
        BackgroundColor = Color.Black;
        Color = Color.White;
        HoverColor = Color.Gray;
        BorderColor = Color.Transparent;
        ActiveColor = Color.Red;
        InactiveColor = Color.Gray;
        BorderSize = 0;
    }

    /// <summary>
    /// Clones from an existing element style
    /// </summary>
    /// <param name="style">Style to copy from</param>
    public ElementStyle(ElementStyle style) {
        BackgroundColor = style.BackgroundColor;
        Color = style.Color;
        HoverColor = style.HoverColor;
        BorderColor = style.BorderColor;
        ActiveColor = style.ActiveColor;
        InactiveColor = style.InactiveColor;
        BorderSize = style.BorderSize;
        Font = style.Font;
    }
}
