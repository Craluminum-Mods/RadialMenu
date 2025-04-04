using Newtonsoft.Json;
using System.Collections.Generic;
using Vintagestory.API.Common;

namespace RadialMenu.Configuration;

public class ConfigRadialMenu : IModConfig
{
    public const string ConfigName = "RadialMenu-Client.json";

    [JsonProperty]
    public List<RadialMenuButton> Buttons { get; set; } = new();

    public ConfigRadialMenu(ICoreAPI api, ConfigRadialMenu previousConfig = null)
    {
        if (previousConfig != null)
        {
            Buttons.Clear();
            Buttons = previousConfig.Buttons;
        }
    }
}