using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using SodaCraft.Localizations;
using UnityEngine;

namespace Duckov_CashSlot
{
    public static class Localization
    {
        private static readonly object Sync = new();
        private static bool _initialized;

        private static Dictionary<string, string>? _currentLanguageDict;
        private static Dictionary<string, string>? _englishDict;

        private static SystemLanguage CurrentLanguage => LocalizationManager.CurrentLanguage;

        private static string LocalizationDirectory
        {
            get
            {
                var asmDir = Path.GetDirectoryName(typeof(ModLoader).Assembly.Location) ?? "";
                return Path.Combine(asmDir, "Localizations");
            }
        }

        private static Dictionary<string, string> DefaultEnglish => new()
        {
            { "window.title", "Duckov CashSlot Settings" },
            { "tab.rows", "Rows & Layout" },
            { "tab.slots", "Slots" },
            { "common.reset_default", "Reset to Default" },
            { "common.save", "Save" },
            { "common.cancel", "Cancel" },
            { "config.rows.saved", "Row settings saved" },
            { "config.key.saved", "Keybinding saved" },
            { "config.slots.saved", "Slot settings saved" },
            { "rows.inventory.max", "Max rows: Player inventory side slot list" },
            { "rows.pet.max", "Max rows: Pet side slot list" },
            { "rows.pet.columns", "Columns: Pet side slot list" },
            { "rows.pet.inv.columns", "Columns: Pet side backpack list" },
            { "rows.pet.position.title", "Pet-side slot list position" },
            { "rows.pet.position.inv.above", "Above Player Inventory" },
            { "rows.pet.position.inv.below", "Below Player Inventory" },
            { "rows.pet.position.pet.below", "Below Pet Icon" },
            {
                "rows.pet.hint.superpet.invalid",
                "Hint: This option has no effect when SuperPet Mod is present and the new Super Pet style adaptation is disabled."
            },
            { "rows.superpet.compact", "Enable NEW Super Pet style adaptation" },
            {
                "rows.superpet.allow_modify_others",
                "Allow this mod to modify pet backpack style modified by other mods"
            },
            { "keybinding.title", "UI Toggle Key" },
            { "keybinding.current", "Current Key: {0}" },
            { "keybinding.press_to_set", "Press the desired key..." },
            { "keybinding.detect", "Detect Key" },
            { "slots.add", "Add New Slot" },
            { "slots.cancel_all", "Cancel All Changes" },
            { "slots.reset_default", "Reset to Default" },
            { "slots.save", "Save" },
            { "slots.select_left_to_edit", "Select a slot on the left to view and edit" },
            { "slots.confirm", "OK" },
            { "slots.remove", "Delete this slot" },
            { "tags.select.placeholder", "Select tag..." },
            { "tags.invalid.prefix", "[Invalid] {0}" },
            { "tags.delete", "Delete" },
            { "tags.search.label", "Search:" },
            { "tags.search.clear", "Clear" },
            { "tags.search.no_result", "No matching tag found" },
            { "tags.close", "Close" },
            { "tags.none", "None" },
            { "tags.add", "Add Tag" },
            { "tags.clean_invalid", "Clean Invalid Tags" },
            { "tags.required.title", "Required Tags" },
            { "tags.excluded.title", "Excluded Tags" },
            { "tags.empty.required.desc", "Name will display as '?' but any item is allowed." },
            { "tags.empty.excluded.desc", "No items will be excluded." },
            { "list.empty", "The list is empty" },
            { "slot.key", "Slot Key:" },
            { "slot.name", "Slot Name:" },
            { "slot.settings.title", "Slot Settings:" },
            { "slot.display_position", "Display Position:" },
            { "slot.showin.character", "Character" },
            { "slot.showin.pet", "Pet" },
            { "slot.toggle.forbid_death_drop", "Forbid dropping on death" },
            { "slot.toggle.forbid_weight_calc", "Forbid weight calculation" },
            { "slot.toggle.forbid_same_id", "Forbid items with same ID" },
            { "slot.toggle.disable_modifier", "Disable item modifier stats (e.g., move speed, backpack capacity)" },
            { "quantity.current_value", "Current: {0}" },
            { "tags.type.required", "Required" },
            { "tags.type.excluded", "Excluded" },
            { "tags.clean.log.cleaned", "Cleaned {1} invalid tag(s) from {0} tags" },
            { "tags.clean.log.none", "No invalid tags to clean in {0} tags" },
            { "tags.clean.invalid_list", "Cleaned invalid tags: {0}" },
            { "tags.empty_tag", "<Empty Tag>" },
        };

        public static void Initialize()
        {
            if (_initialized) return;
            lock (Sync)
            {
                if (_initialized) return;

                try
                {
                    LocalizationManager.OnSetLanguage += OnLanguageChanged;

                    Directory.CreateDirectory(LocalizationDirectory);

                    LoadLanguageFiles();
                }
                catch (Exception ex)
                {
                    ModLogger.LogError($"Failed to initialize localization: {ex.Message}");
                    _currentLanguageDict = DefaultEnglish;
                    _englishDict = DefaultEnglish;
                }

                _initialized = true;
            }
        }

        public static void Uninitialize()
        {
            if (!_initialized) return;
            lock (Sync)
            {
                try
                {
                    LocalizationManager.OnSetLanguage -= OnLanguageChanged;
                }
                catch (Exception ex)
                {
                    ModLogger.LogWarning($"Error while unsubscribing localization events: {ex.Message}");
                }
                finally
                {
                    _initialized = false;
                    _currentLanguageDict = null;
                    _englishDict = null;
                }
            }
        }

        private static void OnLanguageChanged(SystemLanguage language)
        {
            lock (Sync)
            {
                LoadLanguageFiles();
            }
        }

        private static void LoadLanguageFiles()
        {
            try
            {
                var languageKey = GetLanguageKey();
                var languageFile = Path.Combine(LocalizationDirectory, $"{languageKey}.json");
                var englishFile = Path.Combine(LocalizationDirectory, "English.json");

                _englishDict = LoadLanguageFile(englishFile, DefaultEnglish);
                _currentLanguageDict = languageKey == "English"
                    ? _englishDict
                    : LoadLanguageFile(languageFile, _englishDict);
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"Failed to load language files: {ex.Message}");
                _currentLanguageDict = DefaultEnglish;
                _englishDict = DefaultEnglish;
            }
        }

        private static Dictionary<string, string> LoadLanguageFile(string filePath, Dictionary<string, string> fallback)
        {
            if (!File.Exists(filePath))
            {
                try
                {
                    var json = JsonConvert.SerializeObject(fallback, Formatting.Indented);
                    File.WriteAllText(filePath, json);
                    ModLogger.Log($"Created localization file: {filePath}");
                }
                catch (Exception ex)
                {
                    ModLogger.LogError($"Failed to create localization file {filePath}: {ex.Message}");
                }

                return fallback;
            }

            try
            {
                var json = File.ReadAllText(filePath);
                var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                if (dict == null)
                {
                    ModLogger.LogWarning($"Failed to parse localization file: {filePath}, using fallback.");
                    return fallback;
                }

                var result = new Dictionary<string, string>(fallback);
                foreach (var kvp in dict) result[kvp.Key] = kvp.Value;
                return result;
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"Failed to load localization file {filePath}: {ex.Message}");
                return fallback;
            }
        }

        private static string GetLanguageKey()
        {
            var language = CurrentLanguage;

            return language switch
            {
                SystemLanguage.Chinese or SystemLanguage.ChineseSimplified => "Chinese",
                SystemLanguage.ChineseTraditional => "ChineseTraditional",
                _ => language.ToString(),
            };
        }

        public static string Tr(string key)
        {
            Initialize();

            if (_currentLanguageDict != null && _currentLanguageDict.TryGetValue(key, out var text))
                return text;

            if (_englishDict != null && _englishDict.TryGetValue(key, out var englishText))
                return englishText;

            return $"<<{key}>>";
        }

        public static string TrFormat(string key, params object[] args)
        {
            var format = Tr(key);
            return string.Format(format, args);
        }
    }
}