using Cairo;
using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace RadialMenu;

public static class GuiExtensions
{
    public static ElementBounds BelowCopySet(ref ElementBounds bounds, double fixedDeltaX = 0.0, double fixedDeltaY = 0.0, double fixedDeltaWidth = 0.0, double fixedDeltaHeight = 0.0)
    {
        return bounds = bounds.BelowCopy(fixedDeltaX, fixedDeltaY, fixedDeltaWidth, fixedDeltaHeight);
    }

    /// <summary>
    /// Helper method for creating radial menu buttons
    /// </summary>
    public static ElementBounds GetButtonBounds(EnumDialogArea? withAlighment = null, double offsetX = 0, double offsetY = 0)
    {
        double size = GuiElement.scaled(80);

        if (withAlighment != null)
        {
            return ElementBounds.FixedSize(size, size).WithAlignment((EnumDialogArea)withAlighment).WithFixedAlignmentOffset(offsetX, offsetY);
        }
        return ElementBounds.FixedSize(size, size).WithFixedAlignmentOffset(offsetX, offsetY);
    }

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

    public static void DrawIcon(this RadialMenuButton button, ICoreClientAPI capi, Context context, ImageSurface surface, ElementBounds currentBounds)
    {
        if (!string.IsNullOrEmpty(button?.IconPath))
        {
            if (capi.Assets.TryGet(button.IconPath) is IAsset asset)
            {
                int color = ColorUtil.FromRGBADoubles(new double[4] { 1.0, 1.0, 1.0, 1.0 });
                capi.Gui.DrawSvg(
                    svgAsset: asset,
                    intoSurface: surface,
                    posx: (int)(currentBounds.absPaddingX + GuiElement.scaled(4.0)),
                    posy: (int)(currentBounds.absPaddingY + GuiElement.scaled(4.0)),
                    width: (int)(currentBounds.InnerWidth - GuiElement.scaled(9.0)),
                    height: (int)(currentBounds.InnerHeight - GuiElement.scaled(9.0)),
                    color: color);
            }
        }
        else if (button == null || button.IconPath == null || button.IconString == null)
        {
            capi.Gui.Icons.DrawIcon(
                context,
                button?.IconString ?? "plus",
                currentBounds.absPaddingX + GuiElement.scaled(4.0),
                currentBounds.absPaddingY + GuiElement.scaled(4.0),
                currentBounds.InnerWidth - GuiElement.scaled(9.0),
                currentBounds.InnerHeight - GuiElement.scaled(9.0),
                ColorUtil.WhiteArgbDouble);
        }
    }

    public static void RenderIcon(this RadialMenuButton button, ICoreClientAPI capi, float deltaTime, ElementBounds currentBounds)
    {
        if (button == null || button.IconStack == null || !button.IconStack.Resolve(capi.World, ""))
        {
            return;
        }

        double posX = currentBounds.absX + (currentBounds.absPaddingX + GuiElement.scaled(40));
        double posY = currentBounds.absY + (currentBounds.absPaddingY + GuiElement.scaled(40));

        capi.Render.RenderItemstackToGui(
            inSlot: new DummySlot(button.IconStack.ResolvedItemstack),
            posX: posX,
            posY: posY,
            posZ: 100,
            size: (float)(GuiElement.scaled(currentBounds.OuterHeight) / 2),
            color: ColorUtil.WhiteArgb,
            showStackSize: false);
    }
}