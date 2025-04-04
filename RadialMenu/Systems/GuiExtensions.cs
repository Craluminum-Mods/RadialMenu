using Cairo;
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

    public static void DrawIcon(this RadialMenuButton button, ICoreClientAPI capi, Context context, ImageSurface surface, ElementBounds currentBounds)
    {
        if (!string.IsNullOrEmpty(button?.IconSvg) && button.IconSvg.EndsWith(".svg") && capi.Assets.TryGet(button.IconSvg) is IAsset asset)
        {
            int colorInt = ColorUtil.WhiteArgb;

            if (!string.IsNullOrEmpty(button.IconColor))
            {
                colorInt = ColorUtil.Hex2Int(button.IconColor);
            }

            capi.Gui.DrawSvg(
                svgAsset: asset,
                intoSurface: surface,
                posx: (int)(currentBounds.absPaddingX + GuiElement.scaled(4.0)),
                posy: (int)(currentBounds.absPaddingY + GuiElement.scaled(4.0)),
                width: (int)(currentBounds.InnerWidth - GuiElement.scaled(9.0)),
                height: (int)(currentBounds.InnerHeight - GuiElement.scaled(9.0)),
                color: colorInt);
            return;
        }

        double[] colorRgba = ColorUtil.WhiteArgbDouble;

        if (!string.IsNullOrEmpty(button?.IconColor))
        {
            colorRgba = ColorUtil.Hex2Doubles(button.IconColor);
        }

        capi.Gui.Icons.DrawIcon(
            context,
            button?.IconInternal ?? "plus",
            currentBounds.absPaddingX + GuiElement.scaled(4.0),
            currentBounds.absPaddingY + GuiElement.scaled(4.0),
            currentBounds.InnerWidth - GuiElement.scaled(9.0),
            currentBounds.InnerHeight - GuiElement.scaled(9.0),
            colorRgba);
    }

    public static void RenderIcon(this RadialMenuButton button, ICoreClientAPI capi, float deltaTime, ElementBounds currentBounds)
    {
        JsonItemStack? iconStack = button?.IconStack?.Clone();
        if (iconStack == null || !iconStack.Resolve(capi.World, ""))
        {
            return;
        }

        double posX = currentBounds.absX + (currentBounds.absPaddingX + GuiElement.scaled(40));
        double posY = currentBounds.absY + (currentBounds.absPaddingY + GuiElement.scaled(40));

        capi.Render.RenderItemstackToGui(
            inSlot: new DummySlot(iconStack.ResolvedItemstack),
            posX: posX,
            posY: posY,
            posZ: 100,
            size: (float)(GuiElement.scaled(currentBounds.OuterHeight) / 2),
            color: ColorUtil.WhiteArgb,
            showStackSize: false);
    }
}