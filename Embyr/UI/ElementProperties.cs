using Microsoft.Xna.Framework;

namespace Embyr.UI;

public struct ElementProperties {
    public ElementStyle Style;

    public AlignDirection Direction { get; set; }
    public ElementSizing XSizing { get; set; }
    public ElementSizing YSizing { get; set; }
    public ElementPadding Padding { get; set; }

    public int Gap { get; set; }

    public ElementProperties() {
        Style = new ElementStyle();
        Direction = AlignDirection.LeftToRight;
        XSizing = ElementSizing.Fit();
        YSizing = ElementSizing.Fit();
        Padding = ElementPadding.Zero;
        Gap = 0;
    }

    public ElementProperties(ElementProperties other) {
        Style = other.Style;
        Direction = other.Direction;
        XSizing = other.XSizing;
        YSizing = other.YSizing;
        Padding = other.Padding;
        Gap = other.Gap;
    }
}
