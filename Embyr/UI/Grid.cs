using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Embyr.UI;

/// <summary>
/// A 2D grid of menu elements that align together
/// </summary>
public class Grid : MenuElement, IMenuInteractable {
    private readonly MenuElement[,] elements;
    private Vector2 topLeftDrawPrev;

    // 2D index point for elements array that tracks
    //   the currently selected item in the grid
    private Point index;

    /// <summary>
    /// Gets the width of this grid
    /// </summary>
    public int Width { get; private set; }

    /// <summary>
    /// Gets the height of this grid
    /// </summary>
    public int Height { get; private set; }

    /// <summary>
    /// Gets/sets the spacing between grid items
    /// </summary>
    public int Spacing { get; set; }

    /// <summary>
    /// Gets the currently selected element in the grid, can be null
    /// </summary>
    public IMenuInteractable? SelectedInteractable { get; private set; }

    /// <summary>
    /// Gets/sets whether or not this grid is hovered/focused by the mouse
    /// </summary>
    public bool Hovered { get; set; }

    /// <summary>
    /// Whether or not this grid is enabled
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Get/set indexer that updates grid elements within the grid
    /// </summary>
    /// <param name="x">X index of grid to get/set</param>
    /// <param name="y">Y index of grid to get/set</param>
    /// <returns>A menu element at given x/y position</returns>
    public MenuElement this[int x, int y] {
        get {
            if (x < 0 || y < 0 || x >= elements.GetLength(0) || y >= elements.GetLength(1)) {
                throw new ArgumentOutOfRangeException(
                    $"ERROR: Index [{x}, {y}] out of range of grid!"
                );
            }

            return elements[x, y];
        }

        set {
            if (x < 0 || y < 0 || x >= elements.GetLength(0) || y >= elements.GetLength(1)) {
                throw new ArgumentOutOfRangeException(
                    $"ERROR: Index [{x}, {y}] out of range of grid!"
                );
            }

            // make all elements top-left aligned in grids with zero margin
            value.Style.XAlignment = XAlign.Left;
            value.Style.YAlignment = YAlign.Top;
            value.Style.Margin = 0;
            elements[x, y] = value;
        }
    }

    /// <summary>
    /// Creates a new grid element with null internal elements
    /// </summary>
    /// <param name="width">Width of grid to create</param>
    /// <param name="height">Height of grid to create</param>
    /// <param name="style">ElementStyle to dictate look of grid</param>
    public Grid(int width, int height, ElementStyle style)
    : base(Rectangle.Empty, style) {
        Width = width;
        Height = height;
        elements = new MenuElement[width, height];
        index = Point.Zero;
        Position = Vector2.Zero;
    }

    /// <summary>
    /// Updates all internal menu elements
    /// </summary>
    /// <param name="dt"></param>
    public override void Update(float dt) {
        AlignElements();

        for (int y = 0; y < Height; y++) {
            for (int x = 0; x < Width; x++) {
                elements[x, y]?.Update(dt);
            }
        }
    }

    private void AlignElements() {
        int totalWidth = 0;
        for (int i = 0; i < Width; i++) {
            totalWidth += elements[i, 0].Bounds.Width;
        }

        int totalHeight = 0;
        for (int i = 0; i < Height; i++) {
            totalHeight += elements[0, i].Bounds.Height;
        }

        // update bounds with very complicated and technical
        //   copy-alter-replace technique (thanks Erin)
        Rectangle newMarginless = new(
            MarginlessBounds.Location.X,
            MarginlessBounds.Location.Y,
            totalWidth + Spacing * (Width - 1),
            totalHeight + Spacing * (Height - 1)
        );

        // offset to make bounds centered if this is the first
        //   time bounds is being updated (rectangle is still
        //   empty from ctor)
        if (MarginlessBounds.Width == 0 && MarginlessBounds.Height == 0) {
            newMarginless.X -= newMarginless.Width / 2;
            newMarginless.Y -= newMarginless.Height / 2;
        }

        MarginlessBounds = newMarginless;

        Vector2 topLeft = new(
            Style.XAlignment switch {
                XAlign.Left => Position.X,
                XAlign.Center => Position.X - totalWidth / 2,
                XAlign.Right => Position.X - totalWidth,
            },
            Style.YAlignment switch {
                YAlign.Top => Position.Y,
                YAlign.Center => Position.Y - totalHeight / 2,
                YAlign.Bottom => Position.Y - totalHeight
            }
        );

        topLeftDrawPrev = topLeft;

        float xOffset = (totalWidth / Width) + Spacing;
        float yOffset = (totalHeight / Height) + Spacing;

        // place all elements starting top left and iterating
        for (int y = 0; y < Height; y++) {
            for (int x = 0; x < Width; x++) {
                MenuElement element = elements[x, y];
                if (element != null) {
                    element.Position = topLeft + new Vector2(x * xOffset, y * yOffset);
                }
            }
        }
    }

    /// <summary>
    /// Draws this grid collection of elements to the screen
    /// </summary>
    /// <param name="sb">SpriteBatch to draw with</param>
    public override void Draw(SpriteBatch sb) {
        for (int y = 0; y < Height; y++) {
            for (int x = 0; x < Width; x++) {
                elements[x, y]?.Draw(sb);
            }
        }
    }

    /// <summary>
    /// Activates the currently selected element in the grid
    /// </summary>
    public void Activate() {
        if (Enabled) {
            SelectedInteractable?.Activate();
        }
    }

    /// <summary>
    /// Updates internal mouse hovering logic for grid elements
    /// </summary>
    /// <param name="preventHover">Whether or not to prevent hovering mouse logic for this grid</param>
    public void UpdateMouseHover(bool preventHover = false) {
        if (!Enabled) {
            Hovered = false;
            return;
        }

        Hovered = MarginlessBounds.Contains(Input.MousePos);

        SelectedInteractable = null;
        for (int y = 0; y < Height; y++) {
            for (int x = 0; x < Width; x++) {
                if (elements[x, y] is IMenuInteractable interactable) {
                    interactable.UpdateMouseHover(preventHover);
                    if (interactable.Hovered) {
                        SelectedInteractable = interactable;
                    }
                }
            }
        }

        SelectedInteractable?.UpdateMouseHover(preventHover);
    }

    // updates the selected interactable with the current index
    //   returns: true if selected exists, false if null
    private bool UpdateSelectedRef() {
        if (SelectedInteractable != null) {
            SelectedInteractable.Hovered = false;
        }

        SelectedInteractable = elements[index.X, index.Y] as IMenuInteractable;
        if (SelectedInteractable != null) {
            SelectedInteractable.Hovered = true;
            return true;
        }

        return false;
    }

    private void ScrollLeft() {
        index.X--;
        if (index.X < 0) {
            index.X = Width - 1;
        }

        // keep scrolling left until it finds an interactable
        if (!UpdateSelectedRef()) {
            ScrollLeft();
        }
    }

    private void ScrollRight() {
        index.X++;
        if (index.X >= Width) {
            index.X = 0;
        }

        // keep scrolling right until it finds an interactable
        if (!UpdateSelectedRef()) {
            ScrollRight();
        }
    }

    private void ScrollDown() {
        index.Y++;
        if (index.Y >= Height) {
            index.Y = 0;
        }

        // keep scrolling down until it finds an interactable
        if (!UpdateSelectedRef()) {
            ScrollDown();
        }
    }

    private void ScrollUp() {
        index.Y--;
        if (index.Y < 0) {
            index.Y = Height - 1;
        }

        // keep scrolling up until it finds an interactable
        if (!UpdateSelectedRef()) {
            ScrollUp();
        }
    }

    /// <summary>
    /// Checks for and handles grid input using directional input
    /// </summary>
    /// <param name="mouseMode">Reference to mouse mode boolean that is changed when input is detected</param>
    /// <returns>True when controller input is still captured by grid, false if not</returns>
    public bool HandleControllerInput(ref bool mouseMode) {
        // only handle this grid's input if the selected one isn't handling it first
        if (!SelectedInteractable?.HandleControllerInput(ref mouseMode) ?? true && Hovered) {
            if (Input.IsActionOnce(ActionBindingPreset.UIDownAction)) {
                // exit if trying to scroll down from the bottom
                if (index.Y == Height - 1) {
                    if (SelectedInteractable != null) {
                        SelectedInteractable.Hovered = false;
                        SelectedInteractable = null;
                    }
                    return false;
                }

                ScrollDown();
            }

            if (Input.IsActionOnce(ActionBindingPreset.UIUpAction)) {
                // exit if trying to scroll up from the top
                if (index.Y == 0) {
                    if (SelectedInteractable != null) {
                        SelectedInteractable.Hovered = false;
                        SelectedInteractable = null;
                    }
                    return false;
                }

                ScrollUp();
            }

            if (Input.IsActionOnce(ActionBindingPreset.UILeftAction)) {
                // exit if trying to scroll left at the left edge
                if (index.X == 0) {
                    if (SelectedInteractable != null) {
                        SelectedInteractable.Hovered = false;
                        SelectedInteractable = null;
                    }
                    return false;
                }

                ScrollLeft();
            }

            if (Input.IsActionOnce(ActionBindingPreset.UIRightAction)) {
                // exit if trying to scroll right at the right edge
                if (index.X == Width - 1) {
                    if (SelectedInteractable != null) {
                        SelectedInteractable.Hovered = false;
                        SelectedInteractable = null;
                    }
                    return false;
                }

                ScrollRight();
            }

            UpdateSelectedRef();

            return true;
        }

        return false;
    }
}
