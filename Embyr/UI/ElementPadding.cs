namespace Embyr.UI;

public struct ElementPadding {
    public static readonly ElementPadding Zero = new(0);

    public int Left { get; init; }
    public int Right { get; init; }
    public int Top { get; init; }
    public int Bottom { get; init; }

    public ElementPadding() {
        Left = 0;
        Right = 0;
        Top = 0;
        Bottom = 0;
    }

    public ElementPadding(int top, int right, int bottom, int left) {
        Top = top;
        Right = right;
        Bottom = bottom;
        Left = left;
    }

    public ElementPadding(int topBottom, int leftRight) {
        Top = topBottom;
        Bottom = topBottom;
        Left = leftRight;
        Right = leftRight;
    }

    public ElementPadding(int padding) {
        Top = padding;
        Bottom = padding;
        Left = padding;
        Right = padding;
    }
}
