using System;
using System.Collections.Generic;
using System.Reflection;
using Duckov.UI;
using HarmonyLib;

namespace Duckov_CashSlot.HarmonyPatches
{
    [HarmonyPatch]
    // ReSharper disable once InconsistentNaming
    internal static class LootView_Events
    {
        private static readonly Dictionary<LootView, Action<ItemSlotCollectionDisplay, SlotDisplay>>
            LootViewToCashInventoryDisplay = [];

        [HarmonyPatch(typeof(LootView), "RegisterEvents")]
        [HarmonyPostfix]
        // ReSharper disable once InconsistentNaming
        private static void RegisterEvents_Postfix(LootView __instance)
        {
            var cashInventoryDisplayTransform =
                __instance.transform.Find(
                    "Main/EquipmentAndInventory/Content/InventoryDisplay_Pet/CashInventoryDisplay");
            if (cashInventoryDisplayTransform == null) return;

            var cashInventoryDisplay =
                cashInventoryDisplayTransform.GetComponent<ItemSlotCollectionDisplay>();
            if (cashInventoryDisplay == null) return;

            var onCharacterSlotItemDoubleClickedMethod = typeof(LootView)
                .GetMethod("OnCharacterSlotItemDoubleClicked", BindingFlags.NonPublic | BindingFlags.Instance);
            if (onCharacterSlotItemDoubleClickedMethod == null) return;

            cashInventoryDisplay.onElementDoubleClicked += OnElementDoubleClicked;
            LootViewToCashInventoryDisplay[__instance] = OnElementDoubleClicked;

            return;

            void OnElementDoubleClicked(ItemSlotCollectionDisplay collectionDisplay, SlotDisplay slotDisplay)
            {
                onCharacterSlotItemDoubleClickedMethod.Invoke(__instance, [collectionDisplay, slotDisplay]);
            }
        }

        [HarmonyPatch(typeof(LootView), "UnregisterEvents")]
        [HarmonyPostfix]
        // ReSharper disable once InconsistentNaming
        private static void UnregisterEvents_Postfix(LootView __instance)
        {
            if (!LootViewToCashInventoryDisplay.TryGetValue(__instance, out var onElementDoubleClicked)) return;

            var cashInventoryDisplayTransform =
                __instance.transform.Find(
                    "Main/EquipmentAndInventory/Content/InventoryDisplay_Pet/CashInventoryDisplay");
            if (cashInventoryDisplayTransform == null) return;

            var cashInventoryDisplay =
                cashInventoryDisplayTransform.GetComponent<ItemSlotCollectionDisplay>();
            if (cashInventoryDisplay == null) return;

            cashInventoryDisplay.onElementDoubleClicked -= onElementDoubleClicked;
            LootViewToCashInventoryDisplay.Remove(__instance);
        }
    }
}