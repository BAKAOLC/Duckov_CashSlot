using System.Collections.Generic;
using HarmonyLib;
using ItemStatsSystem;

namespace Duckov_CashSlot.HarmonyPatches
{
    [HarmonyPatch]
    // ReSharper disable once InconsistentNaming
    internal class PatchItemSetting_Gun
    {
        [HarmonyPatch(typeof(ItemSetting_Gun), nameof(ItemSetting_Gun.GetBulletCountofTypeInInventory))]
        [HarmonyPostfix]
        // ReSharper disable InconsistentNaming
        private static void ItemSetting_Gun_GetBulletCountofTypeInInventory_Postfix(
                ItemSetting_Gun __instance,
                ref int __result,
                int bulletItemTypeID,
                Inventory inventory)
            // ReSharper restore InconsistentNaming
        {
            if (__instance.TargetBulletID == -1) return;
            if (inventory.AttachedToItem == null) return;
            if (inventory.AttachedToItem.Slots == null || inventory.AttachedToItem.Slots.Count == 0) return;

            var countInSlots = 0;
            foreach (var slot in inventory.AttachedToItem.Slots)
            {
                if (slot == null) continue;
                if (slot.Content == null) continue;
                if (slot.Content.TypeID != bulletItemTypeID) continue;
                if (!SlotManager.IsRegisteredSlot(slot)) continue;

                countInSlots += slot.Content.StackCount;
            }

            __result += countInSlots;
        }

        [HarmonyPatch(typeof(ItemSetting_Gun), nameof(ItemSetting_Gun.GetBulletTypesInInventory))]
        [HarmonyPostfix]
        // ReSharper disable InconsistentNaming
        private static void ItemSetting_Gun_GetBulletTypesInInventory_Postfix(
                ItemSetting_Gun __instance,
                ref Dictionary<int, BulletTypeInfo> __result,
                int ___caliberHash,
                Inventory inventory)
            // ReSharper restore InconsistentNaming
        {
            if (inventory.AttachedToItem == null) return;
            if (inventory.AttachedToItem.Slots == null || inventory.AttachedToItem.Slots.Count == 0) return;

            var str = __instance.Item.Constants.GetString(___caliberHash);
            foreach (var slot in inventory.AttachedToItem.Slots)
            {
                if (slot == null) continue;
                if (slot.Content == null) continue;
                if (!slot.Content.GetBool("IsBullet")) continue;
                if (slot.Content.Constants.GetString(___caliberHash) != str) continue;
                if (!SlotManager.IsRegisteredSlot(slot)) continue;

                if (!__result.TryGetValue(slot.Content.TypeID, out var existingInfo))
                {
                    var bulletTypeInfo = new BulletTypeInfo
                    {
                        bulletTypeID = slot.Content.TypeID,
                        count = slot.Content.StackCount,
                    };
                    __result.Add(slot.Content.TypeID, bulletTypeInfo);
                    continue;
                }

                existingInfo.count += slot.Content.StackCount;
            }
        }

        [HarmonyPatch(typeof(ItemSetting_Gun), nameof(ItemSetting_Gun.AutoSetTypeInInventory))]
        [HarmonyPostfix]
        // ReSharper disable InconsistentNaming
        private static void ItemSetting_Gun_AutoSetTypeInInventory_Postfix(
                ItemSetting_Gun __instance,
                ref bool __result,
                int ___caliberHash,
                Inventory inventory)
            // ReSharper restore InconsistentNaming
        {
            if (inventory == null) return;
            if (inventory.AttachedToItem == null) return;
            if (inventory.AttachedToItem.Slots == null || inventory.AttachedToItem.Slots.Count == 0) return;

            var str = __instance.Item.Constants.GetString(___caliberHash);
            foreach (var slot in inventory.AttachedToItem.Slots)
            {
                if (slot == null) continue;
                if (slot.Content == null) continue;
                if (!slot.Content.GetBool("IsBullet")) continue;
                if (slot.Content.Constants.GetString(___caliberHash) != str) continue;
                if (!SlotManager.IsRegisteredSlot(slot)) continue;

                __instance.SetTargetBulletType(slot.Content);
                __result = true;
                break;
            }
        }
    }
}