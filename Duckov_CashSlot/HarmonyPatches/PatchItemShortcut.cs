using System.Collections.Generic;
using System.Reflection;
using Duckov;
using HarmonyLib;
using ItemStatsSystem;

namespace Duckov_CashSlot.HarmonyPatches
{
    [HarmonyPatch]
    internal class PatchItemShortcut
    {
        [HarmonyPatch(typeof(ItemShortcut), nameof(ItemShortcut.IsItemValid))]
        [HarmonyPostfix]
        // ReSharper disable once InconsistentNaming
        private static void IsItemValid_Postfix(ref bool __result, Item item)
        {
            if (item == null) return;
            if (item.Tags.Contains("Weapon")) return;

            if (CheckIsInRegisteredSlot(item)) __result = true;
        }

        internal static bool CheckIsInRegisteredSlot(Item item)
        {
            if (item == null) return false;
            if (item.InInventory != null) return false;
            return item.PluggedIntoSlot != null && SlotManager.IsRegisteredSlot(item.PluggedIntoSlot);
        }

        internal static MethodBase? GetShortcutSetLocalMethod()
        {
            return AccessTools.Method(typeof(ItemShortcut), "Set_Local");
        }

        internal static Inventory? GetShortcutMainInventory()
        {
            var mainInventoryProperty = AccessTools.Property(typeof(ItemShortcut), "MainInventory");
            return mainInventoryProperty.GetValue(null) as Inventory;
        }

        internal static List<Item>? GetShortcutItems(ItemShortcut shortcut)
        {
            var itemsField = AccessTools.Field(typeof(ItemShortcut), "items");
            return itemsField.GetValue(shortcut) as List<Item>;
        }

        internal static List<int>? GetSaveDataInventoryIndexes(object saveData)
        {
            var inventoryIndexesField = AccessTools.Field(saveData.GetType(), "inventoryIndexes");
            return inventoryIndexesField.GetValue(saveData) as List<int>;
        }
    }

    [HarmonyPatch]
    internal class PatchItemShortcutSaveDataGenerate
    {
        private static MethodBase? TargetMethod()
        {
            var nestedType =
                typeof(ItemShortcut).GetNestedType("SaveData", BindingFlags.NonPublic | BindingFlags.Public);
            return nestedType == null ? null : AccessTools.Method(nestedType, "Generate");
        }

        // ReSharper disable once InconsistentNaming
        private static void Postfix(object __instance, ItemShortcut shortcut)
        {
            var items = PatchItemShortcut.GetShortcutItems(shortcut);
            if (items == null) return;

            var saveDataInventoryIndexes = PatchItemShortcut.GetSaveDataInventoryIndexes(__instance);
            if (saveDataInventoryIndexes == null) return;

            for (var index = 0; index < items.Count; ++index)
            {
                var item = items[index];
                if (item == null) continue;
                if (!PatchItemShortcut.CheckIsInRegisteredSlot(item)) continue;

                var slotInventoryIndex = SlotManager.GetSlotInventoryIndex(item.PluggedIntoSlot);
                saveDataInventoryIndexes[index] = slotInventoryIndex;
            }
        }
    }

    [HarmonyPatch]
    internal class PatchItemShortcutSaveDataApplyTo
    {
        private static MethodBase? TargetMethod()
        {
            var nestedType =
                typeof(ItemShortcut).GetNestedType("SaveData", BindingFlags.NonPublic | BindingFlags.Public);
            return nestedType == null ? null : AccessTools.Method(nestedType, "ApplyTo");
        }

        // ReSharper disable once InconsistentNaming
        private static void Postfix(object __instance, ItemShortcut shortcut)
        {
            var saveDataInventoryIndexes = PatchItemShortcut.GetSaveDataInventoryIndexes(__instance);
            if (saveDataInventoryIndexes == null) return;

            var mainInventory = PatchItemShortcut.GetShortcutMainInventory();
            if (mainInventory == null) return;

            var targetItem = mainInventory.AttachedToItem;
            if (targetItem == null) return;

            var setLocalMethod = PatchItemShortcut.GetShortcutSetLocalMethod();
            if (setLocalMethod == null) return;

            for (var index = 0; index < saveDataInventoryIndexes.Count; ++index)
            {
                var inventoryIndex = saveDataInventoryIndexes[index];
                if (inventoryIndex >= 0) continue;

                var slot = SlotManager.GetSlotByInventoryIndex(targetItem, inventoryIndex);
                if (slot == null) continue;

                setLocalMethod.Invoke(shortcut, [index, slot.Content]);
            }
        }
    }
}