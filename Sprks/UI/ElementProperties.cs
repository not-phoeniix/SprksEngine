using Microsoft.Xna.Framework;

namespace Sprks.UI;

/// <summary>
/// Describes the direction an element can align its children in
/// </summary>
public enum AlignDirection {
    /// <summary>
    /// Align elements left-to-right horizontally
    /// </summary>
    LeftToRight,

    /// <summary>
    /// Align elements top-to-bottom vertically
    /// </summary>
    TopToBottom
}

/// <summary>
/// Describes alignment positions of an element relative to its parent
/// </summary>
public enum ElementAlignment {
    /// <summary>
    /// Aligns the element at the start of the parent box
    /// </summary>
    Start,

    /// <summary>
    /// Aligns the element in the center of the parent box
    /// </summary>
    Center,

    /// <summary>
    /// Aligns the element at the end of the parent box
    /// </summary>
    End
}

/// <summary>
/// Describes the properties to define an element in UI
/// </summary>
public struct ElementProperties {
    /// <summary>
    /// Styling rules for this element
    /// </summary>
    public ElementStyle Style;

    /// <summary>
    /// Gets/sets the direction to align children
    /// </summary>
    public AlignDirection Direction { get; set; }

    /// <summary>
    /// Gets/sets the element sizing in the X direction
    /// </summary>
    public ElementSizing XSizing { get; set; }

    /// <summary>
    /// Gets/sets the element sizing in the Y direction
    /// </summary>
    public ElementSizing YSizing { get; set; }

    /// <summary>
    /// Gets/sets how to align the position of this element relative to its parent on the x axis
    /// </summary>
    public ElementAlignment XAlignment { get; set; }

    /// <summary>
    /// Gets/sets how to align the position of this element relative to its parent on the y axis
    /// </summary>
    public ElementAlignment YAlignment { get; set; }

    /// <summary>
    /// Creates a new ElementProperties instance
    /// </summary>
    public ElementProperties() {
        Style = new ElementStyle();
        Direction = AlignDirection.LeftToRight;
        XSizing = ElementSizing.Fit();
        YSizing = ElementSizing.Fit();
        XAlignment = ElementAlignment.Start;
        YAlignment = ElementAlignment.Start;
    }

    /// <summary>
    /// Copies an ElementProperties struct from an existing instance
    /// </summary>
    /// <param name="other">Existing properties to copy</param>
    public ElementProperties(ElementProperties other) {
        Style = other.Style;
        Direction = other.Direction;
        XSizing = other.XSizing;
        YSizing = other.YSizing;
        XAlignment = other.XAlignment;
        YAlignment = other.YAlignment;
    }

    /// <summary>
    /// Gets the calculated initial Rectangle bounds depending on set sizing rules
    /// </summary>
    /// <returns>A new Rectangle that represents initial element bounds</returns>
    internal readonly Rectangle GetInitialBounds() {
        int w = 0;
        if (XSizing.Behavior == SizingBehavior.Fixed) {
            w = XSizing.DesiredSize;
        }

        int h = 0;
        if (YSizing.Behavior == SizingBehavior.Fixed) {
            h = YSizing.DesiredSize;
        }

        return new Rectangle(0, 0, w, h);
    }
}
