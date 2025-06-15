using Embyr.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Embyr;

/// <summary>
/// Represents the current state of the input system, can either be keyboard
/// or controller, and switches depending on what the previous input was
/// </summary>
public enum InputMode {
    Keyboard,
    Gamepad
}

/// <summary>
/// A static utility class to help with input, has a few useful
/// extra methods and updating for previous/current frame states
/// </summary>
public static class Input {
    private struct VibrationParams {
        private readonly PlayerIndex index;
        private readonly float duration;
        private readonly float initialStrength;
        private readonly bool lerpDecay;
        private float currentTime;

        public VibrationParams(
            PlayerIndex index,
            float duration,
            float initialStrength,
            bool lerpDecay
        ) {
            this.index = index;
            this.duration = duration;
            this.initialStrength = Math.Clamp(initialStrength, 0, 1);
            this.lerpDecay = lerpDecay;
            this.currentTime = 0;
        }

        public VibrationParams() : this(PlayerIndex.One, 0, 0, false) { }

        public void Update(float dt) {
            currentTime += dt;
            float strength = 0;

            if (currentTime <= duration) {
                if (lerpDecay) {
                    float lerpAmt = 1 - (currentTime / duration);
                    lerpAmt = Math.Clamp(lerpAmt, 0, 1);
                    strength = initialStrength * lerpAmt;
                } else {
                    strength = initialStrength;
                }
            }

            GamePad.SetVibration(index, strength, strength);
        }
    }

    private static KeyboardState kbState;
    private static KeyboardState kbStatePrev;
    private static MouseState mState;
    private static MouseState mStatePrev;
    private static GamePadState padState;
    private static GamePadState padStatePrev;
    private static ActionState actionState;
    private static ActionState actionStatePrev;
    private static Matrix mouseCanvasTransform;
    private static Matrix canvasWorldTransform;
    private static VibrationParams currentVibration;
    private static InputMode modePrev;
    private static readonly Queue<Delegate> delegatesToClear = new();

    #region // Properties

    /// <summary>
    /// Gets the current state, whether or not keyboard/gamepad is being used
    /// </summary>
    public static InputMode Mode { get; private set; }

    /// <summary>
    /// Event executed when current input mode changes, passes in
    /// the new InputMode, cleared between scene changes
    /// </summary>
    public static event Action<InputMode>? OnInputModeChanged;

    /// <summary>
    /// Current mouse state's full screen position
    /// </summary>
    public static Vector2 MouseWindowPos {
        get { return mState.Position.ToVector2(); }
    }

    /// <summary>
    /// Gets current mouse state's screen position,
    /// transformed to be relative to the
    /// low res render canvas
    /// </summary>
    public static Vector2 MousePos {
        get { return Vector2.Transform(MouseWindowPos, mouseCanvasTransform); }
    }

    /// <summary>
    /// Gets the change in mouse positions in pixels from last frame
    /// </summary>
    public static Vector2 MousePosDelta {
        get {
            Vector2 pos = Vector2.Transform(mState.Position.ToVector2(), mouseCanvasTransform);
            Vector2 prevPos = Vector2.Transform(mStatePrev.Position.ToVector2(), mouseCanvasTransform);
            return pos - prevPos;
        }
    }

    /// <summary>
    /// Gets position of mouse in world-space
    /// </summary>
    public static Vector2 MouseWorldPos {
        get { return Vector2.Transform(MousePos, canvasWorldTransform); }
    }

    /// <summary>
    /// Gets the total scroll wheel accumulation since the start of the game
    /// </summary>
    public static int ScrollWheelTotal => mState.ScrollWheelValue;

    /// <summary>
    /// Gets the change in scroll wheel values across frames
    /// </summary>
    public static int ScrollWheelDelta => mState.ScrollWheelValue - mStatePrev.ScrollWheelValue;

    /// <summary>
    /// Gets whether or not the mouse state has changed since last frame
    /// </summary>
    public static bool MouseHasChanged {
        get { return mState != mStatePrev; }
    }

    /// <summary>
    /// Gets whether or not the keyboard state has changed since last frame
    /// </summary>
    public static bool KeyboardHasChanged {
        get { return kbState != kbStatePrev; }
    }

    /// <summary>
    /// Gets whether or not the gamepad state has changed since last frame
    /// </summary>
    public static bool GamepadHasChanged {
        get { return padState != padStatePrev; }
    }

    /// <summary>
    /// Gets/sets the current action binding preset for the input manager
    /// </summary>
    internal static ActionBindingPreset CurrentBindingPreset { get; set; }

    #endregion

    #region // Public methods

    /// <summary>
    /// MUST BE RUN EVERY FRAME TO WORK... Updates all input states
    /// </summary>
    /// <param name="mouseCanvasTransform">Transformation matrix of mouse that maps screen to canvas position</param>
    /// <param name="invertedCameraTransform">Inverted transformation matrix that maps canvas positions into the world</param>
    /// <param name="dt">Time passed since last frame</param>
    internal static void Update(Matrix mouseCanvasTransform, Matrix invertedCameraTransform, float dt) {
        // update static fields
        Input.mouseCanvasTransform = mouseCanvasTransform;
        Input.canvasWorldTransform = invertedCameraTransform;

        // update individual input states
        kbStatePrev = kbState;
        mStatePrev = mState;
        padStatePrev = padState;
        actionStatePrev = actionState;
        kbState = Keyboard.GetState();
        mState = Mouse.GetState();
        padState = GamePad.GetState(0);
        actionState = new ActionState(CurrentBindingPreset);

        // update class's internal state, only updates when a state is changed
        //   (therefore a button/key has been pressed, mouse moved, stick flicked, etc)
        if (kbState != kbStatePrev || mState != mStatePrev) {
            Mode = InputMode.Keyboard;
        } else if (
            padState.Buttons != padStatePrev.Buttons ||
            padState.ThumbSticks != padStatePrev.ThumbSticks ||
            padState.DPad != padStatePrev.DPad ||
            padState.Triggers != padStatePrev.Triggers
        ) {
            Mode = InputMode.Gamepad;
        }

        // update vibration only if in gamepad mode, otherwise reset to zero
        if (Mode == InputMode.Gamepad) {
            currentVibration.Update(dt);
        } else {
            currentVibration = new VibrationParams();
        }

        // when mode changes, invoke event (if it's not null)
        if (Mode != modePrev) {
            OnInputModeChanged?.Invoke(Mode);
        }

        modePrev = Mode;
    }

    #region // Event management

    /// <summary>
    /// Queues all events inside this input manager currently to clear
    /// </summary>
    public static void QueueEventsToClear() {
        if (OnInputModeChanged != null) {
            foreach (Delegate d in OnInputModeChanged.GetInvocationList()) {
                delegatesToClear.Enqueue(d);
            }
        }
    }

    /// <summary>
    /// Removes all previously queued delegates in input manager from internal events
    /// </summary>
    public static void ClearQueuedEvents() {
        while (delegatesToClear.Count > 0) {
            Delegate d = delegatesToClear.Dequeue();

            if (d is Action<InputMode> modeChangedDelegate && OnInputModeChanged != null) {
                OnInputModeChanged -= modeChangedDelegate;
            }
        }
    }

    /// <summary>
    /// Disgards all previosuly queued delegates to clear, resetting events
    /// </summary>
    public static void DiscardEventQueue() {
        delegatesToClear.Clear();
    }

    #endregion

    /// <summary>
    /// Returns boolean whether or not a key is pressed this current frame
    /// </summary>
    /// <param name="key">Key to check down state for</param>
    /// <returns>True if key is down, false if not</returns>
    public static bool IsKeyDown(Keys key) {
        return kbState.IsKeyDown(key);
    }

    /// <summary>
    /// Returns boolean whatehr or not any modifier keys
    /// (shift, ctrl, alt) are pressed in this current frame
    /// </summary>
    /// <returns>True if a modifier key is pressed, false if not</returns>
    public static bool AnyModifierDown() {
        return kbState.IsKeyDown(Keys.LeftControl) ||
               kbState.IsKeyDown(Keys.RightControl) ||
               kbState.IsKeyDown(Keys.LeftShift) ||
               kbState.IsKeyDown(Keys.RightShift) ||
               kbState.IsKeyDown(Keys.LeftAlt) ||
               kbState.IsKeyDown(Keys.RightAlt);
    }

    /// <summary>
    /// Boolean of whether or not left mouse button is down this current frame
    /// </summary>
    /// <returns>True if mouse is down, false if not</returns>
    public static bool IsLeftMouseDown() {
        return mState.LeftButton == ButtonState.Pressed;
    }

    /// <summary>
    /// Boolean of whether or not right mouse button is down this current frame
    /// </summary>
    /// <returns>True if mouse is down, false if not</returns>
    public static bool IsRightMouseDown() {
        return mState.RightButton == ButtonState.Pressed;
    }

    /// <summary>
    /// Boolean of whether or not middle mouse button is down this current frame
    /// </summary>
    /// <returns>True if mouse is down, false if not</returns>
    public static bool IsMiddleMouseDown() {
        return mState.MiddleButton == ButtonState.Pressed;
    }

    /// <summary>
    /// Returns boolean whether or not a gamepad button is pressed this current frame
    /// </summary>
    /// <param name="button">Button to check down state for</param>
    /// <returns>True if button is down, false if not</returns>
    public static bool IsButtonPressed(Buttons button) {
        return padState.IsButtonDown(button);
    }

    /// <summary>
    /// Non-repeating key detecting, only true for one frame
    /// </summary>
    /// <param name="key">Key to check for down state</param>
    /// <returns>True if current state is down, previous state is up, false if not</returns>
    public static bool IsKeyDownOnce(Keys key) {
        return kbState.IsKeyDown(key) && !kbStatePrev.IsKeyDown(key);
    }

    /// <summary>
    /// Non-repeating left mouse button detecting, only true for one frame
    /// </summary>
    /// <returns>True if currently clicked and previous is released, false if not</returns>
    public static bool IsLeftMouseDownOnce() {
        return mState.LeftButton == ButtonState.Pressed && mStatePrev.LeftButton == ButtonState.Released;
    }

    /// <summary>
    /// Non-repeating right mouse button detecting, only true for one frame
    /// </summary>
    /// <returns>True if currently clicked and previous is released, false if not</returns>
    public static bool IsRightMouseDownOnce() {
        return mState.RightButton == ButtonState.Pressed && mStatePrev.RightButton == ButtonState.Released;
    }

    /// <summary>
    /// Non-repeating middle mouse button detecting, only true for one frame
    /// </summary>
    /// <returns>True if currently clicked and previous is released, false if not</returns>
    public static bool IsMiddleMouseDownOnce() {
        return mState.MiddleButton == ButtonState.Pressed && mStatePrev.MiddleButton == ButtonState.Released;
    }

    /// <summary>
    /// Non-repeating button checking, only true for one frame
    /// </summary>
    /// <param name="button">Button to check down state for</param>
    /// <returns>True if button is down for one frame, false if not</returns>
    public static bool IsButtonPressedOnce(Buttons button) {
        return padState.IsButtonDown(button) && !padStatePrev.IsButtonDown(button);
    }

    /// <summary>
    /// Gets whether or not an action has been activated this frame
    /// </summary>
    /// <param name="action">Name of action to check for</param>
    /// <returns>True if action is activated, false if not</returns>
    public static bool IsAction(string action) {
        int offset = CurrentBindingPreset.GetBindingBitShiftOffset(action);
        return actionState.IsDown(offset);
    }

    /// <summary>
    /// Gets whether or not an action has been activated for the first time this frame
    /// </summary>
    /// <param name="action">Name of action to check for</param>
    /// <returns>True if action is activated this frame and not last frame, false if not</returns>
    public static bool IsActionOnce(string action) {
        int offset = CurrentBindingPreset.GetBindingBitShiftOffset(action);
        return actionState.IsDown(offset) && !actionStatePrev.IsDown(offset);
    }

    /// <summary>
    /// Gets the composite activation value along a 1D axis with two actions
    /// </summary>
    /// <param name="negative">Name of action to use as negative</param>
    /// <param name="positive">Name of action to use as positive</param>
    /// <returns>Composite float value from -1 to 1 of action axis input</returns>
    public static float GetComposite1D(string negative, string positive) {
        return actionState.GetComposite1D(
            CurrentBindingPreset.GetBindingBitShiftOffset(negative),
            CurrentBindingPreset.GetBindingBitShiftOffset(positive)
        );
    }

    /// <summary>
    /// Gets the composite activation value along a 2D axis with four actions
    /// </summary>
    /// <param name="negative_x">Name of action to use as negative x axis</param>
    /// <param name="negative_y">Name of action to use as negative y axis</param>
    /// <param name="positive_x">Name of action to use as positive x axis</param>
    /// <param name="positive_y">Name of action to use as positive y axis</param>
    /// <param name="normalize">Whether or not to normalize vector</param>
    /// <returns>Composite 2D vector value from -1 to 1 of action axis input</returns>
    public static Vector2 GetComposite2D(
        string negative_x,
        string negative_y,
        string positive_x,
        string positive_y,
        bool normalize = false
    ) {
        return actionState.GetComposite2D(
            CurrentBindingPreset.GetBindingBitShiftOffset(negative_x),
            CurrentBindingPreset.GetBindingBitShiftOffset(negative_y),
            CurrentBindingPreset.GetBindingBitShiftOffset(positive_x),
            CurrentBindingPreset.GetBindingBitShiftOffset(positive_y),
            normalize
        );
    }

    /// <summary>
    /// Gets the composite activation value along a 3D axis with six actions
    /// </summary>
    /// <param name="negative_x">Name of action to use as negative x axis</param>
    /// <param name="negative_y">Name of action to use as negative y axis</param>
    /// <param name="negative_z">Name of action to use as negative z axis</param>
    /// <param name="positive_x">Name of action to use as positive x axis</param>
    /// <param name="positive_y">Name of action to use as positive y axis</param>
    /// <param name="positive_z">Name of action to use as positive z axis</param>
    /// <param name="normalize">Whether or not to normalize vector</param>
    /// <returns>Composite 3D vector value from -1 to 1 of action axis input</returns>
    public static Vector3 GetComposite3D(
        string negative_x,
        string negative_y,
        string negative_z,
        string positive_x,
        string positive_y,
        string positive_z,
        bool normalize = false
    ) {
        return actionState.GetComposite3D(
            CurrentBindingPreset.GetBindingBitShiftOffset(negative_x),
            CurrentBindingPreset.GetBindingBitShiftOffset(negative_y),
            CurrentBindingPreset.GetBindingBitShiftOffset(negative_z),
            CurrentBindingPreset.GetBindingBitShiftOffset(positive_x),
            CurrentBindingPreset.GetBindingBitShiftOffset(positive_y),
            CurrentBindingPreset.GetBindingBitShiftOffset(positive_z),
            normalize
        );
    }

    /// <summary>
    /// Queues a gamepad vibration to be updated at the interval of every frame
    /// </summary>
    /// <param name="index">Controller/player index</param>
    /// <param name="initialStrength">Initial vibration strength to start LERPing from, from 0.0-1.0</param>
    /// <param name="duration">Duration of vibration lerp in seconds</param>
    /// <param name="lerpDecay">Whether or not to do linear decay of strength or just a constant vibration</param>
    public static void QueueGamepadVibrate(
        float duration,
        float initialStrength,
        bool lerpDecay = true,
        PlayerIndex index = PlayerIndex.One
    ) {
        currentVibration = new VibrationParams(
            index,
            duration,
            initialStrength,
            lerpDecay
        );
    }

    /// <summary>
    /// Calculates a Vector2 "center offset" which is between -1 and 1 in both components,
    /// represents right stick and also left mouse position relative to the center of the
    /// screen, can be used for camera offsets
    /// </summary>
    /// <returns>Vector2 representing look direction</returns>
    public static Vector2 GetMouseCenterOffset() {
        Vector2 direction = Vector2.Zero;

        switch (Mode) {
            case InputMode.Keyboard:
                // calculate offset
                Point center = SceneManager.I.GraphicsDevice.Viewport.Bounds.Center;
                Vector2 offset = MouseWindowPos - center.ToVector2();

                // scale each component to be between -1 to 1
                offset.X /= center.X;
                offset.Y /= center.Y;

                // set direction to offset
                direction = offset;
                break;

            case InputMode.Gamepad:
                direction = padState.ThumbSticks.Right;
                direction.Y *= -1;
                break;
        }

        return direction;
    }

    /// <summary>
    /// Returns string of all keyboard input this frame (used for text input)
    /// </summary>
    /// <returns>String concatenation of all pressed keys</returns>
    public static string GetKeyboardString() {
        Keys[] pressedKeys = kbState.GetPressedKeys();
        string concatenation = "";

        bool shiftPressed =
            kbState.IsKeyDown(Keys.LeftShift) ||
            kbState.IsKeyDown(Keys.RightShift);

        // decides what character to add to the string
        foreach (Keys key in pressedKeys) {
            if (kbStatePrev.IsKeyUp(key)) {
                switch (key) {
                    case Keys.Space:
                        concatenation += " ";
                        break;

                    case Keys.OemBackslash:
                        concatenation += "\\";
                        break;

                    case Keys.OemCloseBrackets:
                        if (shiftPressed) {
                            concatenation += "}";
                        } else {
                            concatenation += "]";
                        }
                        break;

                    case Keys.OemComma:
                        if (shiftPressed) {
                            concatenation += "<";
                        } else {
                            concatenation += ",";
                        }
                        break;

                    case Keys.OemMinus:
                        if (shiftPressed) {
                            concatenation += "_";
                        } else {
                            concatenation += "-";
                        }
                        break;

                    case Keys.OemOpenBrackets:
                        if (shiftPressed) {
                            concatenation += "{";
                        } else {
                            concatenation += "[";
                        }
                        break;

                    case Keys.OemPeriod:
                        if (shiftPressed) {
                            concatenation += ">";
                        } else {
                            concatenation += ".";
                        }
                        break;

                    case Keys.OemPipe:
                        concatenation += "|";
                        break;

                    case Keys.OemPlus:
                        if (!shiftPressed) {
                            concatenation += "=";
                        } else {
                            concatenation += "+";
                        }
                        break;

                    case Keys.OemQuestion:
                        if (!shiftPressed) {
                            concatenation += "/";
                        } else {
                            concatenation += "?";
                        }
                        break;

                    case Keys.OemQuotes:
                        if (!shiftPressed) {
                            concatenation += "'";
                        } else {
                            concatenation += "\"";
                        }
                        break;

                    case Keys.OemSemicolon:
                        if (shiftPressed) {
                            concatenation += ":";
                        } else {
                            concatenation += ";";
                        }
                        break;

                    case Keys.Tab:
                        concatenation += "\t";
                        break;

                    case Keys.D0:
                        if (!shiftPressed) {
                            concatenation += "0";
                        } else {
                            concatenation += ")";
                        }
                        break;

                    case Keys.D1:
                        if (!shiftPressed) {
                            concatenation += "1";
                        } else {
                            concatenation += "!";
                        }
                        break;

                    case Keys.D2:
                        if (!shiftPressed) {
                            concatenation += "2";
                        } else {
                            concatenation += "@";
                        }
                        break;

                    case Keys.D3:
                        if (!shiftPressed) {
                            concatenation += "3";
                        } else {
                            concatenation += "#";
                        }
                        break;

                    case Keys.D4:
                        if (!shiftPressed) {
                            concatenation += "4";
                        } else {
                            concatenation += "$";
                        }
                        break;

                    case Keys.D5:
                        if (!shiftPressed) {
                            concatenation += "5";
                        } else {
                            concatenation += "%";
                        }
                        break;

                    case Keys.D6:
                        if (!shiftPressed) {
                            concatenation += "6";
                        } else {
                            concatenation += "^";
                        }
                        break;

                    case Keys.D7:
                        if (!shiftPressed) {
                            concatenation += "7";
                        } else {
                            concatenation += "&";
                        }
                        break;

                    case Keys.D8:
                        if (!shiftPressed) {
                            concatenation += "8";
                        } else {
                            concatenation += "*";
                        }
                        break;

                    case Keys.D9:
                        if (!shiftPressed) {
                            concatenation += "9";
                        } else {
                            concatenation += "(";
                        }
                        break;

                    // regular keyboard keys
                    default:
                        if (key == Keys.LeftShift ||
                            key == Keys.RightShift ||
                            key.ToString().Length != 1
                        ) {
                            continue;
                        }

                        // modify "shiftPressed" for all regular letter
                        //   keys to be inverted if caps lock is enabled
                        bool shiftCapsMod = shiftPressed;
                        if (kbState.CapsLock) {
                            shiftCapsMod = !shiftCapsMod;
                        }

                        if (shiftCapsMod) {
                            concatenation += key.ToString().ToUpper();
                        } else {
                            concatenation += key.ToString().ToLower();
                        }
                        break;

                }
            }
        }

        return concatenation;
    }

    /// <summary>
    /// Edits an inputted string reference with internal keyboard text
    /// input (THIS INCLUDES BACKSPACE FOR DELETION)
    /// </summary>
    /// <param name="edit">String reference to edit</param>
    public static void UpdateKeyboardString(ref string edit) {
        bool ctrlPressed =
            kbState.IsKeyDown(Keys.LeftControl) ||
            kbState.IsKeyDown(Keys.RightControl);

        if (IsKeyDownOnce(Keys.Back) && edit.Length > 0) {
            if (ctrlPressed) {
                // find index of last space (as long as it's not a trailing space)
                int spaceIndex = edit.Length - 1;
                bool foundANonSpaceYet = false;
                char iterChar;
                do {
                    // track whether or not a space has been found so
                    //   strings that end in a space or three will still
                    //   remove the end word and it'll skip past those
                    //   initial spaces
                    iterChar = edit[spaceIndex];
                    if (iterChar != ' ') {
                        foundANonSpaceYet = true;
                    }

                    spaceIndex--;

                    if (spaceIndex < 0) {
                        // make index -2 so below it sets the substring
                        //   length to zero, removing the entire last
                        //   word (not leaving the first character)
                        spaceIndex = -2;
                        break;
                    }
                } while (iterChar != ' ' || !foundANonSpaceYet);

                // substring the inputted string to remove all characters
                //   after the space (removing that word lol)
                // (index is +2'd so one space is left behind (mirrors most OS's))
                edit = edit.Substring(0, spaceIndex + 2);

            } else {
                // if ctrl not pressed, just remove one char lol
                string modified = edit.Remove(edit.Length - 1);
                edit = modified;
            }
        } else {
            edit += GetKeyboardString();
        }
    }

    #endregion
}
