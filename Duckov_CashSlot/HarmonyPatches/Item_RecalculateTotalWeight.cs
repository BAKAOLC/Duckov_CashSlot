using System.Reflection;
using HarmonyLib;
using ItemStatsSystem;
using UnityEngine;

namespace Duckov_CashSlot.HarmonyPatches
{
    [HarmonyPatch(typeof(Item), nameof(Item.RecalculateTotalWeight))]
    // ReSharper disable once InconsistentNaming
    internal class Item_RecalculateTotalWeight
    {
        private static FieldInfo? _cachedTotalWeightField;

        // ReSharper disable InconsistentNaming
        private static void Postfix(Item __instance, ref float __result)
            // ReSharper restore InconsistentNaming
        {
            if (__instance == null) return;
            if (__instance.Slots == null) return;

            var removeWeight = 0f;
            foreach (var slot in __instance.Slots)
            {
                if (slot == null || slot.Content == null) continue;
                if (!SlotManager.IsRegisteredSlot(slot)) continue;
                if (!SlotManager.IsSlotForbidWeightCalculation(slot)) continue;

                removeWeight += slot.Content.TotalWeight;
            }

            if (removeWeight <= 0f) return;

            __result = Mathf.Max(0f, __result - removeWeight);

            _cachedTotalWeightField ??= AccessTools.Field(typeof(Item), "_cachedTotalWeight");
            _cachedTotalWeightField?.SetValue(__instance, __result);
        }
    }
}