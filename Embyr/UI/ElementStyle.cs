using Microsoft.Xna.Framework;

namespace Embyr.UI;

/// <summary>
/// Vertical alignment of Position property within a MenuElement. Where the
/// Position is anchored vertically relative to the bounds of the element.
/// </summary>
public enum YAlign {
    /// <summary>
    /// Y component of Position is the location of the top of the element
    /// </summary>
    Top,

    /// <summary>
    /// Y component of Position is the location of the vertical center of the element
    /// </summary>
    Center,

    /// <summary>
    /// Y component of Position is the location of the bottom of the element
    /// </summary>
    Bottom
}

/// <summary>
/// Horizontal alignment of Position property within a MenuElement. Where the
/// Position is anchored horizontally relative to the bounds of the element.
/// </summary>
public enum XAlign {
    /// <summary>
    /// X component of Position is the location of the left of the element
    /// </summary>
    Left,

    /// <summary>
    /// X component of Position is the location of the horizontal center of the element
    /// </summary>
    Center,

    /// <summary>
    /// X component of Position is the location of the left of the element
    /// </summary>
    Right
}

/// <summary>
/// A structure that contains the styling rules for any MenuElement
/// </summary>
public struct ElementStyle {
    /// <summary>
    /// Vertical alignment of the element
    /// </summary>
    public YAlign YAlignment { get; set; }

    /// <summary>
    /// Horizontal alignment of the element
    /// </summary>
    public XAlign XAlignment { get; set; }

    /// <summary>
    /// Font string for an aseprite font that's been loaded
    /// </summary>
    public AFont Font { get; set; }

    /// <summary>
    /// Pixel space between content and border
    /// </summary>
    public int Padding { get; set; }

    /// <summary>
    /// Pixel space outside of border
    /// </summary>
    public int Margin { get; set; }

    /// <summary>
    /// Color of background, everything within border
    /// </summary>
    public Color BackgroundColor { get; set; }

    /// <summary>
    /// Color of foreground content (typically text)
    /// </summary>
    public Color ForegroundColor { get; set; }

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
    /// Creates a new ElementStyle structure
    /// </summary>
    public ElementStyle() {
        XAlignment = XAlign.Center;
        YAlignment = YAlign.Center;
        Font = ContentHelper.I.LoadGlobal<AFont>("fonts/med_font");
        Padding = 4;
        Margin = 1;
        BackgroundColor = Palette.Col1;
        ForegroundColor = Palette.Col4;
        HoverColor = Palette.Col7;
        BorderColor = Color.Transparent;
        ActiveColor = Palette.Col3;
        InactiveColor = Palette.Col2;
        BorderSize = 0;
    }

    /// <summary>
    /// Clones from an existing element style
    /// </summary>
    /// <param name="style">Style to copy from</param>
    public ElementStyle(ElementStyle style) {
        XAlignment = style.XAlignment;
        YAlignment = style.YAlignment;
        Font = style.Font;
        Padding = style.Padding;
        Margin = style.Margin;
        BackgroundColor = style.BackgroundColor;
        ForegroundColor = style.ForegroundColor;
        HoverColor = style.HoverColor;
        BorderColor = style.BorderColor;
        ActiveColor = style.ActiveColor;
        BorderSize = style.BorderSize;
    }
}
