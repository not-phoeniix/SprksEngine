using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Embyr.UI;

/// <summary>
/// Dragger menu element, can be dragged with mouse and
/// </summary>
public class Dragger : MenuElement, IEnumerable<MenuElement> {
    private readonly List<MenuElement> elements;
    private bool shouldDrag;
    private Vector2 currentDragOffset;
    private Vector2 prevDraggerPosition;
    private Button dragHandle;
    private Button pinnedButton;
    private Point prevBoundsSize;

    /// <summary>
    /// Gets/sets the drag handle button of this dragger
    /// </summary>
    protected Button DragHandle {
        get => dragHandle;
        set {
            // remove old handle if it exists
            if (dragHandle != null) {
                dragHandle.OnClickInstant -= OnDragHandleClicked;
                elements.Remove(dragHandle);
            }

            // set up new handle (if it exists)
            if (value != null) {
                value.OnClickInstant += OnDragHandleClicked;
                elements.Add(value);
                dragHandle = value;
            }
        }
    }

    /// <summary>
    /// Gets whether or not this dragger is currently being dragged
    /// </summary>
    public bool IsDragging { get; private set; }

    /// <summary>
    /// Event executed when this dragger begins dragging
    /// </summary>
    public event Action OnDragEnter;

    /// <summary>
    /// Gets/sets pinned button of this dragger
    /// </summary>
    protected Button PinnedButton {
        get => pinnedButton;
        set {
            // remove old button if it exists
            if (pinnedButton != null) {
                elements.Remove(pinnedButton);
            }

            pinnedButton = null;

            // set up new button (if it exists)
            if (value != null) {
                elements.Add(value);
                pinnedButton = value;
            }
        }
    }

    /// <summary>
    /// Gets/sets the local position offset of the drag handle
    /// relative to this dragger element
    /// </summary>
    public Vector2 DragHandleLocalPos { get; set; }

    /// <summary>
    /// Gets/sets the local position offset of the pinned button
    /// relative to this dragger element
    /// </summary>
    public Vector2 PinnedButtonLocalPos { get; set; }

    /// <summary>
    /// Gets/sets the speed to move dragger when controller dpad is used
    /// </summary>
    public float ControllerDragSpeed { get; set; }

    /// <summary>
    /// Gets/sets the rectangle box to keep the dragger within
    /// </summary>
    public Rectangle ClampBounds { get; set; }

    /// <summary>
    /// Gets/sets whether or not this dragger is enabled
    /// </summary>
    public bool Enabled { get; set; } = true;

    #region // IEnumerable implementation

    /// <summary>
    /// Gets the contents of this aligner as an enumerator of menu elements
    /// </summary>
    /// <returns>Enumerator of elements within this aligner</returns>
    public IEnumerator<MenuElement> GetEnumerator() {
        return elements.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }

    #endregion

    /// <summary>
    /// Creates a new Dragger
    /// </summary>
    /// <param name="dragHandle">Reference to a button used as the drag handle, can be null</param>
    /// <param name="pinnedButton">Reference to a button used to pin the dragger, can be null</param>
    /// <param name="centerPos">Center-aligned position to place dragger</param>
    /// <param name="size">Width/height size of dragger bounds</param>
    public Dragger(
        Button dragHandle,
        Button pinnedButton,
        Point centerPos,
        Point size
    ) : base(new Rectangle(centerPos - new Point(size.X / 2, size.Y / 2), size), new ElementStyle()) {
        elements = new List<MenuElement>();

        // use properties so setup logic is ran
        DragHandle = dragHandle;
        PinnedButton = pinnedButton;

        prevDraggerPosition = Position;
        ControllerDragSpeed = 200;
    }

    /// <summary>
    /// Updates this dragger
    /// </summary>
    /// <param name="dt">Time passed since last frame</param>
    public override void Update(float dt) {
        if (!Enabled) return;

        // if size has changed, prevent any dragging offsets from occuring
        if (prevBoundsSize != MarginlessBounds.Size) {
            prevDraggerPosition = Position;
        }

        UpdateDrag(dt);

        foreach (MenuElement element in elements) {
            Vector2 offset = Position - prevDraggerPosition;
            element.Position += offset;
            element.Update(dt);
        }

        // update previous position only after all other logic is complete
        //   this is so if the initial position is different from (0,0)
        //   then the position of the elements will automatically be updated
        prevDraggerPosition = Position;
        prevBoundsSize = MarginlessBounds.Size;
    }

    private void UpdateDrag(float dt) {
        //* shouldDrag and isPinned are both set by the
        //*   buttons in the constructor too!

        // don't drag at all if dragger is pinned
        if (pinnedButton != null && pinnedButton.Toggled) {
            return;
        }

        // if drag state changes, update drag offset vector and set
        //   the menu to "should drag"
        if (shouldDrag && !IsDragging) {
            currentDragOffset = Position - Input.MousePos;
            IsDragging = true;
            OnDragEnter?.Invoke();
        }

        // make it so only stop dragging after mouse is released
        if (!Input.IsLeftMouseDown() && !Input.IsAction(InputAction.Submit)) {
            IsDragging = false;
            shouldDrag = false;
        }

        if (IsDragging) {
            if (Input.IsAction(InputAction.UILeft)) {
                currentDragOffset.X -= ControllerDragSpeed * dt;
            }

            if (Input.IsAction(InputAction.UIRight)) {
                currentDragOffset.X += ControllerDragSpeed * dt;
            }

            if (Input.IsAction(InputAction.UIUp)) {
                currentDragOffset.Y -= ControllerDragSpeed * dt;
            }

            if (Input.IsAction(InputAction.UIDown)) {
                currentDragOffset.Y += ControllerDragSpeed * dt;
            }

            Position = Input.MousePos + currentDragOffset;
        }

        // clamp center pos to never be able to leave the clamp
        //   bounds,,, only if clamp bounds is actually defined
        if (ClampBounds != Rectangle.Empty) {
            Vector2 min = new(
                ClampBounds.Left + Bounds.Width / 2,
                ClampBounds.Top + Bounds.Height / 2
            );
            Vector2 max = new(
                ClampBounds.Right - Bounds.Width / 2,
                ClampBounds.Bottom - Bounds.Height / 2
            );

            Position = Vector2.Clamp(Position, min, max);
        }
    }

    /// <summary>
    /// Draws this dragger
    /// </summary>
    /// <param name="sb">SpriteBatch to draw with</param>
    public override void Draw(SpriteBatch sb) {
        //* don't draw drag handle or pinned button, save those
        //*   for last so they always draw on top regardless
        //*   of internal order

        sb.DrawRectOutline(Bounds, Style.BorderSize, Style.BorderColor);

        foreach (MenuElement element in elements) {
            if (element != DragHandle && element != PinnedButton) {
                element.Draw(sb);
            }
        }

        PinnedButton?.Draw(sb);
        DragHandle?.Draw(sb);

        foreach (MenuElement element in elements) {
            if (element != DragHandle && element != PinnedButton) {
                element.DrawOverlays(sb);
            }
        }

        PinnedButton?.DrawOverlays(sb);
        DragHandle?.DrawOverlays(sb);
    }

    /// <summary>
    /// Draws debug information for this dragger
    /// </summary>
    /// <param name="sb">SpriteBatch to draw with</param>
    public override void DebugDraw(SpriteBatch sb) {
        sb.DrawRectOutline(MarginlessBounds, 1, Color.Yellow);
        foreach (MenuElement element in elements) {
            element.DebugDraw(sb);
        }
    }

    /// <summary>
    /// Adds a menu element to this dragger
    /// </summary>
    /// <param name="element">Element to add</param>
    public void Add(MenuElement element) {
        elements.Add(element);
    }

    private void OnDragHandleClicked() {
        shouldDrag = true;
    }
}
