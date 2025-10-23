using System.Reflection;
using Duckov.UI;
using HarmonyLib;
using UnityEngine.UI;

namespace Duckov_CashSlot.HarmonyPatches
{
    [HarmonyPatch(typeof(LootView), "OnOpen")]
    // ReSharper disable once InconsistentNaming
    internal class LootView_OnOpen
    {
        // ReSharper disable InconsistentNaming
        private static void Postfix(InventoryDisplay ___petInventoryDisplay)
        {
            if (___petInventoryDisplay == null) return;

            var cashInventoryDisplayTransform =
                ___petInventoryDisplay.transform.Find("CashInventoryDisplay");
            if (cashInventoryDisplayTransform == null) return;

            ModLogger.Log("Setting up cash slot display in loot view.");

            var cashInventoryDisplay = cashInventoryDisplayTransform.GetComponent<ItemSlotCollectionDisplay>();
            ItemSlotCollectionDisplay_Setup.SetupType = ItemSlotCollectionDisplay_Setup.SetupTypes.OnlyKeepCashSlots;
            cashInventoryDisplay.Setup(LevelManager.Instance.MainCharacter.CharacterItem, true);
            ItemSlotCollectionDisplay_Setup.SetupType = ItemSlotCollectionDisplay_Setup.SetupTypes.RemoveCashSlots;

            if (!CheckSuperPetEnabled(___petInventoryDisplay)) return;

            ModLogger.Log("Super Pet mod detected, adjusting cash slot display.");

            cashInventoryDisplay.transform.SetSiblingIndex(2);
        }

        private static bool CheckSuperPetEnabled(InventoryDisplay petInventoryDisplay)
        {
            var field = typeof(InventoryDisplay)
                .GetField("gridLayoutElement", BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null) return false;
            var layoutElement = field.GetValue(petInventoryDisplay) as LayoutElement;
            return layoutElement != null && layoutElement.ignoreLayout;
        }
    }
    // ReSharper restore InconsistentNaming
}