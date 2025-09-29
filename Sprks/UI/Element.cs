using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sprks.UI;

// layout algorithm guidance/inspiration:
//   https://www.youtube.com/watch?v=by9lQvpvMIc

/// <summary>
/// A UI element node as part of a tree data structure for the UI hierarchy
/// </summary>
internal class Element {
    private string? innerText;
    private Vector2 stringSize;
    private Point imageSize;

    /// <summary>
    /// Rectangle bounds of this element
    /// </summary>
    public Rectangle Bounds;

    /// <summary>
    /// Gets/sets the reference to the parent element of this element
    /// </summary>
    public Element? Parent { get; set; }

    /// <summary>
    /// List of children elements of this element
    /// </summary>
    public readonly List<Element> Children;

    /// <summary>
    /// Gets the element properties describing this element
    /// </summary>
    public ElementProperties Props { get; private set; }

    /// <summary>
    /// Gets whether or not this element is hovered over by the mouse
    /// </summary>
    public bool Hovered { get; private set; }

    /// <summary>
    /// Gets/sets whether or not this element is clickable
    /// </summary>
    public bool Clickable { get; set; }

    /// <summary>
    /// Gets/sets the inner text of this element, can be null
    /// </summary>
    public string? InnerText {
        get => innerText;
        set {
            if (innerText != value) {
                if (!string.IsNullOrEmpty(value) && Props.Style.Font != null) {
                    stringSize = Props.Style.Font.MeasureString(value);
                } else {
                    stringSize = Vector2.Zero;
                }
            }

            innerText = value;
        }
    }

    /// <summary>
    /// Gets/sets the image properties to use when rendering images on this element
    /// </summary>
    public ImageProperties? ImageProps { get; set; }

    /// <summary>
    /// Creates a new Element instance
    /// </summary>
    /// <param name="properties">Element properties to describe this element</param>
    public Element(ElementProperties properties) {
        this.Bounds = properties.GetInitialBounds();
        this.Props = properties;
        this.Children = new List<Element>();
    }

    /// <summary>
    /// Resets this existing element instance to its initial state
    /// </summary>
    /// <param name="properties">Element properties to describe this element</param>
    public void Reset(ElementProperties properties) {
        Props = properties;
        Bounds = properties.GetInitialBounds();
        ImageProps = null;
        Children.Clear();
        Parent = null;
        InnerText = "";
        Clickable = false;
    }

    /// <summary>
    /// Calculates the sizing of this element and its children on axes that use fit behavior
    /// </summary>
    public void CalcFitSizing() {
        //* reverse breadth-first search :D

        Bounds.Width += Props.Style.Padding.Left + Props.Style.Padding.Right;
        Bounds.Height += Props.Style.Padding.Top + Props.Style.Padding.Bottom;

        if (ImageProps != null) {
            ImageProperties props = ImageProps.Value;

            imageSize.X = props.ManualSize?.X
                       ?? props.SourceRect?.Width
                       ?? props.Texture.Width;

            imageSize.Y = props.ManualSize?.Y
                       ?? props.SourceRect?.Height
                       ?? props.Texture.Height;

        } else {
            imageSize = Point.Zero;
        }

        // fit text & image !!
        if (Props.XSizing.Behavior == SizingBehavior.Fit) {
            Bounds.Width += Math.Max((int)stringSize.X, imageSize.X);
        }
        if (Props.YSizing.Behavior == SizingBehavior.Fit) {
            Bounds.Height += Math.Max((int)stringSize.Y, imageSize.Y);
        }

        // calculate additional space for gaps for children for THIS element
        int gap = Math.Max((Children.Count - 1) * Props.Style.Gap, 0);
        if (Props.Direction == AlignDirection.LeftToRight) {
            Bounds.Width += gap;
        } else {
            Bounds.Height += gap;
        }

        if (Parent == null) return;

        if (Parent.Props.Direction == AlignDirection.LeftToRight) {
            // on axis
            if (Parent.Props.XSizing.Behavior == SizingBehavior.Fit) {
                Parent.Bounds.Width += Bounds.Width;
            }

            // cross axis
            if (Parent.Props.YSizing.Behavior == SizingBehavior.Fit) {
                Parent.Bounds.Height = Math.Max(Parent.Bounds.Height, Bounds.Height);
            }

        } else {
            // on axis
            if (Parent.Props.YSizing.Behavior == SizingBehavior.Fit) {
                Parent.Bounds.Height += Bounds.Height;
            }

            // cross axis
            if (Parent.Props.XSizing.Behavior == SizingBehavior.Fit) {
                Parent.Bounds.Width = Math.Max(Parent.Bounds.Width, Bounds.Width);
            }
        }
    }

    /// <summary>
    /// Calculates the sizing of this element and its children on axes that use grow behavior
    /// </summary>
    public void CalcGrowSizing() {
        // grow to fit entire screen if root element !
        if (Parent == null) {
            if (Props.XSizing.Behavior == SizingBehavior.Grow) {
                Bounds.Width = Math.Max(EngineSettings.GameCanvasResolution.X, Bounds.Width);
            }

            if (Props.YSizing.Behavior == SizingBehavior.Grow) {
                Bounds.Height = Math.Max(EngineSettings.GameCanvasResolution.Y, Bounds.Height);
            }
        }

        if (Props.Direction == AlignDirection.LeftToRight) {
            GrowHorizontal();
        } else {
            GrowVertical();
        }

        foreach (Element child in Children) {
            child.CalcGrowSizing();
        }
    }

    private void GrowHorizontal() {
        //* breadth-first search :D

        int remainingWidth = Bounds.Width - (Props.Style.Padding.Left + Props.Style.Padding.Right);
        int remainingHeight = Bounds.Height - (Props.Style.Padding.Top + Props.Style.Padding.Bottom);

        // on axis remaining calculations
        foreach (Element child in Children) {
            remainingWidth -= child.Bounds.Width;
        }

        remainingWidth -= Math.Max((Children.Count - 1) * Props.Style.Gap, 0);

        while (remainingWidth > 0) {
            int smallestWidth = int.MaxValue;
            int secondSmallestWidth = int.MaxValue;
            int widthToAdd = remainingWidth;
            int numGrowables = 0;

            foreach (Element child in Children) {
                // skip non-growing children
                if (child.Props.XSizing.Behavior != SizingBehavior.Grow) continue;

                if (child.Bounds.Width < smallestWidth) {
                    secondSmallestWidth = smallestWidth;
                    smallestWidth = child.Bounds.Width;
                }

                if (child.Bounds.Width > smallestWidth) {
                    secondSmallestWidth = Math.Min(secondSmallestWidth, child.Bounds.Width);
                    widthToAdd = secondSmallestWidth - smallestWidth;
                }

                numGrowables++;
            }

            if (numGrowables == 0) break;

            // float divide and ciel this operation to make
            //   sure zero is actually reached so this loop ends
            widthToAdd = Math.Min(
                widthToAdd,
                (int)MathF.Ceiling((float)remainingWidth / numGrowables)
            );

            // apply dynamic on-axis sizing
            foreach (Element child in Children) {
                if (child.Bounds.Width == smallestWidth) {
                    child.Bounds.Width += widthToAdd;
                    remainingWidth -= widthToAdd;
                }
            }
        }

        // apply cross-axis sizing
        foreach (Element child in Children) {
            if (child.Props.YSizing.Behavior == SizingBehavior.Grow) {
                child.Bounds.Height += remainingHeight - child.Bounds.Height;
            }
        }
    }

    private void GrowVertical() {
        //* breadth-first search :D

        int remainingWidth = Bounds.Width - (Props.Style.Padding.Left + Props.Style.Padding.Right);
        int remainingHeight = Bounds.Height - (Props.Style.Padding.Top + Props.Style.Padding.Bottom);

        // on axis remaining calculations
        foreach (Element child in Children) {
            remainingHeight -= child.Bounds.Height;
        }

        remainingHeight -= Math.Max((Children.Count - 1) * Props.Style.Gap, 0);

        while (remainingHeight > 0) {
            int smallestHeight = int.MaxValue;
            int secondSmallestHeight = int.MaxValue;
            int heightToAdd = remainingHeight;
            int numGrowables = 0;

            foreach (Element child in Children) {
                // skip non-growing children
                if (child.Props.YSizing.Behavior != SizingBehavior.Grow) continue;

                if (child.Bounds.Height < smallestHeight) {
                    secondSmallestHeight = smallestHeight;
                    smallestHeight = child.Bounds.Height;
                }

                if (child.Bounds.Height > smallestHeight) {
                    secondSmallestHeight = Math.Min(secondSmallestHeight, child.Bounds.Height);
                    heightToAdd = secondSmallestHeight - smallestHeight;
                }

                numGrowables++;
            }

            if (numGrowables == 0) break;

            // float divide and ciel this operation to make
            //   sure zero is actually reached so this loop ends
            heightToAdd = Math.Min(
                heightToAdd,
                (int)MathF.Ceiling((float)remainingHeight / numGrowables)
            );

            // apply dynamic on-axis sizing
            foreach (Element child in Children) {
                if (child.Bounds.Height == smallestHeight) {
                    child.Bounds.Height += heightToAdd;
                    remainingHeight -= heightToAdd;
                }
            }
        }

        // apply cross-axis sizing
        foreach (Element child in Children) {
            if (child.Props.XSizing.Behavior == SizingBehavior.Grow) {
                child.Bounds.Width += remainingWidth - child.Bounds.Width;
            }
        }
    }

    /// <summary>
    /// Calculates the position of this element and its children
    /// </summary>
    public void CalcPositions() {
        //* breadth-first search :D

        int axisOffset = 0;
        if (Props.Direction == AlignDirection.LeftToRight) {
            axisOffset += Props.Style.Padding.Left;
        } else {
            axisOffset += Props.Style.Padding.Top;
        }

        bool xIsOnAxis = Parent?.Props.Direction == AlignDirection.LeftToRight;
        Point parentSize = EngineSettings.GameCanvasResolution;
        if (Parent != null) {
            parentSize = Parent.Bounds.Size;
            parentSize.X -= Parent.Props.Style.Padding.Left;
            parentSize.X -= Parent.Props.Style.Padding.Right;
            parentSize.Y -= Parent.Props.Style.Padding.Top;
            parentSize.Y -= Parent.Props.Style.Padding.Bottom;
        }

        // x alignment
        if (Parent == null || (xIsOnAxis && Parent.Children.Count == 1) || !xIsOnAxis) {
            int freeSpace = parentSize.X - Bounds.Width;

            Bounds.X += Props.XAlignment switch {
                ElementAlignment.Center => freeSpace / 2,
                ElementAlignment.End => freeSpace,
                _ => 0
            };
        }

        // y alignment
        if (Parent == null || (!xIsOnAxis && Parent.Children.Count == 1) || xIsOnAxis) {
            int freeSpace = parentSize.Y - Bounds.Height;

            Bounds.Y += Props.YAlignment switch {
                ElementAlignment.Center => freeSpace / 2,
                ElementAlignment.End => freeSpace,
                _ => 0
            };
        }

        for (int i = 0; i < Children.Count; i++) {
            if (Props.Direction == AlignDirection.LeftToRight) {
                // on axis
                Children[i].Bounds.X = Bounds.Left + axisOffset;
                // cross axis
                Children[i].Bounds.Y = Bounds.Top + Props.Style.Padding.Top;
                // increment offset
                axisOffset += Children[i].Bounds.Width + Props.Style.Gap;
            } else {
                // on axis
                Children[i].Bounds.Y = Bounds.Top + axisOffset;
                // cross axis
                Children[i].Bounds.X = Bounds.Left + Props.Style.Padding.Left;
                // increment offset
                axisOffset += Children[i].Bounds.Height + Props.Style.Gap;
            }

            Children[i].CalcPositions();
        }

        Hovered = Bounds.Contains(Input.MousePos);
    }

    /// <summary>
    /// Clears all children recursively and places children in a list
    /// </summary>
    /// <param name="destination">Destination list to place children within</param>
    public void ClearChildren(List<Element> destination) {
        foreach (Element child in Children) {
            destination.Add(child);
            child.ClearChildren(destination);
        }

        Children.Clear();
    }

    /// <summary>
    /// Draws this element and its children
    /// </summary>
    /// <param name="sb">SpriteBatch to draw with</param>
    public void Draw(SpriteBatch sb) {
        //* breadth-first search :D

        if (Props.Style.BorderSize > 0) {
            Rectangle borderBounds = Bounds;
            borderBounds.Inflate(Props.Style.BorderSize, Props.Style.BorderSize);
            sb.DrawRectOutline(borderBounds, Props.Style.BorderSize, Props.Style.BorderColor);
        }

        if (Hovered && Clickable) {
            sb.DrawRectFill(Bounds, Props.Style.HoverColor);
        } else {
            sb.DrawRectFill(Bounds, Props.Style.BackgroundColor);
        }

        if (ImageProps != null) {
            ImageProperties props = ImageProps.Value;

            Vector2 imagePos = Bounds.Center.ToVector2() - (imageSize.ToVector2() / 2.0f);
            Rectangle dest = new(
                Vector2.Floor(imagePos).ToPoint(),
                imageSize
            );

            sb.Draw(props.Texture, dest, props.SourceRect, props.Color);
        }

        if (!string.IsNullOrEmpty(innerText) && Props.Style.Font != null) {
            Vector2 centeredTextPos = Bounds.Center.ToVector2();
            centeredTextPos -= Vector2.Ceiling(stringSize / 2);
            centeredTextPos.Y += 1;

            sb.DrawString(
                Props.Style.Font,
                innerText,
                centeredTextPos,
                Props.Style.Color
            );
        }

        foreach (Element child in Children) {
            child.Draw(sb);
        }
    }
}
