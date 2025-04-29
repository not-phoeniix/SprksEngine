using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Embyr;

namespace Embyr.Data;

/// <summary>
/// Static helper class containing methods for saving/loading GameData objects
/// </summary>
public static class GameLoader {
    /// <summary>
    /// Saves a GameData object to a binary file
    /// </summary>
    /// <param name="data">GameData object to save</param>
    /// <param name="path">Path to save the savable to</param>
    public static void Save(IBinarySavable<object, object> savable, string saveName) {
        BinaryWriter writer = null;

        string path = Paths.MakeSavePath(saveName);

        try {
            FileStream stream = File.Open(path, FileMode.Create);
            writer = new BinaryWriter(stream, Encoding.UTF8);
            savable.WriteToBinary(writer);
        } catch (Exception ex) {
            Debug.WriteLine("ERROR in GameLoader saving: " + ex.Message);
        } finally {
            writer?.Close();
        }

        Debug.WriteLine($"Saved savable to file \"{path}\"!");
    }

    /// <summary>
    /// Loads a BinarySavable object given a save name
    /// </summary>
    /// <param name="saveName">Name of save to load game from, without dir or extension</param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static T Load<T>(string saveName) where T : IBinarySavable<T, object> {
        string path = Paths.MakeSavePath(saveName);

        if (!File.Exists(path)) {
            throw new Exception($"ERROR: Save file {path} not found!!");
        }

        BinaryReader reader = null;
        T data = default;

        try {
            FileStream stream = File.Open(path, FileMode.Open);
            reader = new BinaryReader(stream, Encoding.UTF8);
            data = (T)typeof(T).GetMethod("ReadFromBinary")?.Invoke(null, [reader]);
        } catch (Exception ex) {
            Debug.WriteLine("ERROR IN LOADING SAVE! Returned data will be empty!");
            Debug.WriteLine(ex.Message);
        } finally {
            reader?.Close();
        }

        return data;
    }

    /// <summary>
    /// Grabs names of all saves in save directory
    /// </summary>
    /// <returns>Alphabetically sorted list of strings of save names, without extension or directory</returns>
    public static List<string> GetSaves() {
        List<string> files = new();

        Directory.CreateDirectory(Paths.SaveDir);

        foreach (string file in Directory.EnumerateFiles(Paths.SaveDir)) {
            // remove preceeding directory
            string[] split = file.Split(Paths.DirSeparator);
            string fileName = split[split.Length - 1];

            // remove proceeding extension
            split = fileName.Split(".");
            fileName = split[0];
            string extension = split[split.Length - 1];

            if (extension == "gsav") {
                files.Add(fileName);
            }
        }

        files.Sort();

        return files;
    }
}
