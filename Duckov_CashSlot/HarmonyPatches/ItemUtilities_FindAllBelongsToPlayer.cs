using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using ItemStatsSystem;

namespace Duckov_CashSlot.HarmonyPatches
{
    [HarmonyPatch(typeof(ItemUtilities), nameof(ItemUtilities.FindAllBelongsToPlayer))]
    // ReSharper disable once InconsistentNaming
    internal class ItemUtilities_FindAllBelongsToPlayer
    {
        // ReSharper disable once InconsistentNaming
        private static void Postfix(ref List<Item> __result, Predicate<Item> predicate)
        {
            var mainCharacter = LevelManager.Instance.MainCharacter;
            if (mainCharacter == null) return;

            var mainCharacterItem = mainCharacter.CharacterItem;
            var registeredSlot = SlotManager.GetAllRegisteredSlotsInItem(mainCharacterItem);
            if (registeredSlot.Length == 0) return;

            var resultItems = registeredSlot.Select(slot => slot.Content)
                .Where(item => item != null)
                .Where(item => predicate(item))
                .ToArray();
            __result.AddRange(resultItems);
        }
    }
}