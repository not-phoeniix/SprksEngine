using Microsoft.Xna.Framework;

namespace Sprks;

/// <summary>
/// Static settings class that apply to the engine/game being run
/// </summary>
public static class EngineSettings {
    private static Point gameCanvasResolution = new(480, 270);
    private static Point gameWindowResolution = new(1280, 720);
    private static bool enableVSync = true;
    private static bool isFullscreen = false;
    private static bool isBorderless = false;

    /// <summary>
    /// Gets/sets the resolution of the game canvas
    /// </summary>
    public static Point GameCanvasResolution {
        get => gameCanvasResolution;
        set {
            if (gameCanvasResolution != value) {
                ShouldApplyGraphicsChanges = true;
            }

            gameCanvasResolution = value;
        }
    }

    /// <summary>
    /// Gets/sets the resolution of the game window
    /// </summary>
    public static Point GameWindowResolution {
        get => gameWindowResolution;
        set {
            if (gameWindowResolution != value) {
                ShouldApplyGraphicsChanges = true;
            }

            gameWindowResolution = value;
        }
    }

    /// <summary>
    /// Gets/sets whether or not to enable vsync
    /// </summary>
    public static bool EnableVSync {
        get => enableVSync;
        set {
            if (enableVSync != value) {
                ShouldApplyGraphicsChanges = true;
            }

            enableVSync = value;
        }
    }

    /// <summary>
    /// Gets/sets whether or not the window is fullscreen
    /// </summary>
    public static bool IsFullscreen {
        get => isFullscreen;
        set {
            if (isFullscreen != value) {
                ShouldApplyGraphicsChanges = true;
            }

            isFullscreen = value;
        }
    }

    /// <summary>
    /// Gets/sets whether or not the window is borderless
    /// </summary>
    public static bool IsBorderless {
        get => isBorderless;
        set {
            if (isBorderless != value) {
                ShouldApplyGraphicsChanges = true;
            }

            isBorderless = value;
        }
    }

    /// <summary>
    /// Gets/sets the simulation distance for running actor updates
    /// </summary>
    public static float SimulationDistance { get; set; } = 1000;

    /// <summary>
    /// Gets/sets the currently used binding preset for the game
    /// </summary>
    public static ActionBindingPreset CurrentBindingPreset { get; set; }

    /// <summary>
    /// Gets/sets whether or not to show debug drawing information
    /// </summary>
    public static bool ShowDebugDrawing { get; set; } = false;

    /// <summary>
    /// Gets/sets whether or not to show the normals buffer for debugging
    /// </summary>
    public static bool ShowDebugNormalBuffer { get; set; } = false;

    /// <summary>
    /// Gets/sets whether or not to show the depth buffer for debugging
    /// </summary>
    public static bool ShowDebugDepthBuffer { get; set; } = false;

    /// <summary>
    /// Gets/sets the gamma
    /// </summary>
    public static float Gamma { get; set; } = 2.2f;

    /// <summary>
    /// Gets/sets the title of the game window
    /// </summary>
    public static string WindowTitle { get; set; } = "Sprks Project";

    /// <summary>
    /// Toggles the value of <c>EngineSettings.ShowDebugDrawing</c>
    /// </summary>
    public static void ToggleDebugDrawing() {
        ShowDebugDrawing = !ShowDebugDrawing;
    }

    /// <summary>
    /// Gets/sets whether or not graphics changes should be applied this frame
    /// </summary>
    internal static bool ShouldApplyGraphicsChanges { get; set; } = false;
}
