using System;
using System.Collections.Generic;
using Duckov;
using Duckov.UI;
using HarmonyLib;

namespace Duckov_CashSlot.HarmonyPatches
{
    [HarmonyPatch]
    // ReSharper disable once InconsistentNaming
    internal class PatchSlotDisplay
    {
        private static readonly Dictionary<SlotDisplay, Action<UIInputEventData, int>> RegisteredLootViews = [];

        [HarmonyPatch(typeof(SlotDisplay), "RegisterEvents")]
        [HarmonyPostfix]
        // ReSharper disable once InconsistentNaming
        private static void RegisterEvents_PostFix(SlotDisplay __instance)
        {
            UIInputManager.OnShortcutInput += OnShortcutInput;
            RegisteredLootViews[__instance] = OnShortcutInput;

            return;

            void OnShortcutInput(UIInputEventData data, int shortcutIndex)
            {
                OnShortcutInputHandle(__instance, data, shortcutIndex);
            }
        }

        [HarmonyPatch(typeof(SlotDisplay), "UnregisterEvents")]
        [HarmonyPostfix]
        // ReSharper disable once InconsistentNaming
        private static void UnregisterEvents_PostFix(SlotDisplay __instance)
        {
            if (!RegisteredLootViews.TryGetValue(__instance, out var action)) return;

            UIInputManager.OnShortcutInput -= action;
            RegisteredLootViews.Remove(__instance);
        }

        private static void OnShortcutInputHandle(SlotDisplay slotDisplay, UIInputEventData data, int shortcutIndex)
        {
            var hoveringField = AccessTools.Field(typeof(SlotDisplay), "hovering");
            if (hoveringField == null) return;

            var hovering = hoveringField.GetValue(slotDisplay) as bool?;
            if (hovering != true) return;

            if (slotDisplay.Target == null) return;
            if (!SlotManager.IsRegisteredSlot(slotDisplay.Target)) return; // Only handle registered slots

            var item = slotDisplay.Target.Content;
            if (item == null) return;

            ItemShortcut.Set(shortcutIndex, item);
            ItemUIUtilities.NotifyPutItem(item);
        }
    }
}