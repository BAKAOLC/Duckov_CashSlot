using Duckov.Economy;
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
        [HarmonyPatch(typeof(Slot), nameof(Slot.Plug))]
        [HarmonyPostfix]
        private static void Plug_Postfix(ref Item otherItem)
        {
            if (otherItem == null) return;
            if (otherItem.TypeID != EconomyManager.CashItemID) return;
            if (otherItem.Tags.Contains(GameplayDataSettings.Tags.DontDropOnDeadInSlot)) return;

            otherItem.Tags.Add(GameplayDataSettings.Tags.DontDropOnDeadInSlot);
        }

        [HarmonyPatch(typeof(Slot), nameof(Slot.Unplug))]
        // ReSharper disable once InconsistentNaming
        private static void Unplug_Postfix(ref Item __result)
        {
            if (__result == null) return;
            if (__result.TypeID != EconomyManager.CashItemID) return;
            if (!__result.Tags.Contains(GameplayDataSettings.Tags.DontDropOnDeadInSlot)) return;

            __result.Tags.Remove(GameplayDataSettings.Tags.DontDropOnDeadInSlot);
        }
    }
}