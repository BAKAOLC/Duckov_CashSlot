using Duckov.UI;
using HarmonyLib;
using UnityEngine;

namespace Duckov_CashSlot.HarmonyPatches
{
    [HarmonyPatch(typeof(LootView), "Awake")]
    // ReSharper disable once InconsistentNaming
    internal class LootView_Awake
    {
        // ReSharper disable InconsistentNaming
        private static void Prefix(LootView __instance,
                InventoryDisplay ___petInventoryDisplay,
                ItemSlotCollectionDisplay ___characterSlotCollectionDisplay)
            // ReSharper restore InconsistentNaming
        {
            if (___petInventoryDisplay == null) return;

            ModLogger.Log("Creating pet slot collection display.");
            CreatePetSlotCollectionDisplay(___petInventoryDisplay, ___characterSlotCollectionDisplay);
        }

        private static void CreatePetSlotCollectionDisplay(InventoryDisplay petInventoryDisplay,
            ItemSlotCollectionDisplay itemSlotCollectionDisplay)
        {
            var cashInventoryDisplay =
                Object.Instantiate(itemSlotCollectionDisplay, petInventoryDisplay.transform);
            cashInventoryDisplay.name = ModConstant.SlotCollectionDisplayName;
        }
    }
}