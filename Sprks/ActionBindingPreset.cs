using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Xna.Framework.Input;

namespace Sprks;

/// <summary>
/// Defines a possible input mouse button click for an action bind
/// </summary>
public enum MouseClick {
    Left,
    Middle,
    Right,
    Extended1,
    Extended2
}

/// <summary>
/// A preset of action-to-button bindings used in input system
/// </summary>
public class ActionBindingPreset {
    #region // Static members

    internal static readonly string UILeftAction = "ui_left";
    internal static readonly string UIRightAction = "ui_right";
    internal static readonly string UIUpAction = "ui_up";
    internal static readonly string UIDownAction = "ui_down";
    internal static readonly string UISubmitAction = "ui_submit";
    internal static readonly string UIBackAction = "ui_back";

    /// <summary>
    /// Creates the default binding JSON
    /// </summary>
    /// <returns>A new JsonObject that contains the data for a default binding preset</returns>
    public static JsonObject GetDefaultBindingJson() {
        return new JsonObject() {
            ["name"] = "default",
            ["binds"] = new JsonObject() {
                [UIUpAction] = new JsonArray() {
                    "key_up",
                    "button_dpadup"
                },
                [UIDownAction] = new JsonArray() {
                    "key_down",
                    "button_dpaddown"
                },
                [UILeftAction] = new JsonArray() {
                    "key_left",
                    "button_dpadleft"
                },
                [UIRightAction] = new JsonArray() {
                    "key_right",
                    "button_dpadright"
                },
                [UISubmitAction] = new JsonArray() {
                    "key_enter",
                    "button_a"
                },
                [UIBackAction] = new JsonArray() {
                    "key_escape",
                    "button_b"
                }
            }
        };
    }

    /// <summary>
    /// Makes a new action binding preset from the default binds
    /// </summary>
    /// <param name="name">Name of preset</param>
    /// <returns>A new ActionBindingPreset instance filled with default binds</returns>
    public static ActionBindingPreset MakeDefault(string name = "default") {
        ActionBindingPreset preset = new(GetDefaultBindingJson()) {
            Name = name
        };
        return preset;
    }

    #endregion

    /// <summary>
    /// Action binding that associates a string name to a set of keys, buttons, and mouse clicks
    /// </summary>
    /// <param name="name"></param>
    internal class Binding(string name) {
        /// <summary>
        /// Gets the name of this binding
        /// </summary>
        public string Name { get; } = name;

        /// <summary>
        /// Gets the list of keys this binding has
        /// </summary>
        public List<Keys> KeyBinds { get; } = new();

        /// <summary>
        /// Gets the list of buttons this binding has
        /// </summary>
        public List<Buttons> ButtonBinds { get; } = new();

        /// <summary>
        /// Gets the list of mouse clicks this binding has
        /// </summary>
        public List<MouseClick> MouseBinds { get; } = new();

        /// <summary>
        /// Creates a new Binding object using existing JSON bind data
        /// </summary>
        /// <param name="name">Name of binding</param>
        /// <param name="binds">JSON bind data to create from</param>
        public Binding(string name, JsonArray? binds) : this(name) {
            if (binds == null) return;

            foreach (JsonNode? n in binds) {
                // don't parse data if the node's value isn't a string
                if (n?.GetValueKind() != JsonValueKind.String) continue;

                // split a bind string into its type and value
                //   "button_a" -> ["button", "a"]
                //   "key_space" -> ["key", "space"]
                string[]? vSplit = n?.GetValue<string>()?.Split('_');

                // don't try parsing and skip to next if split failed
                if (vSplit == null || vSplit.Length < 2) continue;

                // change what enum type is parsed based
                //   on type prefix in split
                switch (vSplit[0].ToLower()) {
                    case "button":
                        if (
                            Enum.TryParse(vSplit[1], true, out Buttons b) &&
                            !ButtonBinds.Contains(b)
                        ) {
                            ButtonBinds.Add(b);
                        }
                        break;

                    case "key":
                        if (
                            Enum.TryParse(vSplit[1], true, out Keys k) &&
                            !KeyBinds.Contains(k)
                        ) {
                            KeyBinds.Add(k);
                        }
                        break;

                    case "mouse":
                        if (
                            Enum.TryParse(vSplit[1], true, out MouseClick m) &&
                            !MouseBinds.Contains(m)
                        ) {
                            MouseBinds.Add(m);
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Converts this binding to a JSON array representation
        /// </summary>
        /// <returns></returns>
        public JsonArray ToJsonArr() {
            JsonArray arr = new();

            foreach (Keys k in KeyBinds) {
                arr.Add($"key_{k}");
            }

            foreach (Buttons b in ButtonBinds) {
                arr.Add($"button_{b}");
            }

            foreach (MouseClick m in MouseBinds) {
                arr.Add($"mouse_{m}");
            }

            return arr;
        }
    }

    /// <summary>
    /// Dictionary of bindings associated with this preset
    /// </summary>
    internal readonly Dictionary<string, (int, Binding)> Bindings;

    /// <summary>
    /// Name of this binding preset
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Creates a new ActionBindingPreset instance
    /// </summary>
    /// <param name="name">Name of preset to create</param>
    public ActionBindingPreset(string name) {
        this.Name = name;
        this.Bindings = new Dictionary<string, (int, Binding)>();
    }

    /// <summary>
    /// Creates a new ActionBindingPreset instance
    /// </summary>
    /// <param name="json">Existing json data to create binds from</param>
    /// <exception cref="Exception">When inputted JSON object does not have a name property</exception>
    public ActionBindingPreset(JsonObject json) {
        if (!json.TryGetPropertyValue("name", out JsonNode nameNode)) {
            throw new Exception("Name for ActionBindingPreset could not be found in inputted JSON - cannot create preset!");
        } else {
            this.Name = nameNode.GetValue<string>();
        }

        if ((json["binds"] as JsonObject) == null) {
            throw new Exception("Binds not found in ActionBindingPreset json data - cannot create preset!");
        }

        Bindings = new Dictionary<string, (int, Binding)>();
        int bitShiftOffset = 0;
        foreach (KeyValuePair<string, JsonNode?> pair in json["binds"] as JsonObject) {
            // key/value should be binding NAME and binding DATA
            Binding bind = new(pair.Key, pair.Value as JsonArray);
            Bindings.Add(pair.Key, (bitShiftOffset, bind));

            bitShiftOffset++;
        }
    }

    /// <summary>
    /// Adds an action/button bind to this preset
    /// </summary>
    /// <param name="name">Name of bind to add to</param>
    /// <param name="key">Key to bind action to</param>
    public void AddActionBind(string name, Keys key) {
        Binding binding;
        if (!Bindings.TryGetValue(name, out (int, Binding) value)) {
            binding = new Binding(name);
            int offset = Bindings.Count;
            Bindings[name] = (offset, binding);
        } else {
            binding = value.Item2;
        }

        if (!binding.KeyBinds.Contains(key)) {
            binding.KeyBinds.Add(key);
        }
    }

    /// <summary>
    /// Adds an action/button bind to this preset
    /// </summary>
    /// <param name="name">Name of bind to add to</param>
    /// <param name="mouse">Mouse click to bind action too</param>
    public void AddActionBind(string name, MouseClick mouse) {
        Binding binding;
        if (!Bindings.TryGetValue(name, out (int, Binding) value)) {
            binding = new Binding(name);
            int offset = Bindings.Count;
            Bindings[name] = (offset, binding);
        } else {
            binding = value.Item2;
        }

        if (!binding.MouseBinds.Contains(mouse)) {
            binding.MouseBinds.Add(mouse);
        }
    }

    /// <summary>
    /// Adds an action/button bind to this preset
    /// </summary>
    /// <param name="name">Name of bind to add to</param>
    /// <param name="button">Button to bind action to</param>
    public void AddActionBind(string name, Buttons button) {
        Binding binding;
        if (!Bindings.TryGetValue(name, out (int, Binding) value)) {
            binding = new Binding(name);
            int offset = Bindings.Count;
            Bindings[name] = (offset, binding);
        } else {
            binding = value.Item2;
        }

        if (!binding.ButtonBinds.Contains(button)) {
            binding.ButtonBinds.Add(button);
        }
    }


    // TODO: make the ability to remove action binds ...
    //   this will mean we have to reorganize shift offsets, might be a pain in the ass later
    // /// <summary>
    // /// Removes an action bind from this preset along with all key bindings associated with it
    // /// </summary>
    // /// <param name="name">Name of bind to remove</param>
    // /// <returns>True if successfully removed, false if otherwise</returns>
    // public bool RemoveActionBind(string name) {
    //     if (bindings.Remove(name)) {
    //         // if binding was removed, we need to reorganize the shift offsets

    //         return true;
    //     }

    //     return false;
    // }

    /// <summary>
    /// Fully clears all action bindings from this preset
    /// </summary>
    public void ClearActionBinds() {
        Bindings.Clear();
    }

    /// <summary>
    /// Gets the JSON string associated with this binding preset
    /// </summary>
    /// <returns>JSON string of all bindings</returns>
    public string GetJsonString() {
        JsonObject binds = new();
        JsonObject json = new() {
            ["name"] = Name,
            ["binds"] = binds
        };

        foreach ((int, Binding) value in Bindings.Values) {
            binds[value.Item2.Name] = value.Item2.ToJsonArr();
        }

        return json.ToJsonString(new JsonSerializerOptions() {
            WriteIndented = true
        });
    }

    /// <summary>
    /// Gets the bit shift offset of a binding saved in this preset
    /// </summary>
    /// <param name="name">Name of binding to get offset for</param>
    /// <returns>Offset of stored bind, -1 if bind wasn't found</returns>
    internal int GetBindingBitShiftOffset(string name) {
        if (Bindings.TryGetValue(name, out (int, Binding) value)) {
            return value.Item1;
        }

        return -1;
    }
}
