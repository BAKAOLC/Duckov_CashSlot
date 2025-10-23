using HarmonyLib;
using ItemStatsSystem;
using ItemStatsSystem.Items;

namespace Duckov_CashSlot.HarmonyPatches
{
    [HarmonyPatch]
    // ReSharper disable once InconsistentNaming
    internal static class PatchSlot
    {
        [HarmonyPatch(typeof(Slot), nameof(Slot.Plug))]
        [HarmonyPostfix]
        // ReSharper disable once InconsistentNaming
        private static void Plug_Postfix(Slot __instance, ref Item otherItem)
        {
            var isSlotForbidDeathDrop = SlotManager.IsSlotForbidDeathDrop(__instance);
            if (!isSlotForbidDeathDrop) return;

            TagManager.SetDontDropOnDeadTag(otherItem);
        }

        [HarmonyPatch(typeof(Slot), nameof(Slot.Unplug))]
        [HarmonyPostfix]
        // ReSharper disable once InconsistentNaming
        private static void Unplug_Postfix(ref Item __result)
        {
            if (__result == null) return;

            TagManager.CheckRemoveDontDropOnDeadTag(__result);
        }
    }
}