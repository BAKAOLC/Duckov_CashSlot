using System;
using System.Collections.Generic;
using System.Reflection;
using Duckov.Economy;
using Duckov.UI;
using HarmonyLib;
using ItemStatsSystem;
using ItemStatsSystem.Items;
using TMPro;

namespace Duckov_CashSlot
{
    [HarmonyPatch]
    public static class HarmonyPatches
    {
        // ReSharper disable InconsistentNaming
        [HarmonyPatch(typeof(LevelManager), "InitLevel")]
        [HarmonyPrefix]
        private static void LevelManager_InitLevel_Prefix()
        {
            CashSlotManager.AppendCashSlotToCharacter();
        }

        #region Compatibility handling for "Display Cash With Money" mod

        [HarmonyPatch(typeof(ItemUtilities), nameof(ItemUtilities.FindAllBelongsToPlayer))]
        [HarmonyPostfix]
        private static void ItemUtilities_FindAllBelongsToPlayer_Postfix(ref List<Item> __result,
            Predicate<Item> predicate)
        {
            var mainCharacter = LevelManager.Instance.MainCharacter;
            if (mainCharacter == null) return;

            var mainCharacterItem = mainCharacter.CharacterItem;
            var cashSlots = CashSlotManager.FindCashSlotInItem(mainCharacterItem);
            if (cashSlots == null) return;

            var cashSlotItem = cashSlots.Content;
            if (cashSlotItem == null) return;

            if (predicate(cashSlotItem))
                __result.Add(cashSlotItem);
        }

        [HarmonyPatch(typeof(MoneyDisplay), "OnEnable")]
        public class PatchMoneyDisplayOnEnable
        {
            public static readonly Dictionary<MoneyDisplay, Action<Item>> onSetStackCountCallbacks = [];

            public static readonly Dictionary<MoneyDisplay, Action<Slot>> onSlotContentChangedCallbacks = [];

            public static void Unregister(MoneyDisplay __instance)
            {
                if (onSetStackCountCallbacks.TryGetValue(__instance, out var onSetStackCount))
                {
                    var cashItemList = ItemUtilities.FindAllBelongsToPlayer(e =>
                        e != null && e.TypeID == EconomyManager.CashItemID);
                    foreach (var cashItem in cashItemList) cashItem.onSetStackCount -= onSetStackCount;

                    onSetStackCountCallbacks.Remove(__instance);
                }

                if (!onSlotContentChangedCallbacks.TryGetValue(__instance, out var onSlotContentChanged)) return;

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


                onSlotContentChangedCallbacks.Remove(__instance);
            }

            public static void Prefix(MoneyDisplay __instance)
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
                onSetStackCountCallbacks.Add(__instance, onSetStackCount);

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

                onSlotContentChangedCallbacks.Add(__instance, onSlotContentChanged);

                Refresh();
                return;

                void Refresh()
                {
                    if (!cashText.gameObject.activeInHierarchy) return;
                    cashText.text = EconomyManager.Cash.ToString("n0");
                }
            }
        }

        [HarmonyPatch(typeof(MoneyDisplay), "OnDestroy")]
        public class PatchMoneyDisplayOnDestroy
        {
            public static void Prefix(MoneyDisplay __instance)
            {
                PatchMoneyDisplayOnEnable.Unregister(__instance);
            }
        }
    }

    #endregion

    // ReSharper restore InconsistentNaming
}