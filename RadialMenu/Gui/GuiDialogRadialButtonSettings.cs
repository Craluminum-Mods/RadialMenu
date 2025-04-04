using Cairo;
using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using static RadialMenu.GuiExtensions;

namespace RadialMenu;

public class GuiDialogRadialButtonSettings : GuiDialog
{
    #nullable disable
    public RadialMenuButton CurrentButton = new();
    #nullable enable

    int curTab = 0;

    List<string> SvgIcons = new();
    List<int> Colors = new();

    protected static GuiTab[] Tabs => new[]
    {
        new GuiTab() { DataInt = 0, Name = Lang.Get("Name") },
        new GuiTab() { DataInt = 1, Name = Lang.Get("Icon") },
        new GuiTab() { DataInt = 2, Name = Lang.Get("waypoint-color") },
        new GuiTab() { DataInt = 3, Name = Lang.Get("Actions") }
    };

    protected static string[] hexcolors = new string[]
    {
        "#F9D0DC", "#F179AF", "#F15A4A", "#ED272A", "#A30A35", "#FFDE98", "#EFFD5F", "#F6EA5E", "#FDBB3A", "#C8772E", "#F47832",
        "C3D941", "#9FAB3A", "#94C948", "#47B749", "#366E4F", "#516D66", "93D7E3", "#7698CF", "#20909E", "#14A4DD", "#204EA2",
        "#28417A", "#C395C4", "#92479B", "#8E007E", "#5E3896", "D9D4CE", "#AFAAA8", "#706D64", "#4F4C2B", "#BF9C86", "#9885530", "#5D3D21", "#FFFFFF", "#080504"
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
        ClearComposers();
    }

    protected void ComposeDialog()
    {
        ClearComposers();
        PopulateIcons();
        PopulateColors();

        string[] actionNames = Enum.GetNames(typeof(EnumButtonAction)).Select(action => Lang.Get(action)).ToArray();
        string[] actionValues = Enum.GetValues<EnumButtonAction>().Select(action => action.ToString()).ToArray();

        double bgPadding = GuiElement.scaled(15);
        double height = GuiElement.scaled(30);
        double gap = GuiElement.scaled(10);
        double buttonWidth = GuiElement.scaled(80);
        double gridHeight = GuiElement.scaled(500);
        double gridWidth = GuiElement.scaled(500);
        int iconSize = GuiElement.scaledi(22);

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
                AddButtonPreview(gap, buttonWidth, ref leftBounds);

                SingleComposer.AddIconListPickerExtended(
                    icons: SvgIcons.ToArray(),
                    onToggle: OnIconSelected,
                    startBounds: BelowCopySet(ref leftBounds, fixedDeltaY: gap).WithFixedSize(iconSize + 5, iconSize + 5),
                    maxLineWidth: (int)gridWidth - iconSize,
                    key: "iconpicker");
            }
            #endregion

            #region Color
            if (curTab == 2)
            {
                AddButtonPreview(gap, buttonWidth, ref leftBounds);

                SingleComposer.AddColorListPicker(
                    colors: Colors.ToArray(),
                    onToggle: OnColorSelected,
                    startBounds: BelowCopySet(ref leftBounds, fixedDeltaY: gap).WithFixedSize(width: iconSize + 5, height: iconSize + 5),
                    maxLineWidth: (int)gridWidth - iconSize,
                    key: "colorpicker");
            }
            #endregion

            #region Actions
            if (curTab == 3)
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
            SingleComposer.ColorListPickerSetValue("colorpicker", 0);

            SingleComposer.GetDropDown("dropdown-actions")?.SetSelectedIndex((int)CurrentButton.Action);

            GuiElementTextArea textArea = SingleComposer.GetTextArea("commands");
            textArea?.LoadValue(textArea?.Lineize(string.Join("\r\n", CurrentButton.Commands)));
            textArea?.SetMaxHeight((int)gridHeight);
        }
        catch { }
    }

    private void PopulateIcons()
    {
        SvgIcons.Clear();
        SvgIcons.Add(string.Empty);

        if (!string.IsNullOrEmpty(CurrentButton.IconSvg))
        {
            SvgIcons.Add(CurrentButton.IconSvg);
        }

        List<IAsset> icons = capi.Assets.GetMany("textures/icons/", null, false);
        foreach (IAsset icon in icons)
        {
            string path = icon.Location;
            SvgIcons.Add(path);
        }
    }

    private void PopulateColors()
    {
        Colors.Clear();

        if (!string.IsNullOrEmpty(CurrentButton.IconColor))
        {
            int intRgba = ColorUtil.Hex2Int(CurrentButton.IconColor);
            string hexBgr = ColorUtil.Int2HexBGR(intRgba);
            int intBgr = ColorUtil.Hex2Int(hexBgr);
            Colors.Add(intBgr);
        }

        for (int i = 0; i < hexcolors.Length; i++)
        {
            Colors.Add(ColorUtil.Hex2Int(hexcolors[i]));
        }
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

    private void OnIconSelected(int selectedIndex)
    {
        if (SvgIcons.Count > selectedIndex && CurrentButton != null)
        {
            CurrentButton.IconSvg = SvgIcons[selectedIndex];
            SingleComposer.GetCustomDraw("drawicon")?.Redraw();
        }
    }

    private void OnColorSelected(int selectedIndex)
    {
        if (Colors.Count > selectedIndex && CurrentButton != null)
        {
            int intColor = Colors[selectedIndex];
            CurrentButton.IconColor = ColorUtil.Int2HexBGR(intColor);
            SingleComposer.GetCustomDraw("drawicon")?.Redraw();
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

    private void OnCommandCodeChanged(string text)
    {
        GuiElementTextArea textArea = SingleComposer.GetTextArea("commands");
        CurrentButton.Commands = textArea.GetLines();
        for (int i = 0; i < CurrentButton.Commands.Count; i++)
        {
            CurrentButton.Commands[i] = CurrentButton.Commands[i].TrimEnd('\n', '\r');
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

    private void AddButtonPreview(double gap, double buttonWidth, ref ElementBounds leftBounds)
    {
        SingleComposer.AddButton(string.Empty, onClick: () => true, BelowCopySet(ref leftBounds, fixedDeltaY: gap).WithFixedSize(buttonWidth, buttonWidth));

        if (CurrentButton.IconStack == null)
        {
            SingleComposer.AddDynamicCustomDraw(leftBounds.FlatCopy(), OnButtonIconDraw, key: "drawicon");
        }
        else
        {
            SingleComposer.AddCustomRender(leftBounds.FlatCopy(), OnButtonIconRender);
        }
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