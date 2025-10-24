using System.Collections.Generic;
using Duckov_CashSlot.Data;
using Duckov.UI;
using HarmonyLib;
using ItemStatsSystem;
using ItemStatsSystem.Items;

namespace Duckov_CashSlot.HarmonyPatches
{
    [HarmonyPatch(typeof(ItemSlotCollectionDisplay), nameof(ItemSlotCollectionDisplay.Setup))]
    // ReSharper disable once InconsistentNaming
    internal class ItemSlotCollectionDisplay_Setup
    {
        internal static ShowIn CurrentShowIn = ShowIn.Character;
        private static readonly Dictionary<Item, Slot[]> CachedSlots = [];

        private static void Prefix(ref Item target)
        {
            if (target == null || target.Slots == null) return;

            CachedSlots[target] = [..target.Slots];
            RemoveInvisibleSlots(target, CurrentShowIn);
        }

        private static void RemoveInvisibleSlots(Item target, ShowIn showIn)
        {
            target.Slots.list.RemoveAll(slot => SlotManager.GetSlotShowIn(slot) != showIn);
        }

        // ReSharper disable once InconsistentNaming
        private static void Postfix(ref Item target)
        {
            if (target == null || target.Slots == null) return;
            if (!CachedSlots.TryGetValue(target, out var cachedSlots)) return;

            // restore original slots
            target.Slots.Clear();
            target.Slots.list.AddRange(cachedSlots);

            CachedSlots.Remove(target);
        }
    }
}