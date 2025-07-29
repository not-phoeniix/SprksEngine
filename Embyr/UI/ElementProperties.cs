using Microsoft.Xna.Framework;

namespace Embyr.UI;

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
    /// Gets/sets the padding rules of the element
    /// </summary>
    public ElementPadding Padding { get; set; }

    /// <summary>
    /// Gets/sets the pixel gap between children when aligning
    /// </summary>
    public int Gap { get; set; }

    /// <summary>
    /// Creates a new ElementProperties instance
    /// </summary>
    public ElementProperties() {
        Style = new ElementStyle();
        Direction = AlignDirection.LeftToRight;
        XSizing = ElementSizing.Fit();
        YSizing = ElementSizing.Fit();
        Padding = ElementPadding.Zero;
        Gap = 0;
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
        Padding = other.Padding;
        Gap = other.Gap;
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
