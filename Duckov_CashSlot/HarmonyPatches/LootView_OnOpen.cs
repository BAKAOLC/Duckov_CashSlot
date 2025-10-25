using Duckov_CashSlot.Data;
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

            ModLogger.Log("Setting up slot display in loot view.");

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
            var gridLayout = slotCollectionDisplay.transform.Find("GridLayout");
            if (gridLayout == null) return;

            var gridLayoutGroup = gridLayout.GetComponent<GridLayoutGroup>();
            if (gridLayoutGroup == null) return;

            var layoutElement = slotCollectionDisplay.GetComponent<LayoutElement>();
            if (layoutElement == null) return;

            var cellH = gridLayoutGroup.cellSize.y;
            var spacingY = gridLayoutGroup.spacing.y;
            var pad = gridLayoutGroup.padding;

            var childCount = gridLayout.childCount;
            if (childCount <= ModConstant.PetSlotDisplayCount)
            {
                layoutElement.minHeight = -1;
                layoutElement.preferredHeight = -1;
                return;
            }

            const int rows = ModConstant.PetSlotDisplayCount;
            var minHeight = pad.top + pad.bottom + rows * cellH + (rows - 1) * spacingY;
            var preferredHeight = pad.top + pad.bottom + rows * cellH + (rows + 1) * spacingY;
            layoutElement.minHeight = minHeight;
            layoutElement.preferredHeight = preferredHeight;
        }
    }
    // ReSharper restore InconsistentNaming
}