using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Xna.Framework.Input;

namespace Embyr;

/// <summary>
/// Enumeration of all possible input actions that can be binded w/ controller or keyboard
/// </summary>
public enum InputAction {
    // use binary so i can bitwise AND function my way to victory :]

    Left = 1 << 0,
    Right = 1 << 1,
    Up = 1 << 2,
    Down = 1 << 3,
    Jump = 1 << 4,
    ToggleFly = 1 << 5,

    ItemUse = 1 << 6,
    ItemDrop = 1 << 7,
    Flashlight = 1 << 8,
    Inventory = 1 << 9,
    HotbarSlotOne = 1 << 10,
    HotbarSlotTwo = 1 << 11,
    HotbarSlotThree = 1 << 12,
    HotbarSlotFour = 1 << 13,

    Pause = 1 << 14,
    Submit = 1 << 15,
    Back = 1 << 16,
    UILeft = 1 << 17,
    UIRight = 1 << 18,
    UIUp = 1 << 19,
    UIDown = 1 << 20,
}

/// <summary>
/// A binding preset defined from a loaded JSON object
/// </summary>
public class ActionBindingPreset {
    #region // Static members

    private static readonly JsonObject DefaultBindings = new() {
        ["Name"] = "Default",
        ["KeyboardMouse"] = new JsonObject() {
            [nameof(InputAction.Up)] = nameof(Keys.W),
            [nameof(InputAction.Left)] = nameof(Keys.A),
            [nameof(InputAction.Down)] = nameof(Keys.S),
            [nameof(InputAction.Right)] = nameof(Keys.D),
            [nameof(InputAction.Jump)] = nameof(Keys.Space),
            [nameof(InputAction.ToggleFly)] = nameof(Keys.T),

            [nameof(InputAction.ItemUse)] = "LeftMouse",
            [nameof(InputAction.ItemDrop)] = nameof(Keys.Q),
            [nameof(InputAction.Flashlight)] = nameof(Keys.F),
            [nameof(InputAction.Inventory)] = nameof(Keys.Tab),
            [nameof(InputAction.HotbarSlotOne)] = nameof(Keys.D1),
            [nameof(InputAction.HotbarSlotTwo)] = nameof(Keys.D2),
            [nameof(InputAction.HotbarSlotThree)] = nameof(Keys.D3),
            [nameof(InputAction.HotbarSlotFour)] = nameof(Keys.D4),

            [nameof(InputAction.Pause)] = nameof(Keys.Escape),
            [nameof(InputAction.Submit)] = nameof(Keys.Enter),
            [nameof(InputAction.Back)] = nameof(Keys.Escape),
            [nameof(InputAction.UIUp)] = nameof(Keys.Up),
            [nameof(InputAction.UILeft)] = nameof(Keys.Left),
            [nameof(InputAction.UIDown)] = nameof(Keys.Down),
            [nameof(InputAction.UIRight)] = nameof(Keys.Right)
        },
        ["Gamepad"] = new JsonObject() {
            [nameof(InputAction.Up)] = nameof(Buttons.LeftThumbstickUp),
            [nameof(InputAction.Left)] = nameof(Buttons.LeftThumbstickLeft),
            [nameof(InputAction.Down)] = nameof(Buttons.LeftThumbstickDown),
            [nameof(InputAction.Right)] = nameof(Buttons.LeftThumbstickRight),
            [nameof(InputAction.Jump)] = nameof(Buttons.LeftTrigger),
            [nameof(InputAction.ToggleFly)] = "Unassigned",

            [nameof(InputAction.ItemUse)] = nameof(Buttons.RightTrigger),
            [nameof(InputAction.ItemDrop)] = nameof(Buttons.B),
            [nameof(InputAction.Flashlight)] = nameof(Buttons.RightStick),
            [nameof(InputAction.Inventory)] = nameof(Buttons.X),
            [nameof(InputAction.HotbarSlotOne)] = nameof(Buttons.DPadUp),
            [nameof(InputAction.HotbarSlotTwo)] = nameof(Buttons.DPadLeft),
            [nameof(InputAction.HotbarSlotThree)] = nameof(Buttons.DPadDown),
            [nameof(InputAction.HotbarSlotFour)] = nameof(Buttons.DPadRight),

            [nameof(InputAction.Pause)] = nameof(Buttons.Start),
            [nameof(InputAction.Submit)] = nameof(Buttons.A),
            [nameof(InputAction.Back)] = nameof(Buttons.B),
            [nameof(InputAction.UIUp)] = nameof(Buttons.DPadUp),
            [nameof(InputAction.UILeft)] = nameof(Buttons.DPadLeft),
            [nameof(InputAction.UIDown)] = nameof(Buttons.DPadDown),
            [nameof(InputAction.UIRight)] = nameof(Buttons.DPadRight)
        }
    };

    /// <summary>
    /// Static default binding preset, readonly
    /// </summary>
    public static readonly ActionBindingPreset Default = new();

    #endregion

    // actual map data itself lol
    private readonly Dictionary<InputAction, string> keyboardMouseMaps;
    private readonly Dictionary<InputAction, string> gamepadMaps;

    // persistent json data so that binding can be converted to JSON easily :]
    private readonly JsonObject persistentJsonObj;
    private readonly JsonSerializerOptions jsonOptions = new() { WriteIndented = true };

    /// <summary>
    /// Name of this binding preset
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Creates a new ActionBindingPreset
    /// </summary>
    /// <param name="json">Json object to load preset data from</param>
    public ActionBindingPreset(JsonObject json) {
        persistentJsonObj = (JsonObject)DefaultBindings.DeepClone();

        if (json == null) {
            json = DefaultBindings;
        }

        //* binding name

        // print out name of binding
        if (!json.TryGetPropertyValue("Name", out JsonNode name)) {
            Debug.WriteLine("ERROR: Binding name not found!");
            Name = "ERROR";
        } else {
            string nameValue = name.GetValue<string>();
            Name = nameValue;
        }

        persistentJsonObj["Name"] = Name;

        //* keyboard & mouse bindings

        // grab node to search keyboard bindings for
        if (!json.TryGetPropertyValue("KeyboardMouse", out JsonNode kbNode)) {
            Debug.WriteLine("ERROR: No KeyboardMouse input settings found! Falling back to defaults!");
            kbNode = DefaultBindings["KeyboardMouse"];
        }

        // fill keyboard mouse dictionary with values of the specified binding node
        keyboardMouseMaps = new Dictionary<InputAction, string>();
        foreach (InputAction action in Enum.GetValues<InputAction>()) {
            string bind = kbNode[action.ToString()]?.GetValue<string>();

            // set bind to default if it doesn't exist in inputted node
            if (bind == null) {
                bind = DefaultBindings["KeyboardMouse"][action.ToString()].GetValue<string>();
            }

            keyboardMouseMaps[action] = bind;

            // create object if it doesn't exist yet and set value in persistent obj
            persistentJsonObj["KeyboardMouse"] ??= new JsonObject();
            persistentJsonObj["KeyboardMouse"][action.ToString()] = bind;
        }

        //* gamepad bindings

        // grab node to search gamepad bindings for
        if (!json.TryGetPropertyValue("Gamepad", out JsonNode gpNode)) {
            Debug.WriteLine("ERROR: No Gamepad input settings found! Falling back to defaults!");
            gpNode = DefaultBindings["Gamepad"];
        }

        // fill gamepad dictionary with values of the specified binding node
        gamepadMaps = new Dictionary<InputAction, string>();
        foreach (InputAction action in Enum.GetValues<InputAction>()) {
            string map = gpNode[action.ToString()]?.GetValue<string>();

            // set bind to default if it doesn't exist in inputted node
            if (map == null) {
                map = DefaultBindings["Gamepad"][action.ToString()].GetValue<string>();
            }

            gamepadMaps[action] = map;

            // create object if it doesn't exist yet and set value in persistent obj
            persistentJsonObj["Gamepad"] ??= new JsonObject();
            persistentJsonObj["Gamepad"][action.ToString()] = map;
        }
    }

    /// <summary>
    /// Creates a new ActionBindingGroup, default values
    /// </summary>
    public ActionBindingPreset() : this(null) { }

    /// <summary>
    /// Gets the string of a keyboard or mouse bind, can be parsed to
    /// a Microsoft.Xna.Framework.Input.Keys object
    /// </summary>
    /// <param name="action">Action to grab binding from</param>
    /// <returns>String of bind, null if bind doesn't exist</returns>
    public string GetKeyboardMouse(InputAction action) {
        if (!keyboardMouseMaps.TryGetValue(action, out string output)) {
            return null;
        }

        return output;
    }

    /// <summary>
    /// Gets the string of a gamepad bind, can be parsed to
    /// a Microsoft.Xna.Framework.Input.Buttons object
    /// </summary>
    /// <param name="action">Action to grab binding from</param>
    /// <returns>String of bind, null if bind doesn't exist</returns>
    public string GetGamePad(InputAction action) {
        if (!gamepadMaps.TryGetValue(action, out string output)) {
            return null;
        }

        return output;
    }

    /// <summary>
    /// Gets the JSON string associated with this binding preset
    /// </summary>
    /// <returns>JSON string of all bindings</returns>
    public string GetJson() {
        return persistentJsonObj.ToJsonString(jsonOptions);
    }
}
