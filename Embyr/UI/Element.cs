using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Embyr.UI;

public enum AlignDirection {
    LeftToRight,
    TopToBottom
}

internal class Element {
    public Rectangle Bounds;
    public Element? Parent;
    public readonly List<Element> Children;
    public ElementProperties Props;
    public bool Hovered { get; private set; }
    public bool Clickable;

    public Element(ElementProperties properties) {
        Bounds = new Rectangle(
            0,
            0,
            properties.XSizing.DesiredSize,
            properties.YSizing.DesiredSize
        );
        this.Props = properties;
        this.Children = new List<Element>();
    }

    public void CalcSizing() {
        Bounds.Width += Props.Padding.Left + Props.Padding.Right;
        Bounds.Height += Props.Padding.Top + Props.Padding.Bottom;

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
            if (Parent.Props.XSizing.Behavior != SizingBehavior.Fixed) {
                Parent.Bounds.Width += Bounds.Width;
            }

            // cross axis
            if (Parent.Props.YSizing.Behavior != SizingBehavior.Fixed) {
                Parent.Bounds.Height = Math.Max(Parent.Bounds.Height, Bounds.Height);
            }

        } else {
            // on axis
            if (Parent.Props.YSizing.Behavior != SizingBehavior.Fixed) {
                Parent.Bounds.Height += Bounds.Height;
            }

            // cross axis
            if (Parent.Props.XSizing.Behavior != SizingBehavior.Fixed) {
                Parent.Bounds.Width = Math.Max(Parent.Bounds.Width, Bounds.Width);
            }
        }
    }

    public void CalcPositions() {
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

        foreach (Element child in Children) {
            child.Draw(sb);
        }
    }
}
