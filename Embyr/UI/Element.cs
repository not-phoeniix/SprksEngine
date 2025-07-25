using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Embyr.UI;

// layout algorithm guidance/inspiration:
//   https://www.youtube.com/watch?v=by9lQvpvMIc

public enum AlignDirection {
    LeftToRight,
    TopToBottom
}

internal class Element {
    private string innerText;
    private Vector2 stringSize;

    public Rectangle Bounds;
    public Element? Parent;
    public readonly List<Element> Children;
    public ElementProperties Props;
    public bool Hovered { get; private set; }
    public bool Clickable;

    public string InnerText {
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

    public Element(ElementProperties properties) {
        this.Bounds = properties.GetInitialBounds();
        this.Props = properties;
        this.Children = new List<Element>();
    }

    public void CalcFitSizing() {
        //* reverse breadth-first search :D

        Bounds.Width += Props.Padding.Left + Props.Padding.Right;
        Bounds.Height += Props.Padding.Top + Props.Padding.Bottom;

        // fit text !!
        if (Props.XSizing.Behavior == SizingBehavior.Fit) {
            Bounds.Width += (int)stringSize.X;
        }
        if (Props.YSizing.Behavior == SizingBehavior.Fit) {
            Bounds.Height += (int)stringSize.Y;
        }

        // calculate additional space for gaps for children for THIS element
        int gap = Math.Max((Children.Count - 1) * Props.Gap, 0);
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

    public void GrowHorizontal() {
        //* breadth-first search :D

        int remainingWidth = Bounds.Width - (Props.Padding.Left + Props.Padding.Right);
        int remainingHeight = Bounds.Height - (Props.Padding.Top + Props.Padding.Bottom);

        // on axis remaining calculations
        foreach (Element child in Children) {
            remainingWidth -= child.Bounds.Width;
        }

        remainingWidth -= Math.Max((Children.Count - 1) * Props.Gap, 0);

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

    public void GrowVertical() {
        //* breadth-first search :D

        int remainingWidth = Bounds.Width - (Props.Padding.Left + Props.Padding.Right);
        int remainingHeight = Bounds.Height - (Props.Padding.Top + Props.Padding.Bottom);

        // on axis remaining calculations
        foreach (Element child in Children) {
            remainingHeight -= child.Bounds.Height;
        }

        remainingHeight -= Math.Max((Children.Count - 1) * Props.Gap, 0);

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

    public void CalcPositions() {
        //* breadth-first search :D

        int axisOffset = 0;
        if (Props.Direction == AlignDirection.LeftToRight) {
            axisOffset += Props.Padding.Left;
        } else {
            axisOffset += Props.Padding.Top;
        }

        for (int i = 0; i < Children.Count; i++) {
            if (Props.Direction == AlignDirection.LeftToRight) {
                // on axis
                Children[i].Bounds.X = Bounds.Left + axisOffset;
                // cross axis
                Children[i].Bounds.Y = Bounds.Top + Props.Padding.Top;
                // increment offset
                axisOffset += Children[i].Bounds.Width + Props.Gap;
            } else {
                // on axis
                Children[i].Bounds.Y = Bounds.Top + axisOffset;
                // cross axis
                Children[i].Bounds.X = Bounds.Left + Props.Padding.Left;
                // increment offset
                axisOffset += Children[i].Bounds.Height + Props.Gap;
            }

            Children[i].CalcPositions();
        }

        Hovered = Bounds.Contains(Input.MousePos);
    }

    public void ClearChildren(List<Element> destination) {
        foreach (Element child in Children) {
            destination.Add(child);
            child.ClearChildren(destination);
        }

        Children.Clear();
    }

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
