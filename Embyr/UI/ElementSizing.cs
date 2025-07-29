namespace Embyr.UI;

/// <summary>
/// Describes the type of behavior an element can follow in one of the axes when sizing
/// </summary>
public enum SizingBehavior {
    /// <summary>
    /// Element grows to fit all available space in parent element
    /// </summary>
    Grow,

    /// <summary>
    /// Element expands to exactly fit its inner contents
    /// </summary>
    Fit,

    /// <summary>
    /// Element stays one fixed size without dynamically sizing
    /// </summary>
    Fixed
}

/// <summary>
/// Describes the sizing parameters of an element in an arbitrary axis
/// </summary>
public struct ElementSizing {
    /// <summary>
    /// Creates an ElementSizing instance that fits inner contents
    /// </summary>
    /// <returns>A new ElementSizing instance</returns>
    public static ElementSizing Fit() => new() {
        Behavior = SizingBehavior.Fit,
        DesiredSize = 0
    };

    /// <summary>
    /// Creates an ElementSizing instance that grows to fill parent space
    /// </summary>
    /// <returns>A new ElementSizing instance</returns>
    public static ElementSizing Grow() => new() {
        Behavior = SizingBehavior.Grow,
        DesiredSize = 0
    };

    /// <summary>
    /// Creates an ElementSizing instance that stays at one fixed size without dynamic sizing
    /// </summary>
    /// <param name="size">Size to remain at</param>
    /// <returns>A new ElementSizing instance</returns>
    public static ElementSizing Fixed(int size) => new() {
        Behavior = SizingBehavior.Fixed,
        DesiredSize = size
    };

    /// <summary>
    /// Behavior of sizing in this axis
    /// </summary>
    public SizingBehavior Behavior;

    /// <summary>
    /// Desired size of this axis in pixels
    /// </summary>
    public int DesiredSize;

    /// <summary>
    /// Creates a new ElementSizing instance, defaults to Fit parameters
    /// </summary>
    public ElementSizing() {
        Behavior = SizingBehavior.Fit;
        DesiredSize = 0;
    }
}
