namespace Sprks.UI;

/// <summary>
/// Describes the padding on the edges of an element
/// </summary>
public struct ElementPadding {
    /// <summary>
    /// Padding preset that represents zero padding on all edges
    /// </summary>
    public static readonly ElementPadding Zero = new(0);

    /// <summary>
    /// Gets the amount of padding on the left edge in pixels
    /// </summary>
    public int Left { get; init; }

    /// <summary>
    /// Gets the amount of padding on the right edge in pixels
    /// </summary>
    public int Right { get; init; }

    /// <summary>
    /// Gets the amount of padding on the top edge in pixels
    /// </summary>
    public int Top { get; init; }

    /// <summary>
    /// Gets the amount of padding on the bottom edge in pixels
    /// </summary>
    public int Bottom { get; init; }

    /// <summary>
    /// Creates a new ElementPadding instance
    /// </summary>
    public ElementPadding() {
        Left = 0;
        Right = 0;
        Top = 0;
        Bottom = 0;
    }

    /// <summary>
    /// Creates a new ElementPadding instance
    /// </summary>
    /// <param name="top">Top edge padding in pixels</param>
    /// <param name="right">Right edge padding in pixels</param>
    /// <param name="bottom">Bottom edge padding in pixels</param>
    /// <param name="left">Left edge padding in pixels</param>
    public ElementPadding(int top, int right, int bottom, int left) {
        Top = top;
        Right = right;
        Bottom = bottom;
        Left = left;
    }

    /// <summary>
    /// Creates a new ElementPadding instance
    /// </summary>
    /// <param name="topBottom">Top and bottom edge padding in pixels</param>
    /// <param name="leftRight">Left and right edge padding in pixels</param>
    public ElementPadding(int topBottom, int leftRight) {
        Top = topBottom;
        Bottom = topBottom;
        Left = leftRight;
        Right = leftRight;
    }

    /// <summary>
    /// Creates a new ElementPadding instance
    /// </summary>
    /// <param name="padding">Padding of all edges in pixels</param>
    public ElementPadding(int padding) {
        Top = padding;
        Bottom = padding;
        Left = padding;
        Right = padding;
    }
}
