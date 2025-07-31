using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Embyr.UI;

// layout algorithm guidance/inspiration:
//   https://www.youtube.com/watch?v=by9lQvpvMIc

/// <summary>
/// Immediate-mode static GUI (pronounced GOOEY) builder class
/// </summary>
public static class Gooey {
    private static readonly List<Element> rootElements = new();
    private static readonly List<Element> elementPool = new();
    private static readonly List<(Action, Element)> clickables = new();

    // tracks whatever the "current parent" is when calling begin/end
    private static Element? currentParent = null;

    /// <summary>
    /// Begins the creation of a basic element
    /// </summary>
    /// <param name="props">Element properties to describe the element to create</param>
    public static void BeginElement(ElementProperties props) {
        // grab/create new element from pool
        Element element = GetElementFromPool(props);

        // link up references
        if (currentParent != null) {
            currentParent.Children.Add(element);
            element.Parent = currentParent;
        } else {
            rootElements.Add(element);
        }

        // move down the reference tree
        currentParent = element;
    }

    /// <summary>
    /// Ends the creation of any element type, must be called to finalize any element heirarchies
    /// </summary>
    public static void End() {
        if (currentParent == null) {
            throw new Exception("Cannot call End, Begin has not been called!");
        }

        Element element = currentParent;
        element.CalcFitSizing();

        // we move up the reference tree
        currentParent = element.Parent;
    }

    /// <summary>
    /// Creates an element, runs begin and end automatically
    /// </summary>
    /// <param name="props">Element properties describing the element to create</param>
    public static void Element(ElementProperties props) {
        BeginElement(props);
        End();
    }

    /// <summary>
    /// Begins the creation of a clickable element
    /// </summary>
    /// <param name="props">Element properties to describe the element to create</param>
    /// <param name="onClicked">Action to execute when the clickable is clicked</param>
    public static void BeginClickable(ElementProperties props, Action onClicked) {
        BeginElement(props);
        clickables.Add((onClicked, currentParent!));
        currentParent!.Clickable = true;
    }

    /// <summary>
    /// Creates a clickable element, runs begin and end automatically
    /// </summary>
    /// <param name="props">Element properties to describe the element to create</param>
    /// <param name="onClicked">Action to execute when the clickable is clicked</param>
    public static void Clickable(ElementProperties props, Action onClicked) {
        BeginClickable(props, onClicked);
        End();
    }

    /// <summary>
    /// Creates a button element, runs begin and end automatically
    /// </summary>
    /// <param name="props">Element properties to describe the element to create</param>
    /// <param name="label">Label text to display on top of button</param>
    /// <param name="onClicked">Action to execute when the clickable is clicked</param>
    public static void Button(ElementProperties props, string label, Action onClicked) {
        BeginElement(props);
        clickables.Add((onClicked, currentParent!));
        currentParent!.Clickable = true;
        currentParent!.InnerText = label;
        End();
    }

    /// <summary>
    /// Creates a text element, runs begin and end automatically
    /// </summary>
    /// <param name="props">Element properties to describe the element to create</param>
    /// <param name="text">Text to display inside element</param>
    public static void TextElement(ElementProperties props, string text) {
        BeginElement(props);
        currentParent!.InnerText = text;
        End();
    }

    /// <summary>
    /// Creates an image element, runs begin and end automatically
    /// </summary>
    /// <param name="elementProps">Element properties to describe the element to create</param>
    /// <param name="imageProps">Image properties to describe this element's image</param>
    public static void ImageElement(ElementProperties elementProps, ImageProperties imageProps) {
        BeginElement(elementProps);
        currentParent!.ImageProps = imageProps;
        End();
    }

    /// <summary>
    /// Validates the element tree and throws an exception if the user missed an End() call
    /// </summary>
    internal static void ValidateTree() {
        if (currentParent != null) {
            throw new Exception("Tree is imbalanced, missing an End() call somewhere!");
        }
    }

    /// <summary>
    /// Traverses element tree and calculates all sizing for growable elements
    /// </summary>
    internal static void CalcGrowSizing() {
        foreach (Element element in rootElements) {
            element.CalcGrowSizing();
        }
    }

    /// <summary>
    /// Traverses element tree and calculates positions of all elements
    /// </summary>
    internal static void CalcPositions() {
        foreach (Element element in rootElements) {
            element.CalcPositions();
        }
    }

    /// <summary>
    /// Draws all elements to the screen
    /// </summary>
    /// <param name="sb">SpriteBatch to draw with</param>
    internal static void DrawAll(SpriteBatch sb) {
        foreach (Element element in rootElements) {
            element.Draw(sb);
        }
    }

    /// <summary>
    /// Activates all clickable elements if they are clicked this frame
    /// </summary>
    internal static void ActivateClickables() {
        foreach ((Action, Element) pair in clickables) {
            Action action = pair.Item1;
            Element element = pair.Item2;
            if (element.Hovered && Input.IsLeftMouseDownOnce()) {
                action?.Invoke();
            }
        }

        clickables.Clear();
    }

    /// <summary>
    /// Resets the element pool, clearing out all elements
    /// </summary>
    internal static void ResetPool() {
        foreach (Element element in rootElements) {
            element.ClearChildren(elementPool);
            elementPool.Add(element);
        }

        rootElements.Clear();
    }

    private static Element GetElementFromPool(ElementProperties properties) {
        Element element;

        if (elementPool.Count > 0) {
            element = elementPool[elementPool.Count - 1];
            element.Reset(properties);
            elementPool.RemoveAt(elementPool.Count - 1);
        } else {
            element = new Element(properties);
        }

        return element;
    }
}
