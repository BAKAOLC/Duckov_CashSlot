using System.Collections.Generic;
using Duckov_CashSlot.Configs;
using Duckov.Utilities;

namespace Duckov_CashSlot
{
    public static class CustomSlotManager
    {
        public const string ConfigName = "CustomSlots.json";

        private static readonly List<string> RegisteredCustomSlotKeys = [];
        private static CustomSlotSetting? _config;

        public static void Initialize()
        {
            Reload(true);
        }

        public static void Uninitialize()
        {
            foreach (var customSlotKey in RegisteredCustomSlotKeys)
            {
                SlotManager.UnregisterSlot(customSlotKey);
                ModLogger.Log($"Unregistered custom slot with key '{customSlotKey}'.");
            }

            RegisteredCustomSlotKeys.Clear();
        }

        public static void Reload(bool fromInitialize = false)
        {
            Uninitialize();

            LoadConfig();
        }

        private static void LoadConfig()
        {
            _config = ConfigManager.LoadConfigFromFile<CustomSlotSetting>(ConfigName);

            foreach (var customSlot in _config.CustomSlots)
            {
                if (RegisteredCustomSlotKeys.Contains(customSlot.Key))
                {
                    ModLogger.LogWarning(
                        $"Custom slot with key '{customSlot.Key}' is already registered. Skipping duplicate.");
                    continue;
                }

                SlotManager.RegisterSlot(
                    customSlot.Key,
                    GetTagsByNames(customSlot.RequiredTags),
                    GetTagsByNames(customSlot.ExcludedTags),
                    customSlot.Settings);
                RegisteredCustomSlotKeys.Add(customSlot.Key);
                ModLogger.Log($"Registered custom slot with key '{customSlot.Key}'.");
            }
        }

        private static Tag[] GetTagsByNames(string[] tagNames)
        {
            var tags = new List<Tag>();
            foreach (var tagName in tagNames)
            {
                var tag = TagManager.GetTagByName(tagName);
                if (tag != null) tags.Add(tag);
                else ModLogger.LogWarning($"Tag '{tagName}' not found while loading custom slot.");
            }

            return tags.ToArray();
        }
    }
}