using System;
using System.IO;
using Duckov_CashSlot.Data;
using Newtonsoft.Json;

namespace Duckov_CashSlot.Configs
{
    public sealed class CustomSlotSetting : ConfigBase
    {
        public CustomSlot[] CustomSlots { get; set; } = [];

        public override void LoadDefault()
        {
            CustomSlots =
            [
                new("Cash", ["Cash"], new(ShowIn.Pet, true, true)),
                new("Medic", ["Medic"], new(ShowIn.Pet, true, true)),
                new("Key", ["Key"], new(ShowIn.Pet, true, true)),
            ];
        }

        public override void LoadFromFile(string filePath)
        {
            try
            {
                ConfigManager.CreateDirectoryIfNotExists();

                if (!File.Exists(filePath))
                {
                    ModLogger.LogWarning($"Config file '{filePath}' does not exist. Loading default config.");
                    LoadDefault();
                    SaveToFile(filePath);
                    return;
                }

                var json = File.ReadAllText(filePath);
                var customSlots = JsonConvert.DeserializeObject<CustomSlot[]>(json, ConfigManager.JsonSettings);
                CustomSlots = customSlots ?? [];
                return;
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

            ModLogger.LogError("Loading default custom slot configuration due to errors.");
            LoadDefault();
            SaveToFile(filePath);
        }

        public override void SaveToFile(string filePath)
        {
            try
            {
                ConfigManager.CreateDirectoryIfNotExists();

                var json = JsonConvert.SerializeObject(CustomSlots, ConfigManager.JsonSettings);
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"Failed to save config to file '{filePath}': {ex.Message}");
            }
        }
    }
}