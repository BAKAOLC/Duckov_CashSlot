using Duckov_CashSlot.Configs;
using Duckov_CashSlot.Data;
using Duckov.UI;
using HarmonyLib;
using UnityEngine;
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
        private static void Postfix(
            ItemSlotCollectionDisplay ___characterSlotCollectionDisplay,
            InventoryDisplay ___petInventoryDisplay)
        {
            ProcessCharacterItemSlotCollectionDisplay(___characterSlotCollectionDisplay);
            ProcessPetInventoryDisplay(___petInventoryDisplay);
        }

        private static void ProcessCharacterItemSlotCollectionDisplay(
            ItemSlotCollectionDisplay itemSlotCollectionDisplay)
        {
            if (itemSlotCollectionDisplay == null) return;
            Utility.SetSlotCollectionScrollable(itemSlotCollectionDisplay,
                SlotDisplaySetting.Instance.InventorySlotDisplayRows);
        }

        private static void ProcessPetInventoryDisplay(InventoryDisplay petInventoryDisplay)
        {
            if (petInventoryDisplay == null) return;

            var petSlotCollectionDisplayTransform =
                petInventoryDisplay.transform.Find(ModConstant.SlotCollectionDisplayName);
            if (petSlotCollectionDisplayTransform == null) return;

            SetContentSizeFitter(petInventoryDisplay);

            ModLogger.Log("Setting up slot display in loot view.");

            var petSlotCollectionDisplay = petSlotCollectionDisplayTransform.GetComponent<ItemSlotCollectionDisplay>();
            ItemSlotCollectionDisplay_Setup.CurrentShowIn = ShowIn.Pet;
            petSlotCollectionDisplay.Setup(LevelManager.Instance.MainCharacter.CharacterItem, true);
            ItemSlotCollectionDisplay_Setup.CurrentShowIn = ShowIn.Character;

            Utility.SetSlotCollectionScrollable(petSlotCollectionDisplay,
                SlotDisplaySetting.Instance.PetSlotDisplayRows);

            var gridLayout = petSlotCollectionDisplayTransform.Find("GridLayout");
            if (gridLayout)
                Utility.SetGridLayoutConstraintFixedColumnCount(gridLayout.gameObject,
                    SlotDisplaySetting.Instance.PetSlotDisplayColumns);
            
            var siblingIndex = SlotDisplaySetting.Instance.PetSlotDisplayAboveInventory ? 2 : 3;
                petSlotCollectionDisplay.transform.SetSiblingIndex(siblingIndex);

            var allowModifyOtherModDisplay = SlotDisplaySetting.Instance.AllowModifyOtherModPetDisplay;
            if (IsSuperModPatched())
            {
                CheckSuperPet(petInventoryDisplay, petSlotCollectionDisplay);
                if (!allowModifyOtherModDisplay) return; // Skip further adjustments if not allowed
            }

            if (IsBetterDuckovPatched() && !allowModifyOtherModDisplay)
                return; // Skip further adjustments if not allowed

            SetPetInventoryDisplayGridLayoutColumns(petInventoryDisplay);
        }

        private static void SetPetInventoryDisplayGridLayoutColumns(InventoryDisplay petInventoryDisplay)
        {
            var petSlotGridLayout = petInventoryDisplay.transform.Find("Container/Layout");
            if (!petSlotGridLayout) return;
            Utility.SetGridLayoutConstraintFixedColumnCount(petSlotGridLayout.gameObject,
                SlotDisplaySetting.Instance.PetInventoryDisplayColumns);
            Utility.ResetLayoutElementMinPreferredHeight(petSlotGridLayout.gameObject);
        }

        private static void SetContentSizeFitter(InventoryDisplay petInventoryDisplay)
        {
            var contentSizeFitter = petInventoryDisplay.GetComponent<ContentSizeFitter>();
            if (contentSizeFitter == null) return;

            contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            ModLogger.Log("Adjusted ContentSizeFitter for pet inventory display.");
        }

        private static void CheckSuperPet(InventoryDisplay petInventoryDisplay,
            ItemSlotCollectionDisplay petSlotCollectionDisplay)
        {
            if (!SlotDisplaySetting.Instance.NewSuperPetDisplayCompact)
            {
                CheckSuperPet_old(petInventoryDisplay, petSlotCollectionDisplay);
                return;
            }

            var contentLayoutField = AccessTools.Field(typeof(InventoryDisplay), "contentLayout");
            if (contentLayoutField == null) return;
            var contentLayout = contentLayoutField.GetValue(petInventoryDisplay) as GridLayoutGroup;
            if (contentLayout == null) return;

            var zeroOffset = new RectOffset(0, 0, 0, 0);

            if (contentLayout.padding != zeroOffset)
            {
                contentLayout.padding = zeroOffset;
                ModLogger.Log(
                    "Reset GridLayoutGroup padding for pet inventory display from probable Super Pet modification.");
            }

            var gridLayoutElementField = AccessTools.Field(typeof(InventoryDisplay), "gridLayoutElement");
            if (gridLayoutElementField == null) return;
            var layoutElement = gridLayoutElementField.GetValue(petInventoryDisplay) as LayoutElement;
            if (layoutElement == null || !layoutElement.ignoreLayout) return;
            {
                layoutElement.ignoreLayout = false;
                ModLogger.Log(
                    "Reset LayoutElement ignoreLayout for pet inventory display from probable Super Pet modification.");
            }
        }

        private static void CheckSuperPet_old(InventoryDisplay petInventoryDisplay,
            ItemSlotCollectionDisplay petSlotCollectionDisplay)
        {
            var gridLayoutElementField = AccessTools.Field(typeof(InventoryDisplay), "gridLayoutElement");
            if (gridLayoutElementField == null) return;
            var layoutElement = gridLayoutElementField.GetValue(petInventoryDisplay) as LayoutElement;
            if (!(layoutElement != null && layoutElement.ignoreLayout)) return;

            petSlotCollectionDisplay.transform.SetSiblingIndex(2);
        }

        private static bool IsSuperModPatched()
        {
            return Harmony.HasAnyPatches(ModConstant.SuperPetModID) ||
                   Harmony.HasAnyPatches(ModConstant.MergeMyModID);
        }

        private static bool IsBetterDuckovPatched()
        {
            return Harmony.HasAnyPatches(ModConstant.BetterDuckovID);
        }
    }
    // ReSharper restore InconsistentNaming
}