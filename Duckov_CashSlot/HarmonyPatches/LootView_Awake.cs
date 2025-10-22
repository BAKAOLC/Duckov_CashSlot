using System.Reflection;
using Duckov.UI;
using HarmonyLib;
using UnityEngine;

namespace Duckov_CashSlot.HarmonyPatches
{
    [HarmonyPatch(typeof(LootView), "Awake")]
    // ReSharper disable once InconsistentNaming
    internal class LootView_Awake
    {
        // ReSharper disable once InconsistentNaming
        private static void Prefix(LootView __instance)
        {
            var petInventoryDisplay = FindPetInventoryDisplay(__instance);
            if (petInventoryDisplay == null) return;

            var itemSlotCollectionDisplay = FindItemSlotCollectionDisplay(__instance);
            if (itemSlotCollectionDisplay == null) return;

            ModLogger.Log("Creating cash slot display.");
            CreateCashSlotDisplay(petInventoryDisplay, itemSlotCollectionDisplay);
        }

        private static InventoryDisplay? FindPetInventoryDisplay(LootView lootView)
        {
            var petInventoryDisplayField = typeof(LootView)
                .GetField("petInventoryDisplay", BindingFlags.NonPublic | BindingFlags.Instance);
            if (petInventoryDisplayField == null) return null;

            var petInventoryDisplay = petInventoryDisplayField.GetValue(lootView) as InventoryDisplay;
            return petInventoryDisplay;
        }

        private static ItemSlotCollectionDisplay? FindItemSlotCollectionDisplay(LootView lootView)
        {
            var itemSlotCollectionDisplayField = typeof(LootView)
                .GetField("characterSlotCollectionDisplay", BindingFlags.NonPublic | BindingFlags.Instance);
            if (itemSlotCollectionDisplayField == null) return null;

            var itemSlotCollectionDisplay =
                itemSlotCollectionDisplayField.GetValue(lootView) as ItemSlotCollectionDisplay;
            return itemSlotCollectionDisplay;
        }

        private static void CreateCashSlotDisplay(InventoryDisplay petInventoryDisplay,
            ItemSlotCollectionDisplay itemSlotCollectionDisplay)
        {
            var cashInventoryDisplay =
                Object.Instantiate(itemSlotCollectionDisplay, petInventoryDisplay.transform);
            cashInventoryDisplay.name = "CashInventoryDisplay";
        }
    }
}