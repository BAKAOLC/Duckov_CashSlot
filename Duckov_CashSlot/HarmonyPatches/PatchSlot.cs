using HarmonyLib;
using ItemStatsSystem;
using ItemStatsSystem.Items;

namespace Duckov_CashSlot.HarmonyPatches
{
    [HarmonyPatch]
    // ReSharper disable once InconsistentNaming
    internal static class PatchSlot
    {
        [HarmonyPatch(typeof(Slot), nameof(Slot.Plug))]
        [HarmonyPrefix]
        // ReSharper disable InconsistentNaming
        private static bool Plug_Prefix(Slot __instance, ref bool __result, ref Item otherItem)
            // ReSharper restore InconsistentNaming
        {
            if (otherItem == null) return true;
            if (otherItem.TypeID != ModConstant.KeyRingTypeID) return true;
            if (SlotManager.IsRegisteredSlot(__instance)) return true;

            ModLogger.Log(
                $"Prevented plugging Key Ring item '{otherItem.DisplayName}' into unregistered slot '{__instance.Key}'.");
            __result = false;
            return false;
        }

        [HarmonyPatch(typeof(Slot), nameof(Slot.Plug))]
        [HarmonyPostfix]
        // ReSharper disable once InconsistentNaming
        private static void Plug_Postfix(Slot __instance, ref Item otherItem)
        {
            if (otherItem == null) return;
            if (!SlotManager.IsRegisteredSlot(__instance)) return;

            otherItem.onSetStackCount -= OnItemStackCountChanged;
            otherItem.onSetStackCount += OnItemStackCountChanged;

            var isSlotForbidDeathDrop = SlotManager.IsSlotForbidDeathDrop(__instance);
            if (!isSlotForbidDeathDrop) return;

            TagManager.SetDontDropOnDeadTag(otherItem);
        }

        [HarmonyPatch(typeof(Slot), nameof(Slot.Unplug))]
        [HarmonyPostfix]
        // ReSharper disable InconsistentNaming
        private static void Unplug_Postfix(Slot __instance, ref Item __result)
            // ReSharper restore InconsistentNaming
        {
            if (__result == null) return;
            if (!SlotManager.IsRegisteredSlot(__instance)) return;

            __result.onSetStackCount -= OnItemStackCountChanged;

            TagManager.CheckRemoveDontDropOnDeadTag(__result);
        }

        private static void OnItemStackCountChanged(Item item)
        {
            if (item == null) return;

            var slot = item.PluggedIntoSlot;
            slot?.ForceInvokeSlotContentChangedEvent();
        }
    }
}