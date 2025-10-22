using Duckov.UI;
using HarmonyLib;

namespace Duckov_CashSlot.HarmonyPatches
{
    [HarmonyPatch(typeof(LootView), "OnOpen")]
    // ReSharper disable once InconsistentNaming
    internal class LootView_OnOpen
    {
        // ReSharper disable once InconsistentNaming
        private static void Postfix(LootView __instance)
        {
            var petInventoryDisplay =
                __instance.transform.Find("Main/EquipmentAndInventory/Content/InventoryDisplay_Pet");
            if (petInventoryDisplay == null) return;

            var cashInventoryDisplayTransform =
                petInventoryDisplay.transform.Find("CashInventoryDisplay");
            if (cashInventoryDisplayTransform == null) return;

            ModLogger.Log("Setting up cash slot display in loot view.");

            var cashInventoryDisplay = cashInventoryDisplayTransform.GetComponent<ItemSlotCollectionDisplay>();
            ItemSlotCollectionDisplay_Setup.SetupType = ItemSlotCollectionDisplay_Setup.SetupTypes.OnlyKeepCashSlots;
            cashInventoryDisplay.Setup(LevelManager.Instance.MainCharacter.CharacterItem, true);
            ItemSlotCollectionDisplay_Setup.SetupType = ItemSlotCollectionDisplay_Setup.SetupTypes.RemoveCashSlots;

            if (!CheckSuperPetEnabled()) return;

            ModLogger.Log("Super Pet mod detected, adjusting cash slot display.");
            
            cashInventoryDisplay.transform.SetSiblingIndex(2);
        }

        private static bool CheckSuperPetEnabled()
        {
            return Harmony.HasAnyPatches("SuperPet");
        }
    }
}