using System;
using System.Collections.Generic;
using System.Reflection;
using Duckov.Economy;
using Duckov.UI;
using HarmonyLib;
using ItemStatsSystem;
using ItemStatsSystem.Items;
using TMPro;

namespace Duckov_CashSlot.HarmonyPatches
{
    [HarmonyPatch(typeof(MoneyDisplay), "OnEnable")]
    // ReSharper disable once InconsistentNaming
    internal class MoneyDisplay_OnEnable
    {
        private static readonly Dictionary<MoneyDisplay, Action<Item>> OnSetStackCountCallbacks = [];

        private static readonly Dictionary<MoneyDisplay, Action<Slot>>
            OnSlotContentChangedCallbacks = [];

        // ReSharper disable once InconsistentNaming
        internal static void Unregister(MoneyDisplay __instance)
        {
            if (OnSetStackCountCallbacks.TryGetValue(__instance, out var onSetStackCount))
            {
                var cashItemList = ItemUtilities.FindAllBelongsToPlayer(e =>
                    e != null && e.TypeID == EconomyManager.CashItemID);
                foreach (var cashItem in cashItemList) cashItem.onSetStackCount -= onSetStackCount;

                OnSetStackCountCallbacks.Remove(__instance);
            }

            if (!OnSlotContentChangedCallbacks.TryGetValue(__instance, out var onSlotContentChanged)) return;

            var characterItem = LevelManager.Instance?.MainCharacter?.CharacterItem;
            var characterItemSlots = characterItem?.Slots;
            if (characterItemSlots != null)
                foreach (var slot in characterItemSlots)
                {
                    slot.onSlotContentChanged -= onSlotContentChanged;
                    var slotContent = slot.Content;
                    if (slotContent != null && slotContent.TypeID == EconomyManager.CashItemID)
                        slotContent.onSetStackCount -= onSetStackCount;
                }


            OnSlotContentChangedCallbacks.Remove(__instance);
        }

        // ReSharper disable once InconsistentNaming
        private static void Prefix(MoneyDisplay __instance)
        {
            Unregister(__instance);

            var text = typeof(MoneyDisplay)
                .GetField("text", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.GetValue(__instance) as TextMeshProUGUI;
            if (text == null) return;

            var cashTextTransform = text.transform.parent.Find("CashText");
            if (cashTextTransform == null) return;

            var cashText = cashTextTransform.GetComponent<TextMeshProUGUI>();

            Action<Item> onSetStackCount = item =>
            {
                if (item != null && item.TypeID == EconomyManager.CashItemID) Refresh();
            };

            Action<Slot> onSlotContentChanged = slot =>
            {
                Refresh();
                if (slot.Content == null) return;
                if (slot.Content.TypeID == EconomyManager.CashItemID)
                    slot.Content.onSetStackCount += onSetStackCount;
                else
                    slot.Content.onSetStackCount -= onSetStackCount;
            };

            var cashItemList =
                ItemUtilities.FindAllBelongsToPlayer(e => e != null && e.TypeID == EconomyManager.CashItemID);
            foreach (var cashItem in cashItemList) cashItem.onSetStackCount += onSetStackCount;
            OnSetStackCountCallbacks.Add(__instance, onSetStackCount);

            var characterItem = LevelManager.Instance?.MainCharacter?.CharacterItem;

            var characterItemSlots = characterItem?.Slots;
            if (characterItemSlots != null)
                foreach (var slot in characterItemSlots)
                {
                    slot.onSlotContentChanged += onSlotContentChanged;
                    var slotContent = slot.Content;
                    if (slotContent == null) continue;
                    if (slotContent.TypeID == EconomyManager.CashItemID)
                        slotContent.onSetStackCount += onSetStackCount;
                    else
                        slotContent.onSetStackCount -= onSetStackCount;
                }

            OnSlotContentChangedCallbacks.Add(__instance, onSlotContentChanged);

            Refresh();
            return;

            void Refresh()
            {
                if (!cashText.gameObject.activeInHierarchy) return;
                cashText.text = EconomyManager.Cash.ToString("n0");
            }
        }
    }
}