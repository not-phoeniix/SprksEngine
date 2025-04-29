using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Embyr.Tools;
using System;

namespace Embyr.UI;

/// <summary>
/// A menu item that can be clicked and activates events
/// </summary>
public class Button : Label, IMenuInteractable {
    /// <summary>
    /// Event to be called when button is clicked instantly
    /// (left click / select button / enter key)
    /// </summary>
    public event Action OnClickInstant;

    /// <summary>
    /// Event to be called when the button is clicked
    /// </summary>
    public event Action OnClick;

    /// <summary>
    /// Event to be called when button is clicked w/
    /// right mouse button or secondary buttons
    /// </summary>
    public event Action OnSecondaryClick;

    /// <summary>
    /// Whether or not this button is hovered/selected
    /// </summary>
    public bool Hovered { get; set; }

    /// <summary>
    /// Whether or not this button's clicking/selecting is enabled
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets whether or not this button is pressed
    /// </summary>
    public bool Pressed { get; private set; }

    /// <summary>
    /// Gets/sets whether or not this button is toggled
    /// </summary>
    public bool Toggled { get; private set; }

    /// <summary>
    /// Creates a button instance with specified bounds
    /// </summary>
    /// <param name="text">Text to display on button</param>
    /// <param name="toggleable">Whether or not to make button toggle when pressed</param>
    /// <param name="marginlessBounds">Size/location bounds of button</param>
    /// <param name="style">Style rules for this button</param>
    public Button(
        string text,
        bool toggleable,
        Rectangle marginlessBounds,
        ElementStyle style
    ) : base(text, marginlessBounds, style) {
        if (toggleable) {
            OnClick += () => Toggled = !Toggled;
        }
    }

    /// <summary>
    /// Creates a button instance, size is detetrmined by string size
    /// </summary>
    /// <param name="text">Text to display on button</param>
    /// <param name="toggleable">Whether or not to make button toggle when pressed</param>
    /// <param name="position">Where to place button</param>
    /// <param name="style">Style rules for this Button</param>
    public Button(
        string text,
        bool toggleable,
        Vector2 position,
        ElementStyle style
    ) : base(text, position, style) {
        if (toggleable) {
            OnClick += () => Toggled = !Toggled;
        }
    }

    /// <summary>
    /// Creates a button instance, size is determined by inputted width
    /// </summary>
    /// <param name="text">Text to display on button</param>
    /// <param name="toggleable">Whether or not to make button toggle when pressed</param>
    /// <param name="width">Initial width of button</param>
    /// <param name="style">Style rules for this Button</param>
    public Button(string text, bool toggleable, int width, ElementStyle style)
    : base(text, width, style) {
        if (toggleable) {
            OnClick += () => Toggled = !Toggled;
        }
    }

    /// <summary>
    /// Creates a button instance, size is detetrmined by string size
    /// </summary>
    /// <param name="text">Text to display on button</param>
    /// <param name="toggleable">Whether or not to make button toggle when pressed</param>
    /// <param name="style">Style rules for this Button</param>
    public Button(string text, bool toggleable, ElementStyle style)
    : base(text, style) {
        if (toggleable) {
            OnClick += () => Toggled = !Toggled;
        }
    }

    /// <summary>
    /// Updates button input logic, checks and executes event on activation
    /// </summary>
    /// <param name="dt">Time passed since last frame</param>
    public override void Update(float dt) {
        base.Update(dt);

        bool alreadyClicked = false;

        // execute events if clicked
        if (Hovered) {
            // only activate with submit if an alt key isn't pressed
            //   (to not also toggle fullscreen)
            if ((!Input.AnyModifierDown() && Input.IsActionOnce(InputAction.Submit)) ||
                Input.IsLeftMouseDownOnce()
            ) {
                Activate();
                Pressed = true;

                // controllers do "click up" instantly
                if (Input.IsAction(InputAction.Submit)) {
                    OnClick?.Invoke();
                    alreadyClicked = true;
                }
            }

            // TODO: maybe integrate this with the IMenuInteractable at some point
            if (Input.IsRightMouseDownOnce() && Enabled) {
                OnSecondaryClick?.Invoke();
                Pressed = true;
            }
        }

        // disable "pressed" and invoke click up for the first frame the button is released
        if (Pressed && !Input.IsLeftMouseDown()) {
            Pressed = false;

            // only actually invoke click up if hovered,
            //   that way people can click, hold, and
            //   hover off to cancel the click
            if (Hovered && !alreadyClicked) {
                OnClick?.Invoke();
            }
        }

        // don't be pressed if user stops hovering
        if (Pressed && !Hovered) {
            Pressed = false;
        }
    }

    /// <summary>
    /// Updates hover logic for this button
    /// </summary>
    /// <param name="preventHover">Whether or not to prevent updating hovering for this button (will always reset to false if so)</param>
    public void UpdateMouseHover(bool preventHover = false) {
        Hovered = false;
        if (MarginlessBounds.Contains(Input.MousePos) && Enabled && !preventHover) {
            Hovered = true;
        }
    }

    /// <summary>
    /// Draws the button to the screen
    /// </summary>
    /// <param name="sb">SpriteBatch to draw with</param>
    public override void Draw(SpriteBatch sb) {
        Rectangle borderBounds = Utils.ExpandRect(MarginlessBounds, Style.BorderSize);
        sb.DrawRectFill(borderBounds, Style.BorderColor);

        if (Pressed || Toggled) {
            sb.DrawRectFill(MarginlessBounds, Style.ActiveColor);
        } else if (Hovered) {
            if (Hovered) sb.DrawRectFill(MarginlessBounds, Style.HoverColor);
        } else {
            sb.DrawRectFill(MarginlessBounds, Style.BackgroundColor);
        }

        // x position for below vector, dependent on alignment
        int xPos = TextAlign switch {
            XAlign.Left => MarginlessBounds.Left,
            XAlign.Center => MarginlessBounds.Center.X - StringSize.ToPoint().X / 2,
            XAlign.Right => MarginlessBounds.Right - StringSize.ToPoint().X,
        };

        // position for text to be rendered in center of bounds
        Vector2 stringPos = new(
            xPos,
            MarginlessBounds.Center.Y - StringSize.ToPoint().Y / 2
        );

        sb.DrawString(Style.Font, Text, stringPos, Style.ForegroundColor);
    }

    /// <summary>
    /// Activates button action
    /// </summary>
    public void Activate() {
        if (Enabled) {
            OnClickInstant?.Invoke();
        } else {
            Debug.WriteLine("You tried clicking a button that wasn't enabled... lol, take the L");
        }
    }
}
