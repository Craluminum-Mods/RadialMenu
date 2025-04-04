namespace RadialMenu;

/// <summary>
/// Defines the action a radial menu button performs.
/// </summary>
public enum EnumButtonAction
{
    /// <summary>
    /// No action set.
    /// </summary>
    None,

    /// <summary>
    /// Opens a submenu of buttons.
    /// </summary>
    Submenu,

    /// <summary>
    /// Invokes a hotkey.
    /// </summary>
    Hotkey,

    /// <summary>
    /// Executes a list of commands.
    /// </summary>
    Commands
}