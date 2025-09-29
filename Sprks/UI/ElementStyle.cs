using Microsoft.Xna.Framework;

namespace Sprks.UI;

/// <summary>
/// Object containing all properties describing how to describe
/// </summary>
public struct ElementStyle {
    /// <summary>
    /// Creates an empty and transparent element style, where theres no font and every color is transparent
    /// </summary>
    /// <returns>A new empty/transparent ElementStyle</returns>
    public static ElementStyle EmptyTransparent() => EmptyTransparent(null, Color.Black);

    /// <summary>
    /// Creates an empty and transparent element style used for text
    /// </summary>
    /// <param name="font">Font of text to use</param>
    /// <param name="textColor">Color of text to use</param>
    /// <returns>A new empty/transparent ElementStyle</returns>
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
    /// Gets/sets the padding rules of the element
    /// </summary>
    public ElementPadding Padding { get; set; }

    /// <summary>
    /// Gets/sets the pixel margin/gap between aligned child elements
    /// </summary>
    public int Gap { get; set; }

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
        Padding = ElementPadding.Zero;
        Gap = 0;
    }

    /// <summary>
    /// Clones from an existing element style
    /// </summary>
    /// <param name="other">Style to copy from</param>
    public ElementStyle(ElementStyle other) {
        BackgroundColor = other.BackgroundColor;
        Color = other.Color;
        HoverColor = other.HoverColor;
        BorderColor = other.BorderColor;
        ActiveColor = other.ActiveColor;
        InactiveColor = other.InactiveColor;
        BorderSize = other.BorderSize;
        Font = other.Font;
        Padding = other.Padding;
        Gap = other.Gap;
    }
}
