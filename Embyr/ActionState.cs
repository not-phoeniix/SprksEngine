using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Embyr;

/// <summary>
/// A structure full of the boolean states of input actions at a given frame
/// </summary>
internal readonly struct ActionState {
    private readonly UInt128 bitState;

    /// <summary>
    /// Creates a new ActionState by grabbing and combining input
    /// states from Keyboard, Mouse, and GamePad static classes
    /// </summary>
    /// <param name="preset">ActionBindingPreset to base input searching from</param>
    /// <param name="disableDirectionals">Whether or not to disable checking for direction actions (like ui UDLR controls)</param>
    /// <param name="normalizeMovement">Whether or not to normalize movement vector</param>
    public ActionState(ActionBindingPreset preset) {
        // don't create action state if preset is null
        if (preset == null) return;

        KeyboardState kb = Keyboard.GetState();
        MouseState ms = Mouse.GetState();
        GamePadState gs = GamePad.GetState(0);

        this.bitState = 0;

        foreach ((int, ActionBindingPreset.Binding) value in preset.Bindings.Values) {
            int shiftOffset = value.Item1;
            ActionBindingPreset.Binding bind = value.Item2;

            bool isActivated = false;

            foreach (Keys k in bind.KeyBinds) {
                if (kb.IsKeyDown(k)) {
                    isActivated = true;
                }
            }

            foreach (Buttons b in bind.ButtonBinds) {
                if (gs.IsButtonDown(b)) {
                    isActivated = true;
                }
            }

            foreach (MouseClick m in bind.MouseBinds) {
                switch (m) {
                    case MouseClick.Left:
                        if (ms.LeftButton == ButtonState.Pressed) {
                            isActivated = true;
                        }
                        break;
                    case MouseClick.Middle:
                        if (ms.MiddleButton == ButtonState.Pressed) {
                            isActivated = true;
                        }
                        break;
                    case MouseClick.Right:
                        if (ms.RightButton == ButtonState.Pressed) {
                            isActivated = true;
                        }
                        break;
                    case MouseClick.Extended1:
                        if (ms.XButton1 == ButtonState.Pressed) {
                            isActivated = true;
                        }
                        break;
                    case MouseClick.Extended2:
                        if (ms.XButton2 == ButtonState.Pressed) {
                            isActivated = true;
                        }
                        break;
                }
            }

            if (isActivated) {
                bitState |= (UInt128)1 << shiftOffset;
            }
        }
    }

    /// <summary>
    /// Creates a new ActionState with the default action binding preset
    /// </summary>
    public ActionState() : this(null) { }

    /// <summary>
    /// Checks whether or not an action is activated for this action state
    /// </summary>
    /// <param name="bitShiftOffset">Bit shift offset of action, acquired from <c>ActionBindingPreset.GetBindingBitShiftOffset</c></param>
    /// <returns>True if action is activated, false if not</returns>
    public bool IsDown(int bitShiftOffset) {
        UInt128 andValue = bitState & ((UInt128)1 << bitShiftOffset);
        return andValue != 0;
    }

    /// <summary>
    /// Gets the composite activation value along a 1D axis with two actions
    /// </summary>
    /// <param name="bitShiftOffsetNegative">Bit shift offset of negative action, acquired from <c>ActionBindingPreset.GetBindingBitShiftOffset</c></param>
    /// <param name="bitShiftOffsetPositive">Bit shift offset of postiive action, acquired from <c>ActionBindingPreset.GetBindingBitShiftOffset</c></param>
    /// <returns>Composite float value from -1 to 1 of action axis input</returns>
    public float GetComposite1D(
        int bitShiftOffsetNegative,
        int bitShiftOffsetPositive
    ) {
        float value = 0;

        if (IsDown(bitShiftOffsetNegative)) value -= 1;
        if (IsDown(bitShiftOffsetPositive)) value += 1;

        return value;
    }

    /// <summary>
    /// Gets the composite activation value along a 2D axis with four actions
    /// </summary>
    /// <param name="bitShiftOffsetNegativeX">Bit shift offset of negative X action, acquired from <c>ActionBindingPreset.GetBindingBitShiftOffset</c></param>
    /// <param name="bitShiftOffsetNegativeY">Bit shift offset of negative Y action, acquired from <c>ActionBindingPreset.GetBindingBitShiftOffset</c></param>
    /// <param name="bitShiftOffsetPositiveX">Bit shift offset of positive X action, acquired from <c>ActionBindingPreset.GetBindingBitShiftOffset</c></param>
    /// <param name="bitShiftOffsetPositiveY">Bit shift offset of positive Y action, acquired from <c>ActionBindingPreset.GetBindingBitShiftOffset</c></param>
    /// <param name="normalize">Whether or not to normalize vector</param>
    /// <returns>Composite 2D vector value from -1 to 1 of action axis input</returns>
    public Vector2 GetComposite2D(
        int bitShiftOffsetNegativeX,
        int bitShiftOffsetNegativeY,
        int bitShiftOffsetPositiveX,
        int bitShiftOffsetPositiveY,
        bool normalize
    ) {
        Vector2 value = new(
            GetComposite1D(bitShiftOffsetNegativeX, bitShiftOffsetPositiveX),
            GetComposite1D(bitShiftOffsetNegativeY, bitShiftOffsetPositiveY)
        );

        if (normalize && value.LengthSquared() != 1) {
            // use precalculated values for speed & efficiency !
            value.X *= 0.707107f;
            value.Y *= 0.707107f;
        }

        return value;
    }

    /// <summary>
    /// Gets the composite activation value along a 3D axis with six actions
    /// </summary>
    /// <param name="bitShiftOffsetNegativeX">Bit shift offset of negative X action, acquired from <c>ActionBindingPreset.GetBindingBitShiftOffset</c></param>
    /// <param name="bitShiftOffsetNegativeY">Bit shift offset of negative Y action, acquired from <c>ActionBindingPreset.GetBindingBitShiftOffset</c></param>
    /// <param name="bitShiftOffsetNegativeZ">Bit shift offset of negative Z action, acquired from <c>ActionBindingPreset.GetBindingBitShiftOffset</c></param>
    /// <param name="bitShiftOffsetPositiveX">Bit shift offset of positive X action, acquired from <c>ActionBindingPreset.GetBindingBitShiftOffset</c></param>
    /// <param name="bitShiftOffsetPositiveY">Bit shift offset of positive Y action, acquired from <c>ActionBindingPreset.GetBindingBitShiftOffset</c></param>
    /// <param name="bitShiftOffsetPositiveZ">Bit shift offset of positive Z action, acquired from <c>ActionBindingPreset.GetBindingBitShiftOffset</c></param>
    /// <param name="normalize">Whether or not to normalize vector</param>
    /// <returns>Composite 3D vector value from -1 to 1 of action axis input</returns>
    public Vector3 GetComposite3D(
        int bitShiftOffsetNegativeX,
        int bitShiftOffsetNegativeY,
        int bitShiftOffsetNegativeZ,
        int bitShiftOffsetPositiveX,
        int bitShiftOffsetPositiveY,
        int bitShiftOffsetPositiveZ,
        bool normalize
    ) {
        Vector3 value = new(
            GetComposite1D(bitShiftOffsetNegativeX, bitShiftOffsetPositiveX),
            GetComposite1D(bitShiftOffsetNegativeY, bitShiftOffsetPositiveY),
            GetComposite1D(bitShiftOffsetNegativeZ, bitShiftOffsetPositiveZ)
        );

        if (normalize) {
            float lSqr = value.LengthSquared();
            if (lSqr == 2) {
                // if two actions are activated then do 2D normalization,
                // use precalculated values for speed & efficiency !
                value.X *= 0.707107f;
                value.Y *= 0.707107f;
                value.Z *= 0.707107f;
            } else if (lSqr == 3) {
                // if three actions are activated then do 3D normalization,
                // use precalculated values for speed & efficiency !
                value.X *= 0.577350f;
                value.Y *= 0.577350f;
                value.Z *= 0.577350f;
            }
        }

        return value;
    }
}
