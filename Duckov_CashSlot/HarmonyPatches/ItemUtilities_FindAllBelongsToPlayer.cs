using System;
using System.Collections.Generic;
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
            var cashSlots = CashSlotManager.FindCashSlotInItem(mainCharacterItem);
            if (cashSlots == null) return;

            var cashSlotItem = cashSlots.Content;
            if (cashSlotItem == null) return;

            if (predicate(cashSlotItem))
                __result.Add(cashSlotItem);
        }
    }
}