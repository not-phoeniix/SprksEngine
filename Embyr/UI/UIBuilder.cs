using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Embyr.UI;

// layout algorithm guidance/inspiration:
//   https://www.youtube.com/watch?v=by9lQvpvMIc

public static class UIBuilder {
    private static readonly List<Element> rootElements = new();
    private static readonly List<Element> elementPool = new();
    private static readonly List<(Action, Element)> clickables = new();

    // tracks whatever the "current parent" is when calling begin/end
    private static Element? currentParent = null;

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

    public static void End() {
        if (currentParent == null) {
            throw new Exception("Cannot call End, Begin has not been called!");
        }

        Element element = currentParent;
        element.CalcFitSizing();

        // we move up the reference tree
        currentParent = element.Parent;
    }

    public static void Element(ElementProperties props) {
        BeginElement(props);
        End();
    }

    public static void BeginClickable(ElementProperties props, Action onClicked) {
        BeginElement(props);
        clickables.Add((onClicked, currentParent!));
        currentParent!.Clickable = true;
    }

    public static void Clickable(ElementProperties props, Action onClicked) {
        BeginClickable(props, onClicked);
        End();
    }

    public static void Text(ElementProperties props, string text) {
    }

    internal static void CalcGrowSizing() {
        foreach (Element element in rootElements) {
            element.CalcGrowSizing();
        }
    }

    internal static void CalcPositions() {
        foreach (Element element in rootElements) {
            element.CalcPositions();
        }
    }

    internal static void DrawAll(SpriteBatch sb) {
        foreach (Element element in rootElements) {
            element.Draw(sb);
        }
    }

    internal static void DrawAllDebug(SpriteBatch sb) {
    }

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
            element.Props = properties;
            element.Bounds = properties.GetInitialBounds();
            element.Children.Clear();
            element.Parent = null;
            element.Clickable = false;
            elementPool.RemoveAt(elementPool.Count - 1);
        } else {
            element = new Element(properties);
        }

        return element;
    }
}
