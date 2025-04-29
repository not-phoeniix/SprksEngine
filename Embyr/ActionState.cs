using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Embyr;

/// <summary>
/// A structure full of the boolean states of input actions at a given frame
/// </summary>
public readonly struct ActionState {
    private readonly uint bitState;
    private readonly Vector2 moveDir;

    /// <summary>
    /// Gets the 2-dimensional movement direction vector for this frame
    /// </summary>
    public Vector2 MoveDirection => moveDir;

    /// <summary>
    /// Creates a new ActionState by grabbing and combining input
    /// states from Keyboard, Mouse, and GamePad static classes
    /// </summary>
    /// <param name="preset">ActionBindingPreset to base input searching from</param>
    /// <param name="disableDirectionals">Whether or not to disable checking for direction actions (like ui UDLR controls)</param>
    /// <param name="normalizeMovement">Whether or not to normalize movement vector</param>
    public ActionState(ActionBindingPreset preset, bool disableDirectionals = false, bool normalizeMovement = false) {
        bitState = 0;

        // don't create action state if preset is null
        if (preset == null) return;

        KeyboardState kb = Keyboard.GetState();
        MouseState ms = Mouse.GetState();
        GamePadState gs = GamePad.GetState(0);

        foreach (InputAction action in Enum.GetValues<InputAction>()) {
            // skip checking for directionals if specified, using
            //   hashed switch for efficiency
            if (disableDirectionals) {
                switch (action) {
                    case InputAction.Left:
                    case InputAction.Right:
                    case InputAction.Up:
                    case InputAction.Down:
                    case InputAction.UILeft:
                    case InputAction.UIRight:
                    case InputAction.UIUp:
                    case InputAction.UIDown:
                        continue;
                }
            }

            // keyboard/mouse state grabbing
            string kbBind = preset.GetKeyboardMouse(action);
            switch (kbBind) {
                case "LeftMouse":
                    if (ms.LeftButton == ButtonState.Pressed) {
                        bitState |= (uint)action;
                    }
                    break;

                case "RightMouse":
                    if (ms.RightButton == ButtonState.Pressed) {
                        bitState |= (uint)action;
                    }
                    break;

                case "MiddleMouse":
                    if (ms.MiddleButton == ButtonState.Pressed) {
                        bitState |= (uint)action;
                    }
                    break;

                default:
                    if (Enum.TryParse(kbBind, out Keys key) && kb.IsKeyDown(key)) {
                        bitState |= (uint)action;
                    }
                    break;
            }

            // gamepad state grabbing
            string gpBind = preset.GetGamePad(action);
            if (Enum.TryParse(gpBind, out Buttons button) && gs.IsButtonDown(button)) {
                // this should hopefully account for null (hopefully)
                bitState |= (uint)action;
            }
        }

        // update movement direction vector

        moveDir = Vector2.Zero;

        if (IsDown(InputAction.Left)) {
            moveDir.X -= 1.0f;
        }
        if (IsDown(InputAction.Right)) {
            moveDir.X += 1.0f;
        }
        if (IsDown(InputAction.Up)) {
            moveDir.Y -= 1.0f;
        }
        if (IsDown(InputAction.Down)) {
            moveDir.Y += 1.0f;
        }

        if (normalizeMovement && moveDir.X != 0 && moveDir.Y != 0) {
            // normalize vector if going diagonally without needing
            //   to actually use normalization and perform division
            //   by using precalculated values for a normal vector
            moveDir.X *= 0.707107f;
            moveDir.Y *= 0.707107f;
        }
    }

    /// <summary>
    /// Creates a new ActionState with the default action binding preset
    /// </summary>
    public ActionState() : this(ActionBindingPreset.Default) { }

    /// <summary>
    /// Grabs whether or not an action is activated for this action state
    /// </summary>
    /// <param name="action">Action to check activation for </param>
    /// <returns>True if action is activated, false if not</returns>
    public bool IsDown(InputAction action) {
        uint andValue = bitState & (uint)action;
        return andValue != 0;
    }
}
