namespace Embyr;

/// <summary>
/// Interface that contains method for changing resolution
/// </summary>
public interface IResolution {
    /// <summary>
    /// Changes the resolution of this object
    /// </summary>
    /// <param name="width">Resolution width (in pixels)</param>
    /// <param name="height">Resolution height (in pixels)</param>
    /// <param name="canvasExpandSize">Number of pixels to expand bounds for scroll smoothing</param>
    public void ChangeResolution(int width, int height, int canvasExpandSize);
}
