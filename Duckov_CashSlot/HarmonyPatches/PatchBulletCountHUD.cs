using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using ItemStatsSystem.Items;

namespace Duckov_CashSlot.HarmonyPatches
{
    [HarmonyPatch]
    // ReSharper disable once InconsistentNaming
    internal class PatchBulletCountHUD
    {
        private static readonly Dictionary<BulletCountHUD, Action<Slot>> PatchedInstances = [];
        private static MethodBase? _changeTotalCountMethod;

        [HarmonyPatch(typeof(BulletCountHUD), "Update")]
        [HarmonyPostfix]
        // ReSharper disable InconsistentNaming
        private static void BulletCountHUD_Update_Postfix(BulletCountHUD __instance,
                CharacterMainControl ___characterMainControl)
            // ReSharper restore InconsistentNaming
        {
            if (___characterMainControl == null) return;
            if (PatchedInstances.ContainsKey(__instance)) return;

            _changeTotalCountMethod ??= AccessTools.Method(typeof(BulletCountHUD), "ChangeTotalCount");
            foreach (var slot in ___characterMainControl.CharacterItem.Slots)
            {
                if (slot == null) continue;
                slot.onSlotContentChanged += OnSlotChanged;
            }

            PatchedInstances.Add(__instance, OnSlotChanged);

            return;

            void OnSlotChanged(Slot slot)
            {
                _changeTotalCountMethod ??= AccessTools.Method(typeof(BulletCountHUD), "ChangeTotalCount");
                _changeTotalCountMethod?.Invoke(__instance, null);
            }
        }

        [HarmonyPatch(typeof(BulletCountHUD), "OnDestroy")]
        [HarmonyPostfix]
        // ReSharper disable InconsistentNaming
        private static void BulletCountHUD_OnDestroy_Postfix(BulletCountHUD __instance,
                CharacterMainControl ___characterMainControl)
            // ReSharper restore InconsistentNaming
        {
            if (!PatchedInstances.Remove(__instance, out var onSlotChanged)) return;

            if (___characterMainControl == null) return;
            foreach (var slot in ___characterMainControl.CharacterItem.Slots)
            {
                if (slot == null) continue;
                slot.onSlotContentChanged -= onSlotChanged;
            }
        }
    }
}