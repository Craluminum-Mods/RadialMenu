using System.Collections.Generic;
using Vintagestory.API.Client;

namespace RadialMenu;

public static class InputExtensions
{
    public static bool DidPressNumber(this KeyEvent args, ICoreClientAPI capi, bool allowCharacterControls, out int? number)
    {
        number = null;

        Dictionary<int, HotKey> hotbarHotkeys = new()
        {
            [0] = capi.Input.GetHotKeyByCode($"hotbarslot1"),
            [1] = capi.Input.GetHotKeyByCode($"hotbarslot2"),
            [2] = capi.Input.GetHotKeyByCode($"hotbarslot3"),
            [3] = capi.Input.GetHotKeyByCode($"hotbarslot4"),
            [4] = capi.Input.GetHotKeyByCode($"hotbarslot5"),
            [5] = capi.Input.GetHotKeyByCode($"hotbarslot6"),
            [6] = capi.Input.GetHotKeyByCode($"hotbarslot7"),
            [7] = capi.Input.GetHotKeyByCode($"hotbarslot8"),
            [8] = capi.Input.GetHotKeyByCode($"hotbarslot9"),
            [9] = capi.Input.GetHotKeyByCode($"hotbarslot10"),
        };

        foreach ((int num, HotKey hotkey) in hotbarHotkeys)
        {
            if (hotkey != null && hotkey.DidPress(args, capi.World, capi.World.Player, allowCharacterControls: true))
            {
                number = num;
                return true;
            }
        }
        return false;
    }
}