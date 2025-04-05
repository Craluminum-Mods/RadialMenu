using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Config;
using Vintagestory.API.Util;
using Vintagestory.Client;
using Vintagestory.Client.NoObf;
using static RadialMenu.GuiExtensions;

namespace RadialMenu;

public class GuiModuleActionHotkey : IDisposable
{
    public GuiComposer SingleComposer => Dialog.SingleComposer;
    public readonly GuiDialogRadialButtonSettings Dialog;
    public ElementBounds LeftBounds;
    public ElementBounds RightBounds;

    private List<ConfigItem> hotkeys = new List<ConfigItem>();

    private string? currentSearchText;

    private Dictionary<HotkeyType, int> sortOrder = new()
    {
        { HotkeyType.MovementControls, 0 },
        { HotkeyType.MouseModifiers, 1 },
        { HotkeyType.CharacterControls, 2 },
        { HotkeyType.HelpAndOverlays, 3 },
        { HotkeyType.GUIOrOtherControls, 4 },
        { HotkeyType.InventoryHotkeys, 5 },
        { HotkeyType.CreativeOrSpectatorTool, 6 },
        { HotkeyType.CreativeTool, 7 },
        { HotkeyType.DevTool, 8 },
        { HotkeyType.MouseControls, 9 }
    };

    private string[] titles = new string[9]
    {
            Lang.Get("Movement controls"),
            Lang.Get("Mouse click modifiers"),
            Lang.Get("Actions"),
            Lang.Get("In-game Help and Overlays"),
            Lang.Get("User interface & More"),
            Lang.Get("Inventory hotkeys"),
            Lang.Get("Creative mode"),
            Lang.Get("Creative mode"),
            Lang.Get("Debug and Macros")
    };

    public GuiModuleActionHotkey(GuiDialogRadialButtonSettings guiDialog, ElementBounds leftBounds, ElementBounds rightBounds)
    {
        Dialog = guiDialog;
        LeftBounds = leftBounds;
        RightBounds = rightBounds;
    }

    public void Compose()
    {
        LoadKeyCombinations();

        List<ConfigItem> assignedHotkey = new List<ConfigItem>();
        string? assignedHotkeyCode = Dialog.CurrentButton.Hotkey;

        // display current hotkey
        if (!string.IsNullOrEmpty(assignedHotkeyCode) && Dialog.clientApi.Input.GetHotKeyByCode(assignedHotkeyCode) is HotKey hotkey)
        {
            string text = "?";
            if (hotkey.CurrentMapping != null)
            {
                text = hotkey.CurrentMapping.ToString();
            }

            ConfigItem item = new ConfigItem
            {
                Code = assignedHotkeyCode,
                Key = hotkey.Name,
                Value = text
            };

            assignedHotkey.Add(item);
        }

        double spacing = GuiElement.scaled(10);

        SingleComposer.AddConfigList(assignedHotkey, (_, _) => { }, CairoFont.WhiteSmallText().WithFontSize(18f), BelowCopySet(ref LeftBounds, fixedDeltaY: spacing), "assignedhotkey");
        if (!string.IsNullOrEmpty(Dialog.CurrentButton.Hotkey))
        {
            SingleComposer.AddInset(SingleComposer.LastAddedElementBounds.FlatCopy(), 3, 0.8f);
        }
        SingleComposer.AddTextInput(BelowCopySet(ref LeftBounds, fixedDeltaY: spacing), OnSearchItems, null, "searchField");

        ElementBounds hotkeyListBounds = SingleComposer.LastAddedElementBounds
            .CopyOnlySize()
            
            // reduce giant space between text input and config list
            .CopyOffsetedSibling(fixedDeltaY: -(SingleComposer.LastAddedElementBounds.fixedHeight * 6))
            
            .FixedUnder(SingleComposer.LastAddedElementBounds)
            .WithFixedHeight(GuiElement.scaled(400));

        ElementBounds insetBounds = hotkeyListBounds.ForkBoundingParent(5.0, 5.0, 5.0, 5.0);
        ElementBounds clipBounds = hotkeyListBounds.FlatCopy().WithParent(insetBounds);
        ElementBounds scrollbarBounds = ElementStdBounds.VerticalScrollbar(insetBounds);
        SingleComposer.AddVerticalScrollbar(OnNewScrollbarValue, scrollbarBounds.FixedUnder(LeftBounds, spacing), "scrollbar");
        SingleComposer.AddInset(insetBounds.FixedUnder(LeftBounds, spacing), 3, 0.8f);
        SingleComposer.BeginClip(clipBounds);
        SingleComposer.AddConfigList(hotkeys, OnKeyControlItemClick, CairoFont.WhiteSmallText().WithFontSize(18f), hotkeyListBounds, "hotkeylist");
        SingleComposer.EndClip();

        SingleComposer.EndChildElements().Compose();

        SingleComposer.GetTextInput("searchField").SetPlaceHolderText(Lang.Get("Search..."));
        SingleComposer.GetTextInput("searchField").SetValue("");

        GuiElementConfigList hotkeylist = SingleComposer.GetConfigList("hotkeylist");
        hotkeylist.errorFont = hotkeylist.stdFont.Clone();
        hotkeylist.errorFont.Color = GuiStyle.ErrorTextColor;
        hotkeylist.Bounds.CalcWorldBounds();
        clipBounds.CalcWorldBounds();
        ReLoadKeyCombinations();
        SingleComposer.GetScrollbar("scrollbar")?.SetHeights((float)clipBounds.fixedHeight, (float)hotkeylist.innerBounds.fixedHeight);
    }

    public void Dispose() { }

    private void OnSearchItems(string text)
    {
        if (!(currentSearchText == text))
        {
            currentSearchText = text;
            ReLoadKeyCombinations();
        }
    }

    private void ReLoadKeyCombinations()
    {
        LoadKeyCombinations();
        GuiElementConfigList hotkeylist = SingleComposer.GetConfigList("hotkeylist");
        if (hotkeylist != null)
        {
            hotkeylist.Refresh();
            SingleComposer.GetScrollbar("scrollbar")?.SetNewTotalHeight((float)hotkeylist.innerBounds.OuterHeight);
            SingleComposer.GetScrollbar("scrollbar")?.TriggerChanged();
        }
    }

    private void LoadKeyCombinations()
    {
        hotkeys.Clear();
        int i = 0;
        List<ConfigItem>[] sortedItems = new List<ConfigItem>[sortOrder.Count];
        for (int j = 0; j < sortedItems.Length; j++)
        {
            sortedItems[j] = new List<ConfigItem>();
        }
        foreach (KeyValuePair<string, HotKey> val in ScreenManager.hotkeyManager.HotKeys)
        {
            HotKey kc = val.Value;

            string text = "?";
            if (kc.CurrentMapping != null)
            {
                text = kc.CurrentMapping.ToString();
            }

            ConfigItem item = new ConfigItem
            {
                Code = val.Key,
                Key = kc.Name,
                Value = text,
                Data = i
            };

            int index = hotkeys.FindIndex((ConfigItem configitem) => configitem.Value == text);
            if (index != -1)
            {
                item.error = true;
                hotkeys[index].error = true;
            }
            sortedItems[sortOrder[kc.KeyCombinationType]].Add(item);
            i++;
        }
        for (int j = 0; j < sortedItems.Length; j++)
        {
            List<ConfigItem> filteredSortedItems = new List<ConfigItem>();
            string searchText = currentSearchText?.ToSearchFriendly().ToLowerInvariant() ?? "";
            bool canSearch = !string.IsNullOrEmpty(searchText);
            if ((j == 1 && !ClientSettings.SeparateCtrl) || j == 9)
            {
                continue;
            }
            if (canSearch)
            {
                foreach (ConfigItem item in sortedItems[j])
                {
                    if (item.Key.ToSearchFriendly().ToLowerInvariant().Contains(searchText))
                    {
                        filteredSortedItems.Add(item);
                    }
                }
                if (filteredSortedItems != null && !filteredSortedItems.Any())
                {
                    continue;
                }
            }

            filteredSortedItems ??= new();

            if (j != 7)
            {
                hotkeys.Add(new ConfigItem
                {
                    Type = EnumItemType.Title,
                    Key = titles[j]
                });
            }
            hotkeys.AddRange(canSearch ? filteredSortedItems : sortedItems[j]);
        }
    }

    private void OnNewScrollbarValue(float value)
    {
        ElementBounds innerBounds = SingleComposer.GetConfigList("hotkeylist").innerBounds;
        innerBounds.fixedY = 5f - value;
        innerBounds.CalcWorldBounds();
    }

    private void OnKeyControlItemClick(int index, int indexNoTitle)
    {
        int hotkeyIndex = (int)hotkeys[index].Data;
        SingleComposer.GetConfigList("hotkeylist").Refresh();
        SingleComposer.GetScrollbar("scrollbar")?.TriggerChanged();
        string code = ScreenManager.hotkeyManager.HotKeys.GetKeyAtIndex(hotkeyIndex);
        HotKey keyComb = ScreenManager.hotkeyManager.HotKeys[code].Clone();

        Dialog.CurrentButton.Hotkey = keyComb.Code;
        Dialog.OnGuiOpened(); // recompose
    }
}