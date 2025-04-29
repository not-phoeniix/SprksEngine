using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Embyr.Tools;
using System;

namespace Embyr.UI;

/// <summary>
/// A menu item that can be typed in
/// </summary>
public class Typebox : Label, IMenuInteractable {
    private Vector2 cursorSize;
    private readonly string initialText;
    private bool unclicked = true;

    /// <summary>
    /// Whether or not this typebox is empty in text
    /// </summary>
    public bool Empty {
        get { return Text == "" || Text == initialText; }
    }

    /// <summary>
    /// Whether or not this typebox is hovered/selected
    /// </summary>
    public bool Hovered { get; set; }

    /// <summary>
    /// Whether or not this typebox is enabled
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Maximum allowed number of characters in this typebox
    /// </summary>
    public int CharacterLimit { get; set; }

    /// <summary>
    /// Whether or not this typebox grabs keyboard input,,, is "Focused"
    /// </summary>
    public bool Focused { get; set; }

    /// <summary>
    /// Event called when typebox is submitted
    /// </summary>
    public event Action OnSubmit;

    /// <summary>
    /// Creates instance of a Typebox object
    /// </summary>
    /// <param name="initialText">Initial text before editing</param>
    /// <param name="centerPosition">Where to place Typebox's center</param>
    /// <param name="style">Style rules for this Typebox</param>
    public Typebox(
        string initialText,
        Vector2 centerPosition,
        ElementStyle style
    ) : base(initialText, centerPosition, style) {
        cursorSize = Style.Font.MeasureString("|");
        this.initialText = initialText;
        CharacterLimit = -1;
    }

    /// <summary>
    /// Creates instance of a Typebox object
    /// </summary>
    /// <param name="initialText">Initial text before editing</param>
    /// <param name="style">Style rules for this Typebox</param>
    public Typebox(string initialText, ElementStyle style)
    : this(initialText, Vector2.Zero, style) { }

    /// <summary>
    /// Creates instance of a Typebox object with specified width
    /// </summary>
    /// <param name="initialText">Initial text before editing</param>
    /// <param name="width">Width of Typebox to specify</param>
    /// <param name="style">Style rules for this Typebox</param>
    public Typebox(string initialText, int width, ElementStyle style)
    : base(initialText, width, style) {
        cursorSize = Style.Font.MeasureString("|");
        this.initialText = initialText;
        CharacterLimit = -1;
    }

    /// <summary>
    /// Updates Typebox input, including keyboard input and mouse selection
    /// </summary>
    /// <param name="dt">Time passed since last frame</param>
    public override void Update(float dt) {
        bool submitNoMod = !Input.AnyModifierDown() && Input.IsActionOnce(InputAction.Submit);
        bool leftMouse = Input.IsLeftMouseDownOnce();

        // type when active
        if (Focused) {
            // only runs once, sets state of "unclicked" to be false
            //   the very first time this box is clicked and sets text
            //   to nothing to indicate to player that you can type now
            if (unclicked) {
                Text = "";
                unclicked = false;
            }

            Input.PreventDirectionalsNextFrame = true;

            string modified = Text;
            Input.UpdateKeyboardString(ref modified);

            // chop off extra characters w/ substring
            if (CharacterLimit > -1 && modified.Length > CharacterLimit) {
                modified = modified.Substring(0, CharacterLimit);
            }

            Text = modified;

            // submits textbox
            if (submitNoMod) {
                Focused = false;
                Activate();
            }

        } else {
            if (Text == "") Reset();

            // focus if submit is pressed when unfocused
            if ((submitNoMod || leftMouse) && Hovered) {
                Focused = true;
            }
        }

        // unfocuses the typebox and prevents typing when back button is clicked
        if (Input.IsActionOnce(InputAction.Back)) {
            Focused = false;
        }

        // unfocuses the typebox and prevents typing when clicked and not hovered
        if ((submitNoMod || leftMouse) && !Hovered) {
            Focused = false;
        }

        base.Update(dt);
    }

    /// <summary>
    /// Draws the typebox to the screen
    /// </summary>
    /// <param name="sb">SpriteBatch to draw with</param>
    public override void Draw(SpriteBatch sb) {
        Rectangle borderRect = Utils.ExpandRect(MarginlessBounds, Style.BorderSize);
        sb.DrawRectFill(borderRect, Style.BorderColor);
        sb.DrawRectFill(MarginlessBounds, Style.BackgroundColor);
        if (Focused) sb.DrawRectFill(MarginlessBounds, Style.ActiveColor);
        if (Hovered) sb.DrawRectFill(MarginlessBounds, Style.HoverColor);

        // x position of string, dependent on text alignment
        int xPos = TextAlign switch {
            XAlign.Left => MarginlessBounds.Left + Style.Padding,
            XAlign.Center => MarginlessBounds.Center.X - StringSize.ToPoint().X / 2,
            XAlign.Right => MarginlessBounds.Right - StringSize.ToPoint().X - Style.Padding,
        };

        // position for text to be rendered
        Vector2 stringPos = new(
            xPos,
            Bounds.Y + Bounds.Height / 2 - StringSize.ToPoint().Y / 2
        );

        // draw text
        bool textChanged = (Text != initialText);
        Color textColor = textChanged ? Style.ForegroundColor : Style.InactiveColor;
        sb.DrawString(Style.Font, Text, stringPos, textColor);

        // draw cursor
        if (Focused) {
            Rectangle cursorRect = new(
                Bounds.Right - Style.Padding / 2,
                Bounds.Top + Style.Padding / 2,
                2,
                Bounds.Height - Style.Padding
            );

            sb.DrawRectFill(cursorRect, Style.ForegroundColor);
        }
    }

    /// <summary>
    /// Updates hover logic for this typebox
    /// </summary>
    /// <param name="preventHover">Whether or not to prevent updating hovering for this typebox (will always reset to false if so)</param>
    public void UpdateMouseHover(bool preventHover) {
        Hovered = false;
        if (MarginlessBounds.Contains(Input.MousePos) && Enabled && !preventHover) {
            Hovered = true;
        }
    }

    /// <summary>
    /// Activates the submission of this typebox
    /// </summary>
    public void Activate() {
        if (Enabled) {
            OnSubmit?.Invoke();
        } else {
            Debug.WriteLine("You tried submitting a typebox that wasn't enabled... silly goose!");
        }
    }

    /// <summary>
    /// Resets this typebox to its default state from creation
    /// </summary>
    public void Reset() {
        Text = initialText;
        unclicked = true;
    }
}
