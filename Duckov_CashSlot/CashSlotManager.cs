using System.Linq;
using Duckov.Utilities;
using ItemStatsSystem;
using ItemStatsSystem.Items;
using SodaCraft.Localizations;
using UnityEngine;

namespace Duckov_CashSlot
{
    public static class CashSlotManager
    {
        private static Tag? _cachedCashTag;

        private static Tag? CashTag
        {
            get
            {
                _cachedCashTag ??= GameplayDataSettings.Tags.AllTags.FirstOrDefault(t => t.name == "Cash");
                return _cachedCashTag;
            }
        }

        public static bool IsInitialized { get; private set; }

        public static void Initialize()
        {
            if (IsInitialized) return;
            SetupLocalization();
            IsInitialized = true;

            ModLogger.Log("CashSlotManager initialized.");
        }

        public static void Uninitialize()
        {
            if (!IsInitialized) return;
            RemoveLocalization();
            IsInitialized = false;

            ModLogger.Log("CashSlotManager uninitialized.");
        }

        public static void AppendCashSlotToCharacter()
        {
            if (!IsInitialized)
            {
                ModLogger.LogError("CashSlotManager is not initialized!");
                return;
            }

            var slotCollection = Resources.FindObjectsOfTypeAll<SlotCollection>()
                .FirstOrDefault(x => x.name == "Character");
            InnerAppendCashSlotToSlotCollection(slotCollection);
        }

        public static bool IsCashSlot(Slot? slot)
        {
            if (!IsInitialized)
            {
                ModLogger.LogError("CashSlotManager is not initialized!");
                return false;
            }

            if (slot != null) return slot.Key == "Cash";

            ModLogger.LogError("Slot is null!");
            return false;
        }

        public static Slot? FindCashSlotInItem(Item? item)
        {
            if (!IsInitialized)
            {
                ModLogger.LogError("CashSlotManager is not initialized!");
                return null;
            }

            if (item == null)
            {
                ModLogger.LogError("Item is null!");
                return null;
            }

            var slotCollection = item.Slots;
            if (slotCollection != null) return slotCollection.GetSlot("Cash");

            ModLogger.LogError("Item's slot collection is null!");
            return null;
        }

        private static void SetupLocalization()
        {
            RemoveLocalization();
            LocalizationManager.OnSetLanguage += OnSetLanguage;

            var text = LocalizationManager.GetPlainText("Item_Cash");
            LocalizationManager.SetOverrideText("Tag_Cash", text);
        }

        private static void RemoveLocalization()
        {
            LocalizationManager.OnSetLanguage -= OnSetLanguage;

            LocalizationManager.RemoveOverrideText("Tag_Cash");
        }

        private static void OnSetLanguage(SystemLanguage language)
        {
            ModLogger.Log($"Language changed to {language}. Updating localization for 'Tag_Cash'.");

            var text = LocalizationManager.GetPlainText("Item_Cash");
            LocalizationManager.SetOverrideText("Tag_Cash", text);
        }

        private static void InnerAppendCashSlotToSlotCollection(SlotCollection? slotCollection)
        {
            if (slotCollection == null)
            {
                ModLogger.LogError("Slot collection is null!");
                return;
            }

            if (slotCollection.list.Any(s => s.Key == "Cash")) return;

            var slot = new Slot("Cash");
            var cashTag = CashTag;
            if (cashTag == null)
            {
                ModLogger.LogError("Cash tag not found!");
                return;
            }

            slot.requireTags.Add(cashTag);

            slot.Initialize(slotCollection);
            slotCollection.Add(slot);
        }
    }
}