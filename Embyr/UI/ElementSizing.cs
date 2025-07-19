namespace Embyr.UI;

public enum SizingBehavior {
    Grow,
    Fit,
    Fixed
}

public struct ElementSizing {
    public static ElementSizing Fit() => new() {
        Behavior = SizingBehavior.Fit,
        DesiredSize = 0
    };

    public static ElementSizing Grow() => new() {
        Behavior = SizingBehavior.Grow,
        DesiredSize = 0
    };

    public static ElementSizing Fixed(int size) => new() {
        Behavior = SizingBehavior.Fixed,
        DesiredSize = size
    };

    public SizingBehavior Behavior;
    public int DesiredSize;

    public ElementSizing() {
        Behavior = SizingBehavior.Fit;
        DesiredSize = 0;
    }
}
