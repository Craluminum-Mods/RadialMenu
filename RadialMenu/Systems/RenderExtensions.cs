using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace RadialMenu;

public static class RenderExtensions
{
    public static void RenderItemstackToGui(this ItemStack stack, ElementBounds bounds, ICoreClientAPI capi, bool showStackSize = false)
    {
        double posX = bounds.absPaddingX + GuiElement.scaled(4.0);
        double posY = bounds.absPaddingY + GuiElement.scaled(4.0);

        capi.Render.RenderItemstackToGui(
            inSlot: new DummySlot(stack),
            posX: posX,
            posY: posY,
            posZ: 100,
            size: (float)(bounds.InnerWidth - GuiElement.scaled(9.0)),
            color: ColorUtil.WhiteArgb,
            showStackSize: showStackSize);
    }

    public static RenderSkillItemDelegate RenderItemStack(this ItemStack stack, ICoreClientAPI capi, bool showStackSize = false)
    {
        return (AssetLocation code, float dt, double posX, double posY) =>
        {
            double size = GuiElementPassiveItemSlot.unscaledSlotSize + GuiElementItemSlotGridBase.unscaledSlotPadding;
            double scsize = GuiElement.scaled(size - 5);

            capi.Render.RenderItemstackToGui(
                new DummySlot(stack),
                posX + (scsize / 2),
                posY + (scsize / 2),
                100,
                (float)GuiElement.scaled(GuiElementPassiveItemSlot.unscaledItemSize),
                ColorUtil.WhiteArgb,
                showStackSize: showStackSize);
        };
    }
}