﻿using Cairo;
using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using static RadialMenu.GuiExtensions;

namespace RadialMenu;

public class GuiDialogRadialButtonSettings : GuiDialog
{
    #nullable disable
    public RadialMenuButton CurrentButton = new();
    #nullable enable

    int curTab = 0;

    #region Iconpicker
    List<string> IconPaths = new();
    #endregion

    protected static GuiTab[] Tabs => new[]
    {
        new GuiTab() { DataInt = 0, Name = Lang.Get("Name") },
        new GuiTab() { DataInt = 1, Name = Lang.Get("Icon") },
        new GuiTab() { DataInt = 2, Name = Lang.Get("Actions") }
    };

    public override string? ToggleKeyCombinationCode => null;

    public GuiDialogRadialButtonSettings(ICoreClientAPI capi) : base(capi)
    {
        capi.World.RegisterGameTickListener(Tick500ms, 500);
    }

    private void Tick500ms(float deltaTime)
    {
        //if (CurrentButton == null) return;
        //ComposeDialog();
    }

    public override void OnGuiOpened()
    {
        ComposeDialog();
    }

    public override void OnGuiClosed()
    {
        CurrentButton = null;
        ClearComposers();
    }

    protected void ComposeDialog()
    {
        ClearComposers();
        PopulateIcons();

        string[] actionNames = Enum.GetNames(typeof(EnumButtonAction)).Select(action => Lang.Get(action)).ToArray();
        string[] actionValues = Enum.GetValues<EnumButtonAction>().Select(action => action.ToString()).ToArray();

        double bgPadding = GuiElement.scaled(15);
        double height = GuiElement.scaled(30);
        double gap = GuiElement.scaled(10);
        double buttonWidth = GuiElement.scaled(80);
        double gridHeight = GuiElement.scaled(500);
        double gridWidth = GuiElement.scaled(500);
        int colorIconSize = 22;

        CairoFont titleFont = CairoFont.WhiteMediumText();
        CairoFont textFont = CairoFont.WhiteSmallText();

        ElementBounds mainBounds = ElementStdBounds.AutosizedMainDialog
        .WithAlignment(EnumDialogArea.CenterTop)
        .WithFixedAlignmentOffset(GuiStyle.DialogToScreenPadding, GuiStyle.DialogToScreenPadding);

        ElementBounds childBounds = new ElementBounds().WithSizing(ElementSizing.FitToChildren);
        ElementBounds backgroundBounds = childBounds.WithFixedPadding(bgPadding);

        ElementBounds leftBounds = ElementBounds.FixedSize(gridWidth, height).WithFixedOffset(0, height);
        ElementBounds rightBounds = leftBounds.RightCopy(gap);

        try
        {
            SingleComposer = capi.Gui.CreateCompo("radialmenu:radialbutton-settings", mainBounds);
            SingleComposer.AddShadedDialogBG(backgroundBounds);
            SingleComposer.AddDialogTitleBar(Lang.Get("radialmenu:radialbutton-settings", CurrentButton.Id), OnTitleBarClose);
            SingleComposer.BeginChildElements(childBounds);

            SingleComposer.AddHorizontalTabs(Tabs, leftBounds.FlatCopy(), OnTabChanged, CairoFont.SmallButtonText(), CairoFont.SmallButtonText(), "tabs");
            SingleComposer.GetHorizontalTabs("tabs").activeElement = curTab;

            SingleComposer.AddButton(Lang.Get("general-save"), OnSave, rightBounds.FlatCopy().WithFixedSize(buttonWidth, height), CairoFont.ButtonText(), key: "save");
            SingleComposer.AddButton(Lang.Get("general-delete"), OnDelete, BelowCopySet(ref rightBounds, fixedDeltaY: gap).WithFixedSize(buttonWidth, height), CairoFont.ButtonText(), key: "delete");
            #region Name
            if (curTab == 0)
            {
                SingleComposer.AddTextInput(BelowCopySet(ref leftBounds, fixedDeltaY: gap), OnNameChanged, key: "button-name");
            }
            #endregion

            #region Icon
            if (curTab == 1)
            {
                SingleComposer.AddButton(string.Empty, onClick: () => true, BelowCopySet(ref leftBounds, fixedDeltaY: gap).WithFixedSize(buttonWidth, buttonWidth));

                if (CurrentButton.IconStack == null)
                {
                    SingleComposer.AddDynamicCustomDraw(leftBounds.FlatCopy(), OnButtonIconDraw);
                }
                else
                {
                    SingleComposer.AddCustomRender(leftBounds.FlatCopy(), OnButtonIconRender);
                }

                SingleComposer.AddIconListPickerExtended(IconPaths.ToArray(), OnIconSelected, BelowCopySet(ref leftBounds, fixedDeltaY: gap).WithFixedSize(colorIconSize + 5, colorIconSize + 5), (int)gridWidth - colorIconSize, "iconpicker");
            }
            #endregion

            #region Actions
            if (curTab == 2)
            {
                SingleComposer.AddDropDown(actionValues, actionNames, 0, OnActionChanged, BelowCopySet(ref leftBounds, fixedDeltaY: gap), "dropdown-actions");

                switch (CurrentButton.Action)
                {
                    case EnumButtonAction.None:
                        break;

                    case EnumButtonAction.Submenu:
                        break;

                    case EnumButtonAction.Hotkey:
                        break;

                    case EnumButtonAction.Commands:
                        SingleComposer.AddTextArea(BelowCopySet(ref leftBounds, fixedDeltaY: gap).WithFixedHeight(gridHeight), OnCommandCodeChanged, textFont, "commands");
                        break;
                }
            }
            #endregion

            SingleComposer.EndChildElements().Compose();

            SingleComposer.GetTextInput("button-name")?.SetValue(CurrentButton.Name ?? "");
            SingleComposer.GetTextInput("button-name")?.SetPlaceHolderText("...");
            SingleComposer.IconListPickerExtendedSetValue("iconpicker", 0);

            SingleComposer.GetDropDown("dropdown-actions")?.SetSelectedIndex((int)CurrentButton.Action);

            GuiElementTextArea textArea = SingleComposer.GetTextArea("commands");
            textArea?.LoadValue(textArea?.Lineize(string.Join("\r\n", CurrentButton.Commands)));
            textArea?.SetMaxHeight((int)gridHeight);
        }
        catch { }
    }

    private void PopulateIcons()
    {
        IconPaths.Clear();
        IconPaths.Add("");
        if (!string.IsNullOrEmpty(CurrentButton.IconPath))
        {
            IconPaths.Add(CurrentButton.IconPath);
        }

        List<IAsset> icons = capi.Assets.GetMany("textures/icons/", null, false);
        foreach (IAsset icon in icons)
        {
            string path = icon.Location;
            IconPaths.Add(path);
        }
    }

    private void OnIconSelected(int selectedIndex)
    {
        if (IconPaths.Count > selectedIndex && CurrentButton != null)
        {
            CurrentButton.IconPath = IconPaths[selectedIndex];
            SingleComposer?.ReCompose();
        }
    }

    private void OnCommandCodeChanged(string text)
    {
        GuiElementTextArea textArea = SingleComposer.GetTextArea("commands");
        CurrentButton.Commands = textArea.GetLines();
        for (int i = 0; i < CurrentButton.Commands.Count; i++)
        {
            CurrentButton.Commands[i] = CurrentButton.Commands[i].TrimEnd('\n', '\r');
        }
    }

    private void OnActionChanged(string code, bool selected)
    {
        if (Enum.TryParse(code, out EnumButtonAction newAction))
        {
            CurrentButton.Action = newAction;
            ComposeDialog();
        }
    }

    private bool OnSave()
    {
        Core.SetButton(capi, CurrentButton);
        TryClose();
        return true;
    }

    private bool OnDelete()
    {
        Core.RemoveButton(capi, CurrentButton.Id);
        TryClose();
        return true;
    }

    private void OnTabChanged(int tabIndex)
    {
        curTab = tabIndex;
        ComposeDialog();
    }

    private void OnNameChanged(string text)
    {
        CurrentButton.Name = text;
    }

    private void OnButtonIconDraw(Context context, ImageSurface surface, ElementBounds currentBounds)
    {
        CurrentButton.DrawIcon(capi, context, surface, currentBounds);
    }

    private void OnButtonIconRender(float deltaTime, ElementBounds currentBounds)
    {
        CurrentButton.RenderIcon(capi, deltaTime, currentBounds);
    }

    private void OnTitleBarClose() => TryClose();
}