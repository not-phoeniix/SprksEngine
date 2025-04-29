using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Embyr.UI;

/// <summary>
/// A menu element that drops down and can select one value at a time,
/// containing several strings of possible values
/// </summary>
public class Dropdown : MenuElement, IMenuInteractable {
    private readonly Aligner valuesAligner;
    private readonly Button openButton;
    private int selectedValueIndex;

    /// <summary>
    /// Gets/sets whether or not this dropdown is hovered
    /// </summary>
    public bool Hovered { get; set; }

    /// <summary>
    /// Gets/sets whether or not this dropdown is enabled
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets whether or not this dropdown is open
    /// </summary>
    public bool Open { get; private set; }

    /// <summary>
    /// Gets/Sets string of currently selected value within this dropdown
    /// </summary>
    public string SelectedValue {
        get => openButton.Text;
        set => openButton.Text = value;
    }

    /// <summary>
    /// Number of values within this dropdown
    /// </summary>
    public int OptionsCount => valuesAligner.Count;

    /// <summary>
    /// Event to execute when a selection changes in this dropdown
    /// </summary>
    public Action OnSelectionChange;

    /// <summary>
    /// Creates a new Dropdown element
    /// </summary>
    /// <param name="values">
    /// Array of string values to put into this dropdown.
    /// String at index zero will be the default at creation
    /// </param>
    /// <param name="width">Width in pixels of this dropdown</param>
    /// <param name="position">Position of this dropdown</param>
    /// <param name="mainStyle">
    /// ElementStyle used to style the entire Dropdown element,
    /// including main margin/border, as well as top-most label that
    /// opens/closes the dropdown
    /// </param>
    /// <param name="valueStyle">
    /// Style of all the labels of values/options within the dropdown
    /// </param>
    public Dropdown(string[] values, int width, Vector2 position, ElementStyle mainStyle, ElementStyle valueStyle)
    : base(position, new Vector2(width, 0), mainStyle) {
        // baseline rules to make sure value buttons are
        //   aligned and not separated by margins
        valueStyle.YAlignment = YAlign.Top;
        valueStyle.XAlignment = XAlign.Left;
        valueStyle.Margin = 0;

        // button/label that shows current selected value, and opens/closes the menu
        openButton = new Button(values[0], false, width, mainStyle);

        openButton.OnClickInstant += Activate;

        // create labels for each value and toss it in an aligner
        valuesAligner = new Aligner(AlignDirection.Vertical, valueStyle);
        for (int i = 0; i < values.Length; i++) {
            // create button and make clicking it assign the value of SelectedValue
            Button button = new(
                values[i],
                false,
                new Rectangle(0, 0, width, 0),      // button will auto resize, just set width
                valueStyle
            );

            button.OnClickInstant += () => {
                SelectedValue = button.Text;
                Open = false;
                OnSelectionChange?.Invoke();
            };

            valuesAligner.Add(button);
        }
    }

    /// <summary>
    /// Updates Dropdown logic
    /// </summary>
    /// <param name="dt">Time passed since last frame</param>
    public override void Update(float dt) {
        if (Open) {
            // set position to align at the very top of this element's marginless bounds
            YAlign oldAlignment = openButton.Style.YAlignment;
            openButton.Style.YAlignment = YAlign.Top;
            openButton.Position = new Vector2(
                Position.X,
                MarginlessBounds.Top
            );
            openButton.Style.YAlignment = oldAlignment;

            // valuesAligner will always be aligned at the top-left,
            //  connected right below the openButton
            valuesAligner.Position = new Vector2(
                openButton.MarginlessBounds.Left,
                openButton.MarginlessBounds.Bottom
            );

            valuesAligner.Update(dt);

            // when open, all interactables within are enabled and
            //   the hovered value is updated based on the selected
            //   value index
            int i = 0;
            foreach (Button button in valuesAligner) {
                button.Enabled = true;
                button.Hovered = i == selectedValueIndex;
                i++;
            }

            if (Input.IsActionOnce(InputAction.Back)) {
                Open = false;
            }

        } else {
            openButton.Position = Position;
            MarginlessBounds = openButton.MarginlessBounds;

            // when closed, all interactables within are disabled
            foreach (IMenuInteractable interactable in valuesAligner) {
                interactable.Enabled = false;
            }
        }

        // update open toggle button
        openButton.Update(dt);

        // close if clicked elsewhere when not hovered
        if (!Hovered && (Input.IsActionOnce(InputAction.Submit) || Input.IsLeftMouseDownOnce())) {
            Open = false;
        }
    }

    /// <summary>
    /// Updates mouse hover logic for this dropdown
    /// </summary>
    /// <param name="preventHover">Whether or not to prevent hovering</param>
    public void UpdateMouseHover(bool preventHover) {
        Hovered = false;
        openButton.UpdateMouseHover(preventHover);
        if (openButton.Hovered) Hovered = true;

        int i = 0;
        foreach (IMenuInteractable value in valuesAligner) {
            // prevent hovers updating if closed or
            //   preventing hover initially
            value.UpdateMouseHover(!Open || preventHover);

            // if value is hovered in any other way (via mouse),
            //   change index to this one
            if (value.Hovered) {
                selectedValueIndex = i;
                Hovered = true;
            }

            i++;
        }
    }

    /// <summary>
    /// Scrolls down one in the list of value options in dropdown
    /// </summary>
    private void ScrollDown() {
        selectedValueIndex++;
        if (selectedValueIndex >= OptionsCount) {
            selectedValueIndex = 0;
        }
    }

    /// <summary>
    /// Scrolls up one in the list of value options in dropdown
    /// </summary>
    private void ScrollUp() {
        selectedValueIndex--;
        if (selectedValueIndex < 0) {
            selectedValueIndex = OptionsCount - 1;
        }
    }

    /// <summary>
    /// Handles controller input for this dropdown
    /// </summary>
    /// <param name="mouseMode"></param>
    /// <returns></returns>
    public bool HandleControllerInput(ref bool mouseMode) {
        if (Open) {
            if (Input.IsActionOnce(InputAction.Back)) {
                Open = false;
                return false;
            }

            if (Input.IsActionOnce(InputAction.UIDown)) {
                mouseMode = false;
                ScrollDown();
            }

            if (Input.IsActionOnce(InputAction.UIUp)) {
                mouseMode = false;
                ScrollUp();
            }

            return true;
        }

        return false;
    }

    /// <summary>
    /// Activates (opens/closes) this dropdown
    /// </summary>
    public void Activate() {
        // reverse open value and set index to zero again
        Open = !Open;
        selectedValueIndex = 0;
    }

    /// <summary>
    /// Draws this Dropdown to the screen
    /// </summary>
    /// <param name="sb">SpriteBatch to draw with</param>
    public override void Draw(SpriteBatch sb) {
        openButton.Draw(sb);
    }

    /// <summary>
    /// Draws the actual "dropdown" part (values) of this dropdown as an overlay
    /// </summary>
    /// <param name="sb">SpriteBatch to draw with</param>
    public override void DrawOverlays(SpriteBatch sb) {
        if (Open) {
            valuesAligner.Draw(sb);
        }
    }
}
