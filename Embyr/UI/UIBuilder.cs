using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Embyr.UI;

// layout algo guidance/inspiration:
//   https://www.youtube.com/watch?v=by9lQvpvMIc

public enum AlignDirection {
    LeftToRight,
    TopToBottom
}

public static class UIBuilder {
    private static readonly List<Element> rootElements = new();
    private static readonly List<Element> elementPool = new();

    // tracks whatever the "current parent" is when calling begin/end
    private static Element? currentParent = null;

    public static void Begin(ElementProperties props) {
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
        element.CalcSizing();

        // we move up the reference tree
        currentParent = element.Parent;
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
            element.Bounds = new Rectangle(
                0,
                0,
                properties.XSizing.DesiredSize,
                properties.YSizing.DesiredSize
            );
            element.Children.Clear();
            element.Parent = null;
            elementPool.RemoveAt(elementPool.Count - 1);
        } else {
            element = new Element(properties);
        }

        return element;
    }
}
