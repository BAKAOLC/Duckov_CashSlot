using Duckov.UI;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

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
            var petItemSlotCollectionDisplay =
                Object.Instantiate(itemSlotCollectionDisplay, petInventoryDisplay.transform);
            petItemSlotCollectionDisplay.name = ModConstant.SlotCollectionDisplayName;

            ResetDisplaySettings(petItemSlotCollectionDisplay);
        }

        private static void ResetDisplaySettings(ItemSlotCollectionDisplay itemSlotCollectionDisplay)
        {
            if (itemSlotCollectionDisplay == null) return;

            if (!Utility.GetComponent(itemSlotCollectionDisplay.gameObject, out LayoutElement layoutElement))
            {
                ModLogger.Log("No LayoutElement found on slot collection display.");
                return;
            }

            layoutElement.minHeight = -1;
            layoutElement.preferredHeight = -1;

            var gridLayout = itemSlotCollectionDisplay.transform.Find("GridLayout");
            if (gridLayout == null) return;

            if (!Utility.GetComponent(gridLayout.gameObject, out GridLayoutGroup gridLayoutGroup))
            {
                ModLogger.Log("No GridLayoutGroup found on GridLayout.");
                return;
            }

            gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        }
    }
}