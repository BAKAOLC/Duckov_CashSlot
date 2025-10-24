using System;
using System.Collections.Generic;
using System.IO;
using Duckov_CashSlot.Data;
using Duckov.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;

namespace Duckov_CashSlot
{
    public static class CustomSlotManager
    {
        private static readonly List<string> RegisteredCustomSlotKeys = [];

        private static readonly JsonSerializerSettings JsonSettings = new()
        {
            TypeNameHandling = TypeNameHandling.Auto,
            Formatting = Formatting.Indented,
            Converters = [new StringEnumConverter()],
        };

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
            var customSlots = File.Exists(ConfigFilePath)
                ? LoadConfigFromFile(ConfigFilePath)
                : CreateDefaultConfig();

            foreach (var customSlot in customSlots)
            {
                if (RegisteredCustomSlotKeys.Contains(customSlot.Key))
                {
                    ModLogger.LogWarning(
                        $"Custom slot with key '{customSlot.Key}' is already registered. Skipping duplicate.");
                    continue;
                }

                SlotManager.RegisterSlot(customSlot.Key, GetTagsByNames(customSlot.RequiredTags), customSlot.Settings);
                RegisteredCustomSlotKeys.Add(customSlot.Key);
                ModLogger.Log($"Registered custom slot with key '{customSlot.Key}'.");
            }
        }

        private static CustomSlot[] CreateDefaultConfig()
        {
            var defaultCustomSlots = new CustomSlot[]
            {
                new("Cash", ["Cash"], new(ShowIn.Pet, true, true)),
                new("Medic", ["Medic"], new(ShowIn.Pet, true, true)),
                new("Key", ["Key"], new(ShowIn.Pet, true, true)),
            };

            try
            {
                var json = JsonConvert.SerializeObject(defaultCustomSlots, JsonSettings);
                File.WriteAllText(ConfigFilePath, json);
            }
            catch (IOException e)
            {
                ModLogger.LogError($"Failed to create default custom slot configuration file: {e.Message}");
            }

            ModLogger.Log("Created default custom slot configuration.");

            return defaultCustomSlots;
        }

        private static CustomSlot[] LoadConfigFromFile(string filePath)
        {
            try
            {
                var json = File.ReadAllText(filePath);
                var customSlots = JsonConvert.DeserializeObject<CustomSlot[]>(json, JsonSettings);

                if (customSlots == null)
                {
                    ModLogger.LogError("Failed to deserialize custom slot configuration. Using empty configuration.");
                    return [];
                }

                ModLogger.Log("Loaded custom slot configuration from file.");
                return customSlots;
            }
            catch (IOException e)
            {
                ModLogger.LogError($"Failed to read custom slot configuration file: {e.Message}");
            }
            catch (JsonException e)
            {
                ModLogger.LogError($"Failed to parse custom slot configuration file: {e.Message}");
            }
            catch (Exception e)
            {
                ModLogger.LogError($"Unexpected error while loading custom slot configuration: {e.Message}");
            }

            return [];
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

        private class CustomSlot(string key, string[] requiredTags, SlotSettings settings)
        {
            public string Key { get; } = key;
            public string[] RequiredTags { get; } = requiredTags;
            public SlotSettings Settings { get; } = settings;
        }
    }
}