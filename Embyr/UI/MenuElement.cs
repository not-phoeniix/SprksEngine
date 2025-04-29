using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Embyr.UI;

/// <summary>
/// A basic abstract element of a menu, cannot be instantiated
/// </summary>
public abstract class MenuElement : IDrawable, IDebugDrawable {
    /// <summary>
    /// Rectangular bounds of this menu item, in screen-space
    /// </summary>
    public Rectangle Bounds {
        get {
            return new Rectangle(
                MarginlessBounds.X - Style.Margin,
                MarginlessBounds.Y - Style.Margin,
                MarginlessBounds.Width + Style.Margin * 2,
                MarginlessBounds.Height + Style.Margin * 2
            );
        }
    }

    /// <summary>
    /// Rectangular bounds of this item WITHOUT margin, in screen-space
    /// </summary>
    public Rectangle MarginlessBounds { get; set; }

    // Style isn't a property, rather it's a public variable so
    //   the values can be accessed/modified directly without C#
    //   complaining "ohhh wah wah wah Style isn't a variable so
    //   you can't modify it" blah blah blah

    /// <summary>
    /// Style rules of this MenuElement
    /// </summary>
    public ElementStyle Style;

    /// <summary>
    /// Gets/sets position relative to center of menu item's bounds
    /// </summary>
    public Vector2 Position {
        get {
            int x = 0;
            int y = 0;

            switch (Style.XAlignment) {
                case XAlign.Left:
                    x = MarginlessBounds.Left;
                    break;
                case XAlign.Center:
                    x = MarginlessBounds.Center.X;
                    break;
                case XAlign.Right:
                    x = MarginlessBounds.Right;
                    break;
            }

            switch (Style.YAlignment) {
                case YAlign.Top:
                    y = MarginlessBounds.Top;
                    break;
                case YAlign.Center:
                    y = MarginlessBounds.Center.Y;
                    break;
                case YAlign.Bottom:
                    y = MarginlessBounds.Bottom;
                    break;
            }

            return new Vector2(x, y);
        }

        set {
            Rectangle newBounds = MarginlessBounds;

            switch (Style.XAlignment) {
                case XAlign.Left:
                    newBounds.X = (int)value.X;
                    break;
                case XAlign.Center:
                    newBounds.X = (int)value.X - newBounds.Width / 2;
                    break;
                case XAlign.Right:
                    newBounds.X = (int)value.X - newBounds.Width;
                    break;
            }

            switch (Style.YAlignment) {
                case YAlign.Top:
                    newBounds.Y = (int)value.Y;
                    break;
                case YAlign.Center:
                    newBounds.Y = (int)value.Y - newBounds.Height / 2;
                    break;
                case YAlign.Bottom:
                    newBounds.Y = (int)value.Y - newBounds.Height;
                    break;
            }

            MarginlessBounds = newBounds;
        }
    }

    /// <summary>
    /// Creates an instance of a MenuItem object with specified bounds
    /// </summary>
    /// <param name="marginlessBounds">Bounds to create with, doesn't include margins</param>
    /// <param name="style">Style rules for this MenuElement</param>
    public MenuElement(Rectangle marginlessBounds, ElementStyle style) {
        MarginlessBounds = marginlessBounds;
        Style = style;
    }

    /// <summary>
    /// Creates an instance of a MenuItem object with specified position/size
    /// </summary>
    /// <param name="position">Top left aligned position of bounds</param>
    /// <param name="size">Size of bounding rectangle</param>
    /// <param name="style">Style rules for this MenuElement</param>
    public MenuElement(Vector2 position, Vector2 size, ElementStyle style) {
        MarginlessBounds = new Rectangle(position.ToPoint(), size.ToPoint());
        Style = style;
        Position = position;
    }

    /// <summary>
    /// Updates state of MenuItem, used for input handling
    /// </summary>
    /// <param name="dt"></param>
    public abstract void Update(float dt);

    /// <summary>
    /// Draws this menu item to the screen
    /// </summary>
    /// <param name="sb">SpriteBatch to draw with</param>
    public abstract void Draw(SpriteBatch sb);

    /// <summary>
    /// Draws overlay elements of this menu element, optional definition in child classes
    /// </summary>
    /// <param name="sb">SpriteBatch to draw with</param>
    public virtual void DrawOverlays(SpriteBatch sb) { }

    /// <summary>
    /// Draws bounds of this menu element
    /// </summary>
    /// <param name="sb">SpriteBatch to draw with</param>
    public virtual void DebugDraw(SpriteBatch sb) {
        sb.DrawRectOutline(Bounds, 1, Color.Red);
    }
}
