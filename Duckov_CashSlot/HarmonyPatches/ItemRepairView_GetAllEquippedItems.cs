using System.Collections.Generic;
using System.Linq;
using Duckov.UI;
using HarmonyLib;
using ItemStatsSystem;

namespace Duckov_CashSlot.HarmonyPatches
{
    [HarmonyPatch(typeof(ItemRepairView), nameof(ItemRepairView.GetAllEquippedItems))]
    // ReSharper disable once InconsistentNaming
    internal class ItemRepairView_GetAllEquippedItems
    {
        // ReSharper disable once InconsistentNaming
        private static void Postfix(ref List<Item> __result)
        {
            var excludedItems = __result
                .Select(item => item.PluggedIntoSlot)
                .Where(slot => slot != null && SlotManager.IsRegisteredSlot(slot, false))
                .Select(slot => slot.Content)
                .ToArray();

            foreach (var excludedItem in excludedItems) __result.Remove(excludedItem);
        }
    }
}