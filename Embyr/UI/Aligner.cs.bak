using System.Collections;
using System.Collections.Generic;
using Embyr.Tools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Embyr.UI;

/// <summary>
/// Direction to align in an Aligner.
/// </summary>
public enum AlignDirection {
    Horizontal,
    Vertical
}

/// <summary>
/// A collection of MenuElements that automatically stack/align
/// either horizontally or vertically. Implements IEnumerable, can
/// be enumerated across stored MenuElements.
/// </summary>
public class Aligner : MenuElement, IEnumerable<MenuElement> {
    private AlignDirection direction;
    private List<MenuElement> contents = new();
    private Queue<MenuElement> toRemove = new();
    private Queue<MenuElement> toAdd = new();
    private bool markedToClear;

    /// <summary>
    /// Number of elements within this Aligner
    /// </summary>
    public int Count => contents.Count;

    /// <summary>
    /// Creates a new Aligner element
    /// </summary>
    /// <param name="direction">Direction to align</param>
    /// <param name="style">Style rules for this aligner</param>
    public Aligner(AlignDirection direction, ElementStyle style)
    : base(Rectangle.Empty, style) {
        this.direction = direction;

        Style.BackgroundColor = Color.Transparent;
        Style.BorderSize = 0;
    }

    /// <summary>
    /// Creates a new Aligner element
    /// </summary>
    /// <param name="direction">Direction to align</param>
    public Aligner(AlignDirection direction)
    : this(direction, new ElementStyle()) { }

    #region // IEnumerable implementation

    /// <summary>
    /// Gets the contents of this aligner as an enumerator of menu elements
    /// </summary>
    /// <returns>Enumerator of elements within this aligner</returns>
    public IEnumerator<MenuElement> GetEnumerator() {
        return contents.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }

    #endregion

    /// <summary>
    /// Updates positioning/centering of aligner,
    /// and updates all internal elements.
    /// </summary>
    /// <param name="dt">Time passed since last frame</param>
    public override void Update(float dt) {
        // align elements, setting their position to an aligned position
        switch (direction) {
            case AlignDirection.Horizontal:
                AlignHorizontally();
                break;
            case AlignDirection.Vertical:
                AlignVertically();
                break;
        }

        // reset marginless bounds to empty at current aligner
        //   position, ready to resize w/ union in loop
        MarginlessBounds = new(Position.ToPoint(), Point.Zero);

        // update all internally stored elements, update dropdown
        //   property, and recalculate bounds
        for (int i = contents.Count - 1; i >= 0; i--) {
            contents[i].Update(dt);

            // use Rectangle.Union to perfectly encapsulate all elements stored within
            MarginlessBounds = Rectangle.Union(MarginlessBounds, contents[i].Bounds);
        }

        // clears contents if marked for clearing from "QueueClear"
        //   OUTSIDE the update loop
        if (markedToClear) {
            Clear();
            markedToClear = false;
        }

        // use queues to remove/add elements OUTSIDE the update loop
        while (toRemove.Count > 0) Remove(toRemove.Dequeue());
        while (toAdd.Count > 0) Add(toAdd.Dequeue());
    }

    /// <summary>
    /// Draws this aligner, with all of its contents
    /// </summary>
    /// <param name="sb">SpriteBound to draw with</param>
    public override void Draw(SpriteBatch sb) {
        sb.DrawRectFill(MarginlessBounds, Style.BackgroundColor);
        sb.DrawRectOutline(
            Utils.ExpandRect(MarginlessBounds, Style.BorderSize),
            Style.BorderSize,
            Style.BorderColor
        );

        foreach (MenuElement element in contents) {
            element.Draw(sb);
        }
    }

    /// <summary>
    /// Draws all overlays within this aligner
    /// </summary>
    /// <param name="sb">SpriteBatch to draw with</param>
    public override void DrawOverlays(SpriteBatch sb) {
        foreach (MenuElement element in contents) {
            element.DrawOverlays(sb);
        }
    }

    /// <summary>
    /// Draws debug information for this aligner
    /// and all internally stored elements
    /// </summary>
    /// <param name="sb">SpriteBatch to draw with</param>
    public override void DebugDraw(SpriteBatch sb) {
        base.DebugDraw(sb);
        foreach (MenuElement element in contents) {
            element.DebugDraw(sb);
        }
    }

    #region // Data management

    /// <summary>
    /// Adds an element to this aligner instantly
    /// </summary>
    /// <param name="element">Element to add</param>
    public void Add(MenuElement element) {
        // whenever adding a menu element, make the vertical/horizontal
        //   alignment mirror that of this aligner
        element.Style.YAlignment = Style.YAlignment;
        element.Style.XAlignment = Style.XAlignment;

        contents.Add(element);
    }

    /// <summary>
    /// Adds an element to this aligner at the end of its update sequence
    /// </summary>
    /// <param name="element">Element to add</param>
    public void QueueAdd(MenuElement element) {
        toAdd.Enqueue(element);
    }

    /// <summary>
    /// Removes an element from this aligner instantly
    /// </summary>
    /// <param name="element">Element to remove</param>
    /// <returns>True if element was returned successfully, false if not</returns>
    public bool Remove(MenuElement element) {
        return contents.Remove(element);
    }

    /// <summary>
    /// Removes an element from this aligner at the end of its update sequence
    /// </summary>
    /// <param name="element">Element to remove</param>
    public void QueueRemove(MenuElement element) {
        toRemove.Enqueue(element);
    }

    /// <summary>
    /// Clears out this aligner of all elements instantly
    /// </summary>
    public void Clear() {
        contents.Clear();
    }

    /// <summary>
    /// Clears out this aligner of all elements at the end of its update sequence
    /// </summary>
    public void QueueClear() {
        markedToClear = true;
    }

    #endregion

    private void AlignVertically() {
        // height of all contents summated
        int totalHeight = 0;
        foreach (MenuElement element in contents)
            totalHeight += element.Bounds.Height;

        // start y position based on center pos of this collection
        int startY = Bounds.Center.Y - (totalHeight / 2);

        // position all items vertically lined up
        for (int i = 0; i < contents.Count; i++) {
            // if iteration isn't the first one, make the previous Y value
            //   the bottom-aligned coord of the previous element's bounds
            int prevY = i > 0 ?
                contents[i - 1].Bounds.Bottom :
                startY;

            Vector2 newPos = new(Position.X, 0);

            switch (Style.YAlignment) {
                case YAlign.Top:
                    newPos.Y = prevY;
                    break;
                case YAlign.Center:
                    newPos.Y = prevY + contents[i].Bounds.Height / 2;
                    break;
                case YAlign.Bottom:
                    newPos.Y = prevY + contents[i].Bounds.Height;
                    break;
            }

            contents[i].Position = newPos;
        }
    }

    private void AlignHorizontally() {
        // width of all contents summated
        int totalWidth = 0;
        foreach (MenuElement element in contents) {
            totalWidth += element.Bounds.Width;
        }

        // start x position based on center pos of this collection
        int startX = Bounds.Center.X - (totalWidth / 2);

        // position all items horizontally lined up
        for (int i = 0; i < contents.Count; i++) {
            // if iteration isn't the first one, make the previous X value
            //   the right-aligned coord of the previous element's bounds
            int prevX = i > 0 ?
                contents[i - 1].Bounds.Right :
                startX;

            Vector2 newPos = new(
                prevX + contents[i].Bounds.Width / 2,
                Position.Y
            );

            contents[i].Position = newPos;
        }
    }
}
