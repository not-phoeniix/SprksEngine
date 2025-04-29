namespace Embyr.UI;

/// <summary>
/// Interface
/// </summary>
public interface IMenuInteractable {
    /// <summary>
    /// Whether or not this object is hovered/selected with mouse/controller
    /// </summary>
    public bool Hovered { get; set; }

    /// <summary>
    /// Whether or not this interactable's interaction is enabled
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Thing to execute when "activated" (enter key, click, or select button)
    /// </summary>
    public void Activate();

    /// <summary>
    /// Updates state of this interactable in regards to mouse movement,
    /// updating the "selected" state to when hovered by mouse
    /// </summary>
    /// <param name="preventHover">Whether or not to prevent updating hovering for this interactable (will always reset to false if so)</param>
    public void UpdateMouseHover(bool preventHover = false);

    /// <summary>
    /// Handles controller input for this menu element when focused
    /// </summary>
    /// <param name="mouseMode">Reference to the private mouse mode boolean to change by accepting controller input</param>
    /// <returns>True if menu element should capture input and prevent scrolling, false if not</returns>
    public virtual bool HandleControllerInput(ref bool mouseMode) {
        return false;
    }
}
