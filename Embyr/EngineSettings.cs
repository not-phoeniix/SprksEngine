using Microsoft.Xna.Framework;

namespace Embyr;

public static class EngineSettings {
    private static Point gameCanvasResolution = new(480, 270);
    private static Point gameWindowResolution = new(1280, 720);
    private static bool enableVSync = true;
    private static bool isFullscreen = false;
    private static bool isBorderless = false;

    public static Point GameCanvasResolution {
        get => gameCanvasResolution;
        set {
            gameCanvasResolution = value;
            ShouldApplyGraphicsChanges = true;
        }
    }

    public static Point GameWindowResolution {
        get => gameWindowResolution;
        set {
            gameWindowResolution = value;
            ShouldApplyGraphicsChanges = true;
        }
    }

    public static bool EnableVSync {
        get => enableVSync;
        set {
            enableVSync = value;
            ShouldApplyGraphicsChanges = true;
        }
    }

    public static bool IsFullscreen {
        get => isFullscreen;
        set {
            isFullscreen = value;
            ShouldApplyGraphicsChanges = true;
        }
    }

    public static bool IsBorderless {
        get => isBorderless;
        set {
            isBorderless = value;
            ShouldApplyGraphicsChanges = true;
        }
    }

    public static float SimulationDistance { get; set; } = 1000;

    public static ActionBindingPreset CurrentBindingPreset { get; set; }

    public static bool ShowDebugDrawing { get; set; } = false;

    public static Color RenderClearColor { get; set; } = Palette.Col2;

    /// <summary>
    /// Toggles the value of <c>EngineSettings.ShowDebugDrawing</c>
    /// </summary>
    public static void ToggleDebugDrawing() {
        ShowDebugDrawing = !ShowDebugDrawing;
    }

    internal static bool ShouldApplyGraphicsChanges { get; set; } = false;
}
