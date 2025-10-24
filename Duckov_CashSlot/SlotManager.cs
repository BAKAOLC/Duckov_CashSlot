using System.Collections.Generic;
using System.Linq;
using Duckov_CashSlot.Data;
using Duckov.Utilities;
using ItemStatsSystem;
using ItemStatsSystem.Items;
using SodaCraft.Localizations;
using UnityEngine;

namespace Duckov_CashSlot
{
    public static class SlotManager
    {
        public const int InventoryIndexBegin = -100000;
        private static readonly Dictionary<Tag, string> TagLocalizationKeys = new();
        private static readonly Dictionary<string, RegisteredSlot> RegisteredSlots = [];

        public static bool IsInitialized { get; private set; }

        public static void Initialize()
        {
            if (IsInitialized) return;
            SetupLocalization();
            IsInitialized = true;

            ModLogger.Log("SlotManager initialized.");
        }

        public static void Uninitialize()
        {
            if (!IsInitialized) return;
            RemoveLocalization();
            IsInitialized = false;

            ModLogger.Log("SlotManager uninitialized.");
        }

        public static bool IsSlotRegistered(string key)
        {
            if (IsInitialized) return RegisteredSlots.ContainsKey(key);

            ModLogger.LogError("SlotManager is not initialized!");
            return false;
        }

        public static void RegisterSlot(string key, Tag[] requiredTags, SlotSettings settings)
        {
            if (RegisteredSlots.TryGetValue(key, out _))
            {
                ModLogger.LogWarning($"Slot with key '{key}' is already registered. Overwriting.");
                RegisteredSlots.Remove(key);
            }

            RegisteredSlots.Add(key, new(key, requiredTags, settings));
            ModLogger.Log($"""
                           Registered slot with key '{key}'.
                           === Slot Settings ===
                           Required Tags: {string.Join(", ", requiredTags.Select(t => t.name))}
                           {settings}
                           """);
        }

        public static void UnregisterSlot(string key)
        {
            if (!RegisteredSlots.Remove(key))
            {
                ModLogger.LogWarning($"No slot with key '{key}' found to unregister.");
                return;
            }

            ModLogger.Log($"Unregistered slot with key '{key}'.");
        }

        public static void ClearRegisteredSlots()
        {
            RegisteredSlots.Clear();
            ModLogger.Log("Cleared all registered slots.");
        }

        public static void ApplySlotRegistrations()
        {
            if (!IsInitialized)
            {
                ModLogger.LogError("SlotManager is not initialized!");
                return;
            }

            ModLogger.Log("Applying slot registrations.");

            var slotCollection = FindCharacterSlotCollection();
            if (slotCollection == null)
            {
                ModLogger.LogError("Character SlotCollection not found!");
                return;
            }

            InnerApplySlotRegistrationsToSlotCollection(slotCollection);

            ModLogger.Log("Slot registrations applied.");
        }

        public static Slot[] GetAllRegisteredSlotsInItem(Item item)
        {
            if (!IsInitialized)
            {
                ModLogger.LogError("SlotManager is not initialized!");
                return [];
            }

            if (item == null)
            {
                ModLogger.LogWarning("Item is null.");
                return [];
            }

            if (item.Slots != null)
                return item.Slots.list.Where(slot => RegisteredSlots.ContainsKey(slot.Key)).ToArray();

            ModLogger.LogWarning("Item has no SlotCollection.");
            return [];
        }

        public static bool IsRegisteredSlot(Slot slot)
        {
            var key = slot.Key;
            return IsInitialized && RegisteredSlots.ContainsKey(key);
        }

        public static SlotSettings? GetRegisteredSlotSettings(Slot slot)
        {
            var key = slot.Key;
            if (!IsInitialized) return null;

            return RegisteredSlots.TryGetValue(key, out var registeredSlot)
                ? registeredSlot.Settings
                : null;
        }

        public static ShowIn GetSlotShowIn(Slot slot)
        {
            var key = slot.Key;
            if (!IsInitialized) return ShowIn.Character;

            return RegisteredSlots.TryGetValue(key, out var registeredSlot)
                ? registeredSlot.Settings.ShowIn
                : ShowIn.Character;
        }

        public static bool IsSlotForbidDeathDrop(Slot slot)
        {
            var key = slot.Key;
            if (!IsInitialized) return false;

            return RegisteredSlots.TryGetValue(key, out var registeredSlot)
                   && registeredSlot.Settings.ForbidDeathDrop;
        }

        public static bool IsSlotForbidWeightCalculation(Slot slot)
        {
            var key = slot.Key;
            if (!IsInitialized) return false;

            return RegisteredSlots.TryGetValue(key, out var registeredSlot)
                   && registeredSlot.Settings.ForbidWeightCalculation;
        }

        public static int GetSlotInventoryIndex(Slot slot)
        {
            if (!IsInitialized) return -1;

            var master = slot.Master;
            if (master == null) return -1;

            var index = master.Slots.list.FindIndex(s => s == slot);
            if (index < 0) return -1;

            return InventoryIndexBegin - (index + 1);
        }

        public static Slot? GetSlotByInventoryIndex(Item item, int inventoryIndex)
        {
            if (!IsInitialized) return null;

            if (item == null) return null;
            if (item.Slots == null) return null;
            if (inventoryIndex >= InventoryIndexBegin) return null;

            var index = InventoryIndexBegin - inventoryIndex - 1;
            return index >= item.Slots.list.Count ? null : item.Slots.list[index];
        }

        public static void RegisterTagLocalization(Tag tag, string localizationKey)
        {
            if (!IsInitialized)
            {
                ModLogger.LogError("SlotManager is not initialized!");
                return;
            }

            if (TagLocalizationKeys.ContainsKey(tag))
                ModLogger.LogWarning($"Tag '{tag.name}' is already registered for localization. Overwriting.");

            TagLocalizationKeys[tag] = localizationKey;

            // Immediately set the localization for the current language
            var text = LocalizationManager.GetPlainText(localizationKey);
            LocalizationManager.SetOverrideText($"Tag_{tag.name}", text);
        }

        private static SlotCollection? FindCharacterSlotCollection()
        {
            var slotCollection = Resources.FindObjectsOfTypeAll<SlotCollection>()
                .FirstOrDefault(x => x.name == "Character");
            return slotCollection;
        }

        private static void InnerApplySlotRegistrationsToSlotCollection(SlotCollection slotCollection)
        {
            if (slotCollection == null)
            {
                ModLogger.LogError("SlotCollection is null!");
                return;
            }

            foreach (var (_, registeredSlot) in RegisteredSlots)
            {
                if (slotCollection.GetSlot(registeredSlot.Key) != null)
                {
                    ModLogger.LogWarning(
                        $"Slot with key '{registeredSlot.Key}' already exists in SlotCollection. Skipping.");
                    continue;
                }

                var slot = new Slot(registeredSlot.Key);
                slot.requireTags.AddRange(registeredSlot.RequiredTags);

                slot.Initialize(slotCollection);
                slotCollection.Add(slot);
            }
        }

        private static void SetupLocalization()
        {
            RemoveLocalization();

            LocalizationManager.OnSetLanguage += OnSetLanguage;

            OnSetLanguage(LocalizationManager.CurrentLanguage);
        }

        private static void RemoveLocalization()
        {
            LocalizationManager.OnSetLanguage -= OnSetLanguage;

            foreach (var (tag, _) in TagLocalizationKeys) LocalizationManager.RemoveOverrideText($"Tag_{tag.name}");
        }

        private static void OnSetLanguage(SystemLanguage language)
        {
            ModLogger.Log($"Language changed to {language}. Updating localization.");

            foreach (var (tag, key) in TagLocalizationKeys)
            {
                var text = LocalizationManager.GetPlainText(key);
                LocalizationManager.SetOverrideText($"Tag_{tag.name}", text);
            }
        }
    }
}