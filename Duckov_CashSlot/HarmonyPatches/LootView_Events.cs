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
        // ReSharper disable InconsistentNaming
        private static void RegisterEvents_Postfix(LootView __instance, InventoryDisplay ___petInventoryDisplay)
            // ReSharper restore InconsistentNaming
        {
            if (___petInventoryDisplay == null) return;

            var petSlotCollectionDisplayTransform =
                ___petInventoryDisplay.transform.Find("PetSlotCollectionDisplay");
            if (petSlotCollectionDisplayTransform == null) return;

            var petSlotCollectionDisplay =
                petSlotCollectionDisplayTransform.GetComponent<ItemSlotCollectionDisplay>();
            if (petSlotCollectionDisplay == null) return;

            var onCharacterSlotItemDoubleClickedMethod = typeof(LootView)
                .GetMethod("OnCharacterSlotItemDoubleClicked", BindingFlags.NonPublic | BindingFlags.Instance);
            if (onCharacterSlotItemDoubleClickedMethod == null) return;

            petSlotCollectionDisplay.onElementDoubleClicked += OnElementDoubleClicked;
            LootViewToCashInventoryDisplay[__instance] = OnElementDoubleClicked;

            return;

            void OnElementDoubleClicked(ItemSlotCollectionDisplay collectionDisplay, SlotDisplay slotDisplay)
            {
                onCharacterSlotItemDoubleClickedMethod.Invoke(__instance, [collectionDisplay, slotDisplay]);
            }
        }

        [HarmonyPatch(typeof(LootView), "UnregisterEvents")]
        [HarmonyPostfix]
        // ReSharper disable InconsistentNaming
        private static void UnregisterEvents_Postfix(LootView __instance, InventoryDisplay ___petInventoryDisplay)
            // ReSharper restore InconsistentNaming
        {
            if (!LootViewToCashInventoryDisplay.TryGetValue(__instance, out var onElementDoubleClicked)) return;

            if (___petInventoryDisplay == null)
            {
                LootViewToCashInventoryDisplay.Remove(__instance);
                return;
            }

            var petSlotCollectionDisplayTransform =
                ___petInventoryDisplay.transform.Find("PetSlotCollectionDisplay");
            if (petSlotCollectionDisplayTransform == null) return;

            var petSlotCollectionDisplay =
                petSlotCollectionDisplayTransform.GetComponent<ItemSlotCollectionDisplay>();
            if (petSlotCollectionDisplay == null) return;

            petSlotCollectionDisplay.onElementDoubleClicked -= onElementDoubleClicked;
            LootViewToCashInventoryDisplay.Remove(__instance);
        }
    }
}