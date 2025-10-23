using System.Collections.Generic;
using Duckov.Utilities;
using HarmonyLib;
using ItemStatsSystem;
using ItemStatsSystem.Items;

namespace Duckov_CashSlot.HarmonyPatches
{
    [HarmonyPatch]
    // ReSharper disable once InconsistentNaming
    internal static class Slot_Plug
    {
        private static readonly List<Item> ModifiedItems = [];

        [HarmonyPatch(typeof(Slot), nameof(Slot.Plug))]
        [HarmonyPostfix]
        // ReSharper disable once InconsistentNaming
        private static void Plug_Postfix(Slot __instance, ref Item otherItem)
        {
            if (otherItem == null) return;
            if (otherItem.Tags.Contains(GameplayDataSettings.Tags.DontDropOnDeadInSlot)) return;

            var isSlotForbidDeathDrop = SlotManager.IsSlotForbidDeathDrop(__instance);
            if (!isSlotForbidDeathDrop) return;

            otherItem.Tags.Add(GameplayDataSettings.Tags.DontDropOnDeadInSlot);
            ModifiedItems.Add(otherItem);
        }

        [HarmonyPatch(typeof(Slot), nameof(Slot.Unplug))]
        // ReSharper disable once InconsistentNaming
        private static void Unplug_Postfix(ref Item __result)
        {
            if (__result == null) return;
            if (!ModifiedItems.Contains(__result)) return;

            __result.Tags.Remove(GameplayDataSettings.Tags.DontDropOnDeadInSlot);
            ModifiedItems.Remove(__result);
        }
    }
}