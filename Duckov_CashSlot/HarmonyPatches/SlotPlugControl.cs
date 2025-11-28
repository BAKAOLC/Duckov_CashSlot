using Duckov.UI;
using HarmonyLib;
using ItemStatsSystem.Items;

namespace Duckov_CashSlot.HarmonyPatches
{
    [HarmonyPatch]
    internal class SlotPlugControl
    {
        private static bool _flag;

        [HarmonyPatch(typeof(LootView), "OnLootTargetItemDoubleClicked")]
        [HarmonyPrefix]
        private static void LootView_OnLootTargetItemDoubleClicked_Prefix()
        {
            _flag = true;
        }

        [HarmonyPatch(typeof(LootView), "OnLootTargetItemDoubleClicked")]
        [HarmonyPostfix]
        private static void LootView_OnLootTargetItemDoubleClicked_Postfix()
        {
            _flag = false;
        }

        [HarmonyPatch(typeof(CharacterItemControl), nameof(CharacterItemControl.PickupItem))]
        [HarmonyPrefix]
        private static void CharacterItemControl_PickupItem_Prefix()
        {
            _flag = true;
        }

        [HarmonyPatch(typeof(CharacterItemControl), nameof(CharacterItemControl.PickupItem))]
        [HarmonyPostfix]
        private static void CharacterItemControl_PickupItem_Postfix()
        {
            _flag = false;
        }

        [HarmonyPatch(typeof(Slot), nameof(Slot.CanPlug))]
        [HarmonyPostfix]
        // ReSharper disable InconsistentNaming
        private static void Slot_CanPlug_Postfix(Slot __instance, ref bool __result)
            // ReSharper restore InconsistentNaming
        {
            if (!_flag) return;
            if (!__result) return;
            if (!SlotManager.IsRegisteredSlot(__instance)) return;

            if (SlotManager.IsSlotForbidAutoPlug(__instance)) __result = false;
        }
    }
}