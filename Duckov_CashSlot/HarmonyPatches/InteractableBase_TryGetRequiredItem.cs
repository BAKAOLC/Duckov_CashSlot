using System.Linq;
using HarmonyLib;
using ItemStatsSystem;

namespace Duckov_CashSlot.HarmonyPatches
{
    [HarmonyPatch(typeof(InteractableBase), nameof(InteractableBase.TryGetRequiredItem))]
    // ReSharper disable once InconsistentNaming
    internal class InteractableBase_TryGetRequiredItem
    {
        // ReSharper disable InconsistentNaming
        private static void Postfix(InteractableBase __instance,
                CharacterMainControl fromCharacter,
                ref (bool hasItem, Item ItemInstance) __result,
                bool ___requireItem
            )
            // ReSharper restore InconsistentNaming
        {
            if (__result.hasItem) return;
            if (fromCharacter == null) return;

            var registeredSlot = SlotManager.GetAllRegisteredSlotsInItem(fromCharacter.CharacterItem);
            if (registeredSlot.Length == 0) return;

            var slotItems = registeredSlot.Select(slot => slot.Content).Where(item => item != null).ToArray();
            foreach (var item in slotItems)
            {
                if (item.TypeID == __instance.requireItemId)
                {
                    __result = (true, item);
                    return;
                }

                if (item.Slots == null || item.Slots.Count <= 0) continue;
                foreach (var slot in item.Slots)
                {
                    if (slot.Content == null || slot.Content.TypeID != __instance.requireItemId) continue;
                    __result = (true, slot.Content);
                    return;
                }
            }
        }
    }
}