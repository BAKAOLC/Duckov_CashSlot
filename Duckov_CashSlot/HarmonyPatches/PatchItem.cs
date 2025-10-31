using System.Reflection;
using HarmonyLib;
using ItemStatsSystem;

namespace Duckov_CashSlot.HarmonyPatches
{
    [HarmonyPatch]
    internal class PatchItem
    {
        private static MethodBase TargetMethod()
        {
            return AccessTools.PropertyGetter(typeof(Item), nameof(Item.TotalWeight));
        }

        // ReSharper disable InconsistentNaming
        private static void Postfix(Item __instance, ref float __result)
            // ReSharper restore InconsistentNaming
        {
            if (__instance == null) return;
            if (__instance.PluggedIntoSlot == null) return;
            if (!SlotManager.IsRegisteredSlot(__instance.PluggedIntoSlot)) return;
            if (!SlotManager.IsSlotForbidWeightCalculation(__instance.PluggedIntoSlot)) return;

            __result = 0;
        }
    }
}