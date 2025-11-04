using System.Collections.Generic;
using System.Reflection;
using Duckov.MasterKeys.UI;
using HarmonyLib;
using ItemStatsSystem;
using ItemStatsSystem.Items;

namespace Duckov_CashSlot.HarmonyPatches
{
    [HarmonyPatch]
    // ReSharper disable once InconsistentNaming
    internal static class PatchSlot
    {
        private static readonly HashSet<Item> DisabledModifierItems = [];

        private static readonly PropertyInfo MasterKeysRegisterViewKeySlot =
            AccessTools.Property(typeof(MasterKeysRegisterView), "KeySlot");

        [HarmonyPatch(typeof(Slot), "CheckAbleToPlug")]
        [HarmonyPostfix]
        // ReSharper disable InconsistentNaming
        private static void CheckAbleToPlug_Postfix(Slot __instance, ref Item otherItem, ref bool __result)
            // ReSharper restore InconsistentNaming
        {
            if (!__result) return;
            if (otherItem == null) return;
            if (otherItem.TypeID != ModConstant.KeyRingTypeID) return;

            var isKeyRingItem = __instance.Master != null && __instance.Master.TypeID == ModConstant.KeyRingTypeID;
            var masterKeysRegisterViewKeySlot =
                (Slot)MasterKeysRegisterViewKeySlot.GetValue(MasterKeysRegisterView.Instance);
            var isMasterKeysRegisterSlot = __instance == masterKeysRegisterViewKeySlot;

            if (!isKeyRingItem && !isMasterKeysRegisterSlot) return;
            __result = false;
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
            if (isSlotForbidDeathDrop) TagManager.SetDontDropOnDeadTag(otherItem);

            var isSlotDisableModifiers = SlotManager.IsSlotDisableModifiers(__instance);
            if (isSlotDisableModifiers) DisableModifiersForItem(otherItem);
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

            EnableModifiersForItem(__result);
        }

        private static void OnItemStackCountChanged(Item item)
        {
            if (item == null) return;

            var slot = item.PluggedIntoSlot;
            slot?.ForceInvokeSlotContentChangedEvent();
        }

        private static void DisableModifiersForItem(Item item)
        {
            if (item == null) return;
            if (item.Modifiers == null) return;
            item.Modifiers.ModifierEnable = false;
            DisabledModifierItems.Add(item);
        }

        private static void EnableModifiersForItem(Item item)
        {
            if (item == null) return;
            if (item.Modifiers == null) return;
            if (!DisabledModifierItems.Contains(item)) return;
            item.Modifiers.ModifierEnable = true;
            DisabledModifierItems.Remove(item);
        }
    }
}