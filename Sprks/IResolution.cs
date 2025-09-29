namespace Sprks;

/// <summary>
/// Interface that describes an object that has a screen resolution
/// </summary>
public interface IResolution {
    /// <summary>
    /// Changes the resolution of this object
    /// </summary>
    /// <param name="width">Resolution width (in pixels)</param>
    /// <param name="height">Resolution height (in pixels)</param>
    public void ChangeResolution(int width, int height);
}
