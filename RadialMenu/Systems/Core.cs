using RadialMenu.Configuration;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace RadialMenu;

public class Core : ModSystem
{
    #nullable disable
    public ConfigRadialMenu Config { get; private set; }
    #nullable enable

    public static Core GetInstance(ICoreAPI api) => api.ModLoader.GetModSystem<Core>();

    public override bool ShouldLoad(EnumAppSide forSide) => forSide.IsClient();

    public override bool ShouldLoad(ICoreAPI api) => api.Side.IsClient();

    public override void StartPre(ICoreAPI api)
    {
        Config = ModConfig.ReadConfig<ConfigRadialMenu>(api, ConfigRadialMenu.ConfigName);
    }

    public override void StartClientSide(ICoreClientAPI capi)
    {
        capi.Input.RegisterHotKeyFirst("radialmenu:radialmenu", Lang.Get("radialmenu:hotkey-radialmenu"), GlKeys.R, HotkeyType.GUIOrOtherControls);
        capi.Gui.RegisterDialog(new GuiDialogRadialMenu(capi));

        Mod.Logger.Event("started '{0}' mod", Mod.Info.Name);
    }

    public static List<RadialMenuButton> GetButtons(ICoreClientAPI capi)
    {
        return GetInstance(capi).Config.Buttons;
    }

    public static RadialMenuButton GetButton(ICoreClientAPI capi, string buttonId)
    {
        return GetButtons(capi).Find(x => x.Id == buttonId) ?? new RadialMenuButton() { Id = buttonId };
    }

    public static void SetButton(ICoreClientAPI capi, RadialMenuButton newButton)
    {
        RemoveButton(capi, newButton.Id);
        GetInstance(capi).Config.Buttons.Add(newButton);

        ModConfig.WriteConfig(capi, ConfigRadialMenu.ConfigName, GetInstance(capi).Config);
        GetInstance(capi).Config = ModConfig.ReadConfig<ConfigRadialMenu>(capi, ConfigRadialMenu.ConfigName);
    }

    public static void RemoveButton(ICoreClientAPI capi, string buttonId)
    {
        GetInstance(capi).Config.Buttons.RemoveAll(x => x.Id == buttonId);

        ModConfig.WriteConfig(capi, ConfigRadialMenu.ConfigName, GetInstance(capi).Config);
        GetInstance(capi).Config = ModConfig.ReadConfig<ConfigRadialMenu>(capi, ConfigRadialMenu.ConfigName);
    }
}