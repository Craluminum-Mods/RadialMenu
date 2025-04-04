using Cairo;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace RadialMenu;

public class GuiElementIconListPickerExtended : GuiElementElementListPickerBase<string>
{
    public GuiElementIconListPickerExtended(ICoreClientAPI capi, string elem, ElementBounds bounds) : base(capi, elem, bounds)
    {
    }

    public override void DrawElement(string iconPath, Context ctx, ImageSurface surface)
    {
        ctx.SetSourceRGBA(1.0, 1.0, 1.0, 0.2);
        RoundRectangle(ctx, Bounds.drawX, Bounds.drawY, Bounds.InnerWidth, Bounds.InnerHeight, 1.0);
        ctx.Fill();

        if (iconPath != null && api.Assets.TryGet(iconPath) is IAsset asset)
        {
            int color = ColorUtil.FromRGBADoubles(new double[4] { 1.0, 1.0, 1.0, 1.0 });
            api.Gui.DrawSvg(
                svgAsset: asset,
                intoSurface: surface,
                posx: (int)(Bounds.drawX + 2.0),
                posy: (int)(Bounds.drawY + 2.0),
                width: (int)(Bounds.InnerWidth - 4.0),
                height: (int)(Bounds.InnerHeight - 4.0),
                color: color);
        }
    }
}