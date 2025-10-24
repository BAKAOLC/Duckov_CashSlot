using Duckov_CashSlot.Enums;
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
        [HarmonyAfter("SuperPet", "MergeMyMOD")]
        [HarmonyPriority(Priority.Low)]
        private static void Postfix(InventoryDisplay ___petInventoryDisplay)
        {
            if (___petInventoryDisplay == null) return;

            var petSlotCollectionDisplayTransform =
                ___petInventoryDisplay.transform.Find(ModConstant.SlotCollectionDisplayName);
            if (petSlotCollectionDisplayTransform == null) return;

            ModLogger.Log("Setting up cash slot display in loot view.");

            var petSlotCollectionDisplay = petSlotCollectionDisplayTransform.GetComponent<ItemSlotCollectionDisplay>();
            ItemSlotCollectionDisplay_Setup.CurrentShowIn = ShowIn.Pet;
            petSlotCollectionDisplay.Setup(LevelManager.Instance.MainCharacter.CharacterItem, true);
            ItemSlotCollectionDisplay_Setup.CurrentShowIn = ShowIn.Character;

            ResetGridLayout(petSlotCollectionDisplay);

            if (!CheckSuperPetEnabled(___petInventoryDisplay)) return;

            ModLogger.Log("Super Pet mod detected, adjusting cash slot display.");

            petSlotCollectionDisplay.transform.SetSiblingIndex(2);
        }

        private static bool CheckSuperPetEnabled(InventoryDisplay petInventoryDisplay)
        {
            var gridLayoutElementField = AccessTools.Field(typeof(InventoryDisplay), "gridLayoutElement");
            if (gridLayoutElementField == null) return false;
            var layoutElement = gridLayoutElementField.GetValue(petInventoryDisplay) as LayoutElement;
            return layoutElement != null && layoutElement.ignoreLayout;
        }

        private static void ResetGridLayout(ItemSlotCollectionDisplay slotCollectionDisplay)
        {
            var layoutElement = slotCollectionDisplay.GetComponent<LayoutElement>();
            if (layoutElement == null) return;

            layoutElement.preferredHeight = -1;
        }
    }
    // ReSharper restore InconsistentNaming
}