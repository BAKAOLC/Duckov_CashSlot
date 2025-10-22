using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        internal static SetupTypes SetupType = SetupTypes.RemoveCashSlots;
        private static readonly Dictionary<Item, Slot[]> CachedSlots = [];

        private static void Prefix(ref Item target)
        {
            if (target == null || target.Slots == null) return;
            var cashSlot = target.Slots.GetSlot("Cash");
            if (cashSlot == null) return;

            CachedSlots[target] = [..target.Slots];

            switch (SetupType)
            {
                case SetupTypes.RemoveCashSlots:
                    target.Slots.Remove(cashSlot);
                    break;
                case SetupTypes.OnlyKeepCashSlots:
                    target.Slots.Clear();
                    target.Slots.Add(cashSlot);
                    break;
            }
        }

        // ReSharper disable once InconsistentNaming
        private static void Postfix(ref Item target)
        {
            if (target == null || target.Slots == null) return;
            if (!CachedSlots.TryGetValue(target, out var cachedSlots)) return;

            target.Slots.Clear();
            target.Slots.list.AddRange(cachedSlots);

            CachedSlots.Remove(target);
        }

        private static void RemoveCashSlotDisplays(List<SlotDisplay> slotDisplays)
        {
            var toRemove = slotDisplays
                .Where(slotDisplay => CashSlotManager.IsCashSlot(slotDisplay.Target))
                .ToArray();
            foreach (var slotDisplay in toRemove)
            {
                ClearEvents(slotDisplay);
                SlotDisplay.Release(slotDisplay);
            }
        }

        private static void OnlyKeepCashSlotDisplays(List<SlotDisplay> slotDisplays)
        {
            foreach (var slotDisplay in
                     slotDisplays.Where(slotDisplay => !CashSlotManager.IsCashSlot(slotDisplay.Target)))
            {
                ClearEvents(slotDisplay);
                SlotDisplay.Release(slotDisplay);
            }
        }

        private static void ClearEvents(SlotDisplay slotDisplay)
        {
            var onSlotDisplayClickedField = typeof(SlotDisplay)
                .GetField("onSlotDisplayClicked", BindingFlags.NonPublic | BindingFlags.Instance);
            if (onSlotDisplayClickedField != null) onSlotDisplayClickedField.SetValue(slotDisplay, null);

            var onSlotDisplayDoubleClickedField = typeof(SlotDisplay)
                .GetField("onSlotDisplayDoubleClicked", BindingFlags.NonPublic | BindingFlags.Instance);
            if (onSlotDisplayDoubleClickedField != null) onSlotDisplayDoubleClickedField.SetValue(slotDisplay, null);
        }

        internal enum SetupTypes
        {
            RemoveCashSlots,
            OnlyKeepCashSlots,
        }
    }
}