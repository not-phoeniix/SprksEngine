using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Embyr.Scenes;

namespace Embyr.UI;

/// <summary>
/// Representation of a menu
/// </summary>
public abstract class Menu : IDrawable2D, IDebugDrawable, IResolution {
    private Color backgroundColor = Color.Black;
    private Texture2D backgroundImg;
    private Queue<MenuElement> toAdd = new();
    private Queue<MenuElement> toRemove = new();
    private IMenuInteractable selectedInteractable = null;
    private bool mouseMode = true;

    /// <summary>
    /// Gets the bounding rectangle for this menu on screen
    /// </summary>
    public Rectangle Bounds { get; protected set; }

    /// <summary>
    /// List of all menu items to be drawn and updated
    /// </summary>
    protected List<MenuElement> Elements { get; private set; } = new();

    /// <summary>
    /// Currently selected element, automatically modifies previous/next "selected" properties
    /// </summary>
    protected IMenuInteractable SelectedInteractable {
        get { return selectedInteractable; }
        set {
            if (selectedInteractable != null) selectedInteractable.Hovered = false;
            selectedInteractable = value;
            if (selectedInteractable != null) selectedInteractable.Hovered = true;
        }
    }

    /// <summary>
    /// Ordered list of all interactable elements used for controller menu interaction
    /// </summary>
    protected List<IMenuInteractable> InteractableElements { get; private set; } = new();

    /// <summary>
    /// When true, controller input is captured, menus should not pop when this is happening
    /// </summary>
    protected bool ControllerInputIsHandling { get; private set; }

    /// <summary>
    /// What is executed when the back action is pressed (esc/B button)
    /// </summary>
    public event Action OnBackAction;

    /// <summary>
    /// Creates an instance of a menu
    /// </summary>
    /// <param name="backgroundColor">Color to draw behind everything</param>
    /// <param name="bounds">Bounding rectangle of menu</param>
    /// <param name="enableBackPop">Whether or not to enable menu stack popping upon back action</param>
    public Menu(Color backgroundColor, Rectangle bounds, bool enableBackPop = true) {
        this.backgroundColor = backgroundColor;
        this.Bounds = bounds;

        if (enableBackPop) {
            OnBackAction += () => {
                if (!ControllerInputIsHandling) {
                    SceneManager.I.CurrentScene.MenuStackPop();
                }
            };
        }
    }

    /// <summary>
    /// Creates an instance of a menu
    /// </summary>
    /// <param name="backgroundColor">Color to draw as menu background</param>
    /// <param name="opacity">Float 0-1 opacity of background color</param>
    /// <param name="bounds">Bounding rectangle of menu</param>
    /// <param name="enableBackPop">Whether or not to enable menu stack popping upon back action</param>
    public Menu(Color backgroundColor, float opacity, Rectangle bounds, bool enableBackPop = true)
    : this(
        Color.FromNonPremultiplied(
            backgroundColor.R,
            backgroundColor.G,
            backgroundColor.B,
            (int)(255 * opacity)),
        bounds,
        enableBackPop
    ) { }

    /// <summary>
    /// Creates an instance of a menu
    /// </summary>
    /// <param name="backgroundImg">Background image to draw behind UI</param>
    /// <param name="backgroundColor">Color to draw behind everything</param>
    /// <param name="bounds">Bounding rectangle of menu</param>
    /// <param name="enableBackPop">Whether or not to enable menu stack popping upon back action</param>
    public Menu(Texture2D backgroundImg, Color backgroundColor, Rectangle bounds, bool enableBackPop = true) {
        this.backgroundImg = backgroundImg;
        this.backgroundColor = backgroundColor;
        this.Bounds = bounds;

        if (enableBackPop) {
            OnBackAction += () => {
                if (!ControllerInputIsHandling) {
                    SceneManager.I.CurrentScene.MenuStackPop();
                }
            };
        }
    }

    /// <summary>
    /// Adds an item to the menu
    /// </summary>
    /// <param name="element">Element to add</param>
    public void AddElement(MenuElement element) {
        if (element == null) return;

        Elements.Add(element);
        if (element is IMenuInteractable interactable) {
            InteractableElements.Add(interactable);
        }

        // if the element removed is enumerable, add all its
        //   containing interactable elements
        if (element is IEnumerable<MenuElement> enumerble) {
            AddEnumerableInteractables(enumerble);
        }
    }

    /// <summary>
    /// Recursively adds all interactables in
    /// inputted menu element and all internal elements
    /// </summary>
    /// <param name="element">Element to search</param>
    private void AddEnumerableInteractables(IEnumerable<MenuElement> element) {
        if (element == null) return;

        foreach (MenuElement child in element) {
            if (child is IMenuInteractable i) InteractableElements.Add(i);
            if (child is IEnumerable<MenuElement> e) AddEnumerableInteractables(e);
        }
    }

    /// <summary>
    /// Removes an item from the menu
    /// </summary>
    /// <param name="element">Element to remove</param>
    public void RemoveElement(MenuElement element) {
        if (element == null) return;

        Elements.Remove(element);
        if (element is IMenuInteractable interactable) {
            InteractableElements.Remove(interactable);
        }

        // if the element removed is enumerable, remove all its
        //   containing interactable elements
        if (element is IEnumerable<MenuElement> enumerable) {
            RemoveEnumerableInteractables(enumerable);
        }
    }

    /// <summary>
    /// Recursively removes all interactables in
    /// inputted menu element and all internal elements
    /// </summary>
    /// <param name="element">Element to search</param>
    private void RemoveEnumerableInteractables(IEnumerable<MenuElement> element) {
        if (element == null) return;

        foreach (MenuElement child in element) {
            if (child is IMenuInteractable i) InteractableElements.Remove(i);
            if (child is IEnumerable<MenuElement> e) RemoveEnumerableInteractables(e);
        }
    }

    /// <summary>
    /// Queues an element to be added to the menu after updating
    /// </summary>
    /// <param name="element">Element to queue to add</param>
    public void QueueAddElement(MenuElement element) {
        if (element != null) {
            toAdd.Enqueue(element);
        }
    }

    /// <summary>
    /// Queues an element to be removed to the menu after updating
    /// </summary>
    /// <param name="element">Element to queue to remove</param>
    public void QueueRemoveElement(MenuElement element) {
        if (element != null) {
            toRemove.Enqueue(element);
        }
    }

    /// <summary>
    /// Changes the resolution of this menu by changing the bounding rectangle values
    /// </summary>
    /// <param name="width">Resolution width (in pixels)</param>
    /// <param name="height">Resolution height (in pixels)</param>
    /// <param name="canvasExpandSize">Number of pixels to expand bounds for scroll smoothing</param>
    public virtual void ChangeResolution(int width, int height, int canvasExpandSize) {
        Bounds = new Rectangle(canvasExpandSize / 2, canvasExpandSize / 2, width, height);
    }

    /// <summary>
    /// Method that positions all elements within this menu,
    /// defined differently in child each class
    /// </summary>
    protected abstract void PositionElements();

    /// <summary>
    /// Method that deals with controller/arrow key input, so a controller
    /// or arrow keys can be used to navigate around the menu
    /// </summary>
    /// <param name="mouseMode">Reference to boolean that dictates whether ui in menu is in "mouse mode"</param>
    protected virtual void HandleControllerInput(ref bool mouseMode) {
        ControllerInputIsHandling = false;

        // don't do button input if there are no elements
        if (InteractableElements.Count == 0) return;

        // assign interactable to be the first element if it's ever null
        SelectedInteractable ??= InteractableElements[0];

        if (Input.IsActionOnce(InputAction.Submit)) {
            mouseMode = false;
        }

        if (SelectedInteractable?.HandleControllerInput(ref mouseMode) ?? false) {
            // if selected interactable is capturing input, update bool flag to show as such
            ControllerInputIsHandling = true;
        } else {
            // only do scrolling if current interactable is done handling input
            if (Input.IsActionOnce(InputAction.UIDown)) {
                ScrollDown();
            }

            if (Input.IsActionOnce(InputAction.UIUp)) {
                ScrollUp();
            }
        }
    }

    /// <summary>
    /// Scrolls down in stored interactables
    /// </summary>
    protected void ScrollDown() {
        mouseMode = false;

        // only select elements that are enabled
        do {
            int index = InteractableElements.IndexOf(SelectedInteractable);
            index++;
            if (index >= InteractableElements.Count) {
                index = 0;
            }

            SelectedInteractable = InteractableElements[index];
        } while (!SelectedInteractable.Enabled);
    }

    /// <summary>
    /// Scrolls up in stored interactables
    /// </summary>
    private void ScrollUp() {
        mouseMode = false;

        // only select elements that are enabled
        do {
            int index = InteractableElements.IndexOf(SelectedInteractable);
            index--;
            if (index < 0) {
                index = InteractableElements.Count - 1;
            }

            SelectedInteractable = InteractableElements[index];
        } while (!SelectedInteractable.Enabled);
    }

    private void HandleMouseHoverInput() {
        bool selectedIsOpenDropdown = SelectedInteractable is Dropdown dropdown && dropdown.Open;
        bool interactableSet = false;

        foreach (IMenuInteractable interactable in InteractableElements) {
            // dont prevent hovering if this iteration's
            //   interactable is the currently selected one
            //   (used for dropdown)
            if (interactable == SelectedInteractable) {
                interactable.UpdateMouseHover(false);
            } else {
                // otherwise, prevent hovering if selected is an open dropdown
                interactable.UpdateMouseHover(selectedIsOpenDropdown);
            }

            if (interactable.Hovered) {
                SelectedInteractable = interactable;
                interactableSet = true;
            }
        }

        if (!interactableSet) {
            SelectedInteractable = null;
        }
    }

    /// <summary>
    /// Updates menu state for input
    /// </summary>
    /// <param name="dt">Time passed since last frame</param>
    public virtual void Update(float dt) {
        // enable mouse mode if mouse state has changed since last frame
        if (Input.MouseHasChanged) {
            mouseMode = true;
        }

        // invoke back action if action is indeed back'd
        if (Input.IsActionOnce(InputAction.Back) && !ControllerInputIsHandling) {
            OnBackAction?.Invoke();
        }

        PositionElements();
        HandleControllerInput(ref mouseMode);

        if (mouseMode) {
            HandleMouseHoverInput();
        }

        foreach (MenuElement item in Elements) {
            item.Update(dt);
        }

        // update queues and adds/removes elements, won't run if queues are empty
        while (toRemove.Count > 0) RemoveElement(toRemove.Dequeue());
        while (toAdd.Count > 0) AddElement(toAdd.Dequeue());
    }

    /// <summary>
    /// Updates physics calculations for this menu
    /// </summary>
    /// <param name="dt">Time passed since last physics update</param>
    public virtual void PhysicsUpdate(float dt) { }

    /// <summary>
    /// Draws the menu to the screen, has sb.Begin() and sb.End() internally,
    /// DONT put inside another spritebatch draw call
    /// </summary>
    /// <param name="sb">SpriteBatch to draw with</param>
    public virtual void Draw(SpriteBatch sb) {
        // draw menu itself
        sb.DrawRectFill(Bounds, backgroundColor);
        if (backgroundImg != null) {
            sb.Draw(backgroundImg, Bounds, Color.White);
        }

        // draw items
        foreach (MenuElement item in Elements) {
            item.Draw(sb);
        }

        // draw item overlays (usually doesn't do anything since there
        //   aren't many elements with defined overlay methods)
        foreach (MenuElement item in Elements) {
            item.DrawOverlays(sb);
        }
    }

    /// <summary>
    /// Draws debug info for all containing menu elements
    /// </summary>
    /// <param name="sb">SpriteBatch to draw with</param>
    public virtual void DebugDraw(SpriteBatch sb) {
        foreach (MenuElement item in Elements) {
            item.DebugDraw(sb);
        }
    }

    /// <summary>
    /// Invokes the back action event of this menu
    /// </summary>
    public void InvokeBackAction() {
        OnBackAction?.Invoke();
    }
}
