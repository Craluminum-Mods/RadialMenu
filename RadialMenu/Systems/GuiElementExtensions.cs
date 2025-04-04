using System;
using Vintagestory.API.Client;

namespace RadialMenu;

public static class GuiElementExtensions
{
    public static GuiComposer AddIconListPickerExtended(this GuiComposer composer, string[] icons, Action<int> onToggle, ElementBounds startBounds, int maxLineWidth, string key = null)
    {
        return composer.AddElementListPicker(typeof(GuiElementIconListPickerExtended), icons, onToggle, startBounds, maxLineWidth, key);
    }

    public static GuiElementIconListPickerExtended GetIconListPickerExtended(this GuiComposer composer, string key)
    {
        return (GuiElementIconListPickerExtended)composer.GetElement(key);
    }

    public static void IconListPickerExtendedSetValue(this GuiComposer composer, string key, int selectedIndex)
    {
        int num = 0;
        GuiElementIconListPickerExtended iconListPicker;
        while ((iconListPicker = composer.GetIconListPickerExtended(key + "-" + num)) != null)
        {
            iconListPicker.SetValue(num == selectedIndex);
            num++;
        }
    }
}