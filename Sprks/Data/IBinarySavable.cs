using System.IO;

namespace Sprks.Data;

/// <summary>
/// Interface that contains methods for reading/writing binary game data
/// </summary>
/// <typeparam name="T">Type of data object that implements interface</typeparam>
/// <typeparam name="U">Type of object that T's data is based on</typeparam>
public interface IBinarySavable<T, U> {
    /// <summary>
    /// Writes data to a binary file with an already-open stream
    /// </summary>
    /// <param name="writer">Writer to write binary with</param>
    public void WriteToBinary(BinaryWriter writer);

    /// <summary>
    /// Reads data from a binary file with an already-open stream
    /// </summary>
    /// <param name="reader">Reader to read binary with</param>
    /// <returns>New object created from file data</returns>
    public static abstract T ReadFromBinary(BinaryReader reader);

    /// <summary>
    /// Creates a new data object from an existing object in memory
    /// </summary>
    /// <param name="item">Item to grab data from</param>
    /// <returns>New object created from data in existing item</returns>
    public static abstract T FromExisting(U item);
}

// public interface IBinarySavable
