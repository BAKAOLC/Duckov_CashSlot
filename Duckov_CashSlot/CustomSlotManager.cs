using System;
using System.Collections.Generic;
using System.IO;
using Duckov_CashSlot.Data;
using Duckov.Utilities;
using UnityEngine;

namespace Duckov_CashSlot
{
    public static class CustomSlotManager
    {
        private static readonly List<string> RegisteredCustomSlotKeys = [];
        public static string ConfigBaseDirectory => $"{Application.dataPath}/../ModConfigs/Duckov_CashSlot";
        public static string ConfigFilePath => $"{ConfigBaseDirectory}/CustomSlots.json";

        public static void Initialize()
        {
            CreateDirectoryIfNotExists();

            Reload();
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

        public static void Reload()
        {
            Uninitialize();

            LoadConfig();
        }

        private static void LoadConfig()
        {
            var config = !File.Exists(ConfigFilePath)
                ? CreateDefaultConfig()
                : CustomSlotConfig.LoadFromFile(ConfigFilePath);

            foreach (var customSlot in config.customSlots)
            {
                if (RegisteredCustomSlotKeys.Contains(customSlot.key))
                {
                    ModLogger.LogWarning(
                        $"Custom slot with key '{customSlot.key}' is already registered. Skipping duplicate.");
                    continue;
                }

                SlotManager.RegisterSlot(customSlot.key, GetTagsByNames(customSlot.requiredTags), customSlot.settings);
                RegisteredCustomSlotKeys.Add(customSlot.key);
                ModLogger.Log($"Registered custom slot with key '{customSlot.key}'.");
            }
        }

        private static CustomSlotConfig CreateDefaultConfig()
        {
            var defaultCustomSlots = new CustomSlot[]
            {
                new("CustomCashSlot", ["Cash"], new(ShowIn.Pet, true, true)),
                new("CustomMedicSlot", ["Medic"], new(ShowIn.Pet, true, true)),
                new("CustomKeySlot", ["Key"], new(ShowIn.Pet, true, true)),
            };

            var customSlotConfig = new CustomSlotConfig(defaultCustomSlots);
            customSlotConfig.SaveToFile(ConfigFilePath);

            ModLogger.Log("Created default custom slot configuration.");

            return customSlotConfig;
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

        private static void CreateDirectoryIfNotExists()
        {
            if (!Directory.Exists(ConfigBaseDirectory)) Directory.CreateDirectory(ConfigBaseDirectory);
        }

        [Serializable]
        private class CustomSlotConfig(CustomSlot[] customSlots)
        {
            public CustomSlot[] customSlots = customSlots;

            public void SaveToFile(string filePath)
            {
                try
                {
                    var json = JsonUtility.ToJson(this, true);
                    File.WriteAllText(filePath, json);
                }
                catch (Exception e)
                {
                    ModLogger.LogError($"Failed to save custom slot configuration: {e.Message}");
                }
            }

            public static CustomSlotConfig LoadFromFile(string filePath)
            {
                if (!File.Exists(filePath)) return new([]);

                try
                {
                    var json = File.ReadAllText(filePath);
                    return JsonUtility.FromJson<CustomSlotConfig>(json) ?? new CustomSlotConfig([]);
                }
                catch (Exception e)
                {
                    ModLogger.LogError($"Failed to load custom slot configuration: {e.Message}");
                    return new([]);
                }
            }
        }

        [Serializable]
        private class CustomSlot(string key, string[] requiredTags, SlotSettings settings)
        {
            public string key = key;
            public string[] requiredTags = requiredTags;
            public SlotSettings settings = settings;
        }
    }
}