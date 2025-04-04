using Cairo;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Config;

namespace RadialMenu;

public class GuiDialogRadialMenu : GuiDialog
{
    /// <summary>
    /// Button submenu id containing current buttons
    /// </summary>
    protected string SubId;

    public override string ToggleKeyCombinationCode => "radialmenu:radialmenu";

    public GuiDialogRadialMenu(ICoreClientAPI capi) : base(capi) { }

    public override void OnGuiOpened()
    {
        ComposeDialog();
    }

    public override void OnGuiClosed()
    {
        SubId = null;
        ClearComposers();
    }

    public override void OnKeyDown(KeyEvent args)
    {
        HotKey hotKeyByCode = capi.Input.GetHotKeyByCode(ToggleKeyCombinationCode);
        if (hotKeyByCode != null && hotKeyByCode.DidPress(args, capi.World, capi.World.Player, allowCharacterControls: true) && IsOpened())
        {
            args.Handled = true;
            return;
        }

        base.OnKeyDown(args);
    }

    public override void OnKeyUp(KeyEvent args)
    {
        HotKey hotKeyByCode = capi.Input.GetHotKeyByCode(ToggleKeyCombinationCode);
        if (hotKeyByCode != null && hotKeyByCode.DidPress(args, capi.World, capi.World.Player, allowCharacterControls: true))
        {
            args.Handled = true;
            TryClose();
            return;
        }

        base.OnKeyUp(args);
    }

    protected void ComposeDialog()
    {
        ClearComposers();

        double minOffY = GuiElement.scaled(50);
        double middleOffY = GuiElement.scaled(75);
        double maxOffY = GuiElement.scaled(100);

        Dictionary<string, ElementBounds> buttons = new()
        {
            [$"{SubId}/0"] = GuiExtensions.GetButtonBounds(EnumDialogArea.CenterTop),
            [$"{SubId}/1"] = GuiExtensions.GetButtonBounds(EnumDialogArea.RightTop, -maxOffY, minOffY),
            [$"{SubId}/2"] = GuiExtensions.GetButtonBounds(EnumDialogArea.RightMiddle, 0, -middleOffY),
            [$"{SubId}/3"] = GuiExtensions.GetButtonBounds(EnumDialogArea.RightMiddle, 0, middleOffY),
            [$"{SubId}/4"] = GuiExtensions.GetButtonBounds(EnumDialogArea.RightBottom, -maxOffY, -minOffY),
            [$"{SubId}/5"] = GuiExtensions.GetButtonBounds(EnumDialogArea.CenterBottom),
            [$"{SubId}/6"] = GuiExtensions.GetButtonBounds(EnumDialogArea.LeftBottom, maxOffY, -minOffY),
            [$"{SubId}/7"] = GuiExtensions.GetButtonBounds(EnumDialogArea.LeftMiddle, 0, middleOffY),
            [$"{SubId}/8"] = GuiExtensions.GetButtonBounds(EnumDialogArea.LeftMiddle, 0, -middleOffY),
            [$"{SubId}/9"] = GuiExtensions.GetButtonBounds(EnumDialogArea.LeftTop, maxOffY, minOffY)
        };

        ElementBounds mainBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.CenterMiddle);
        ElementBounds childBounds = ElementBounds.FixedSize(GuiElement.scaled(500), GuiElement.scaled(500));

        SingleComposer = capi.Gui.CreateCompo("radialmenu:radialmenu", mainBounds);
        SingleComposer.BeginChildElements(childBounds);

        foreach ((string buttonId, ElementBounds buttonBounds) in buttons)
        {
            RadialMenuButton button = Core.GetButton(capi, buttonId);

            SingleComposer.AddButton(string.Empty, () => OnButtonClick(buttonId), buttonBounds.FlatCopy(), key: "button-" + buttonId);

            string buttonName = button?.Name ?? buttonId;
            string hoverText = Lang.Get("radialmenu:radialbutton-tooltip", buttonName);

            SingleComposer.AddAutoSizeHoverText(hoverText, CairoFont.WhiteSmallText(), 250, buttonBounds.FlatCopy());

            if (button != null && button.IconStack != null)
            {
                SingleComposer.AddCustomRender(buttonBounds.FlatCopy(), (dt, cb) => OnButtonIconRender(buttonId, dt, cb));
            }
            else
            {
                SingleComposer.AddDynamicCustomDraw(buttonBounds.FlatCopy(), (c, s, cb) => OnButtonIconDraw(buttonId, c, s, cb));
            }
        }

        SingleComposer.EndChildElements().Compose();
    }

    private void OnButtonIconDraw(string id, Context context, ImageSurface surface, ElementBounds currentBounds)
    {
        RadialMenuButton button = Core.GetButton(capi, id);
        button.DrawIcon(capi, context, surface, currentBounds);
    }

    private void OnButtonIconRender(string id, float deltaTime, ElementBounds currentBounds)
    {
        RadialMenuButton button = Core.GetButton(capi, id);
        button.RenderIcon(capi, deltaTime, currentBounds);
    }

    private bool OnButtonClick(string id)
    {
        RadialMenuButton button = Core.GetButton(capi, id);

        if (capi.World.Player.Entity.Controls.ShiftKey)
        {
            GuiDialogRadialButtonSettings buttonSettings = new GuiDialogRadialButtonSettings(capi);
            buttonSettings.CurrentButton = button?.Clone();
            buttonSettings.CurrentButton ??= new RadialMenuButton() { Id = id };
            buttonSettings.TryOpen();
            TryClose();
            return true;
        }

        if (button == null)
        {
            return false;
        }

        switch (button.Action)
        {
            case EnumButtonAction.None:
                break;
            case EnumButtonAction.Submenu:
                SubId = id;
                ComposeDialog();
                break;
            case EnumButtonAction.Hotkey:
                if (button.Hotkey != null && capi.Input.GetHotKeyByCode(button.Hotkey) is HotKey hotkey && hotkey != null)
                {
                    hotkey.Handler.Invoke(hotkey.CurrentMapping);
                }
                break;
            case EnumButtonAction.Commands:
                button.Commands.ForEach(cmd => capi.SendChatMessage(cmd));
                break;
        }
        return true;
    }
}