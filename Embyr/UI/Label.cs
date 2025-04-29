using System;
using Embyr.Tools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Embyr.UI;

/// <summary>
/// A menu item that displays centered text
/// </summary>
public class Label : MenuElement {
    private string textPrev;        // used for detecting changes in string
    private int paddingPrev;

    /// <summary>
    /// Size of text in label, updated dynamically
    /// </summary>
    protected Vector2 StringSize { get; private set; }

    /// <summary>
    /// Text of label itself
    /// </summary>
    public string Text { get; set; }

    /// <summary>
    /// Gets/sets whether or not dynamic resizing occurs for width/height for text size
    /// </summary>
    public bool ConformToText { get; set; }

    /// <summary>
    /// Horizontal alignment of text inside Label
    /// </summary>
    public XAlign TextAlign { get; set; }

    /// <summary>
    /// Creates an instance of a Label object
    /// </summary>
    /// <param name="text">Text to display on label</param>
    /// <param name="marginlessBounds">
    /// Bounds of this element, without account for style margin
    /// </param>
    /// <param name="style">Style rules for this Label</param>
    public Label(
        string text,
        Rectangle marginlessBounds,
        ElementStyle style
    ) : base(marginlessBounds, style) {
        this.Text = text;
        TextAlign = XAlign.Center;
        StringSize = Style.Font.MeasureString(text);
        ConformToText = false;
        ResizeBounds();
    }

    /// <summary>
    /// Creates an instance of a Label object
    /// </summary>
    /// <param name="text">Text to display on label</param>
    /// <param name="position">Where to place label</param>
    /// <param name="width">Initial width of label</param>
    /// <param name="style">Style rules for this Label</param>
    public Label(string text, Vector2 position, int width, ElementStyle style)
    : this(text, new Rectangle(position.ToPoint(), new Point(width, 0)), style) {
        // setting position manually so alignment adjusts the internal bounds
        Position = position;
    }

    /// <summary>
    /// Creates an instance of a Label object
    /// </summary>
    /// <param name="text">Text to display on label</param>
    /// <param name="position">Where to place label</param>
    /// <param name="style">Style rules for this Label</param>
    public Label(
        string text,
        Vector2 position,
        ElementStyle style
    ) : this(text, new Rectangle(position.ToPoint(), Point.Zero), style) {
        // setting position manually so alignment adjusts the internal bounds
        Position = position;
    }

    /// <summary>
    /// Creates an instance of a Label object
    /// </summary>
    /// <param name="text">Text to display on label</param>
    /// <param name="width">Initial width of label</param>
    /// <param name="style">Style rules for this Label</param>
    public Label(string text, int width, ElementStyle style)
    : this(text, new Rectangle(0, 0, width, 0), style) { }

    /// <summary>
    /// Creates an instance of a Label object
    /// </summary>
    /// <param name="text">Text to display on label</param>
    /// <param name="style">Style rules for this Label</param>
    public Label(string text, ElementStyle style)
    : this(text, Vector2.Zero, style) { }

    /// <summary>
    /// Updates label, changing position information and resizing bounds to match string
    /// </summary>
    /// <param name="dt">Time passed since last frame</param>
    public override void Update(float dt) {
        // update string size cache if text or padding changes
        if (Text != textPrev || Style.Padding != paddingPrev) {
            StringSize = Style.Font.MeasureString(Text);
            ResizeBounds();
        }

        textPrev = Text;
        paddingPrev = Style.Padding;
    }

    /// <summary>
    /// Draws the Label to the screen
    /// </summary>
    /// <param name="sb">SpriteBatch to draw with</param>
    public override void Draw(SpriteBatch sb) {
        Rectangle borderBounds = Utils.ExpandRect(MarginlessBounds, Style.BorderSize);
        sb.DrawRectFill(borderBounds, Style.BorderColor);
        sb.DrawRectFill(MarginlessBounds, Style.BackgroundColor);

        // x position of string, dependent on text alignment
        int xPos = TextAlign switch {
            XAlign.Left => MarginlessBounds.Left + Style.Padding,
            XAlign.Center => MarginlessBounds.Center.X - StringSize.ToPoint().X / 2,
            XAlign.Right => MarginlessBounds.Right - StringSize.ToPoint().X - Style.Padding,
        };

        // position for text to be rendered in center of bounds
        Vector2 stringPos = new(
            xPos,
            MarginlessBounds.Center.Y - StringSize.ToPoint().Y / 2
        );

        // draw text itself
        sb.DrawString(
            Style.Font,
            Text,
            stringPos,
            Style.ForegroundColor
        );
    }

    private void ResizeBounds() {
        int newWidth = (int)StringSize.X + (Style.Padding * 2);
        int newHeight = (int)StringSize.Y + (Style.Padding * 2);

        if (ConformToText) {
            MarginlessBounds = new Rectangle(
                MarginlessBounds.X,
                MarginlessBounds.Y,
                newWidth,
                newHeight
            );
        } else {
            // only resize dynamically if the new dimensions are bigger
            MarginlessBounds = new Rectangle(
                MarginlessBounds.X,
                MarginlessBounds.Y,
                Math.Max(newWidth, MarginlessBounds.Width),
                Math.Max(newHeight, MarginlessBounds.Height)
            );
        }
    }
}
