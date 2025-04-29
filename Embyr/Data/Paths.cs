using System;
using System.Diagnostics;

namespace Embyr.Data;

/// <summary>
/// Static class that stores common paths used around the project for persistent save data
/// </summary>
public static class Paths {
    private static string baseDataDir;
    private static string dirSeparator;
    private static bool initialized = false;

    /// <summary>
    /// Gets the base directory location local to this system
    /// that all other configuration and saves and game data
    /// is stored within
    /// </summary>
    public static string BaseDataDir {
        get {
            if (!initialized) {
                throw new Exception("Cannot access paths data, paths manager never initialized!");
            }

            return baseDataDir;
        }
    }

    /// <summary>
    /// Gets the directory separator local to this particular
    /// platform, either "/" or "\"
    /// </summary>
    public static string DirSeparator {
        get {
            if (!initialized) {
                throw new Exception("Cannot access paths data, paths manager never initialized!");
            }

            return dirSeparator;
        }
    }

    /// <summary>
    /// Gets the directory that save data is stored in
    /// </summary>
    public static string SaveDir => BaseDataDir + DirSeparator + "saves";

    /// <summary>
    /// Gets the directory that screenshots are stored in
    /// </summary>
    public static string ScreenshotsDir => BaseDataDir + DirSeparator + "screenshots";

    /// <summary>
    /// Gets the path to the config file that all settings are stored in
    /// </summary>
    public static string ConfigFilePath => BaseDataDir + DirSeparator + "config.json";

    /// <summary>
    /// Gets the path to the file that stores input maps
    /// </summary>
    public static string InputFilePath => BaseDataDir + DirSeparator + "input.json";

    /// <summary>
    /// Initialzes paths manager
    /// </summary>
    /// <param name="folderName">Name of game config folder</param>
    public static void Init(string folderName) {
        Debug.WriteLine("Initializing system paths...");

        if (string.IsNullOrWhiteSpace(folderName)) {
            throw new Exception("Paths config folder name cannot be an invalid string (null, empty, or whitespace)...");
        }

        switch (Environment.OSVersion.Platform) {
            case PlatformID.Win32NT:
                Debug.WriteLine("System on windows...");

                baseDataDir =
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) +
                    "\\My Games\\" +
                    folderName;

                dirSeparator = "\\";

                break;

            case PlatformID.Unix:
                Debug.WriteLine("System on unix...");

                baseDataDir =
                    Environment.GetEnvironmentVariable("HOME") +
                    "/.local/share/" +
                    folderName;

                dirSeparator = "/";

                break;

            case PlatformID.Other:
                Debug.WriteLine("ERROR: System on anything else, not recognized!");
                break;
        }

        initialized = true;
        Debug.WriteLine("System paths initialized!");
    }

    /// <summary>
    /// Turns a save name into a full system filepath with extension
    /// </summary>
    /// <param name="saveName">Name of save to concatenate, no extension</param>
    /// <returns>Full path in system to this hypothetical save, with extension</returns>
    public static string MakeSavePath(string saveName) {
        return SaveDir + DirSeparator + saveName + ".gsav";
    }
}
