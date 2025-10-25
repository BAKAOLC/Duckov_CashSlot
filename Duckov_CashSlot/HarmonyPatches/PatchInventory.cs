using System;
using System.Collections.Generic;
using HarmonyLib;
using ItemStatsSystem;

namespace Duckov_CashSlot.HarmonyPatches
{
    [HarmonyPatch]
    internal class PatchInventory
    {
        [HarmonyPatch(typeof(Inventory), nameof(Inventory.FindAll))]
        [HarmonyPostfix]
        // ReSharper disable InconsistentNaming
        private static void Inventory_FindAll_Postfix(Inventory __instance, List<Item> __result, Predicate<Item> match)
        // ReSharper restore InconsistentNaming
        {
            var item = __instance.AttachedToItem;
            if (item == null) return;

            List<Item> itemInSlots = [];
            foreach (var slot in item.Slots)
            {
                if (slot == null || slot.Content == null) continue;
                if (!match(slot.Content)) continue;
                itemInSlots.Add(slot.Content);
            }
            
            __result.InsertRange(0, itemInSlots);
        }
    }
}