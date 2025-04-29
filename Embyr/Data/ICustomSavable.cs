using Embyr.Scenes;

namespace Embyr.Data;

/// <summary>
/// Extension for actors used for custom saving of data
/// </summary>
public interface ICustomSavable : IActor {
    /// <summary>
    /// Method used for custom writing of save data on a per-actor basis
    /// </summary>
    /// <returns>String representation of custom data</returns>
    public string MakeSaveData();

    /// <summary>
    /// Method used for custom reading of save data on a per-actor basis
    /// </summary>
    /// <param name="data">Data string that has been previously saved</param>
    public void LoadSaveData(string data);
}
