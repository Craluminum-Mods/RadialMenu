using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Common;

namespace RadialMenu;

public class RadialMenuButton
{
    /// <summary>
    /// Button ID, determining its position and hierarchy within the radial menu.
    /// Example: "3" represents the 4th button in the main menu, "9/4" indicates the 5th button in a submenu of the 10th main button and so on.
    /// </summary>
    #nullable disable
    [JsonProperty]
    public string Id { get; set; }
    #nullable enable

    /// <summary>
    /// Button Name
    /// </summary>
    [JsonProperty]
    public string? Name { get; set; }

    /// <summary>
    /// The action this button performs when clicked. Defaults to <see cref="EnumButtonAction.None"/>.
    /// </summary>
    [JsonProperty]
    [JsonConverter(typeof(StringEnumConverter))]
    public EnumButtonAction Action { get; set; } = EnumButtonAction.None;

    /// <summary>
    /// The item used to render this button's icon. If null, <see cref="IconInternal"/> is used as fallback.
    /// </summary>
    [JsonProperty]
    public JsonItemStack? IconStack { get; set; }

    /// <summary>
    /// The color of this button's icon. White color by default.
    /// </summary>
    [JsonProperty]
    public string? IconColor { get; set; } = "#ffffff";

    /// <summary>
    /// A fallback internal icon string, used if <see cref="IconStack"/> is null.
    /// </summary>
    [JsonProperty]
    public string? IconInternal { get; set; }

    /// <summary>
    /// Optional file path to a custom SVG icon. Used if neither <see cref="IconStack"/> nor <see cref="IconInternal"/> are set.
    /// </summary>
    [JsonProperty]
    public string? IconSvg { get; set; }

    /// <summary>
    /// The hotkey to invoke when <see cref="Action"/> is set to <see cref="EnumButtonAction.Hotkey"/>. Ignored otherwise.
    /// </summary>
    [JsonProperty]
    public string? Hotkey { get; set; }

    /// <summary>
    /// A list of commands to execute when <see cref="Action"/> is set to <see cref="EnumButtonAction.Commands"/>. Ignored otherwise.
    /// </summary>
    [JsonProperty]
    public List<string> Commands { get; set; } = new();

    public RadialMenuButton Clone()
    {
        return new RadialMenuButton()
        {
            Id = Id,
            Name = Name,
            Action = Action,
            IconStack = IconStack?.Clone(),
            IconColor = IconColor,
            IconInternal = IconInternal,
            IconSvg = IconSvg,
            Hotkey = Hotkey,
            Commands = Commands.ToList()
        };
    }
}