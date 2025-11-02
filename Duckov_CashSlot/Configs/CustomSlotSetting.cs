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
            // ReSharper disable All
            var defaultSlotSetting = new SlotSettings(
                showIn: ShowIn.Pet,
                forbidDeathDrop: true,
                forbidWeightCalculation: true,
                forbidItemsWithSameID: false,
                enableModifier: true);
            // ReSharper restore All

            CustomSlots =
            [
                new("Cash", ["Cash"], defaultSlotSetting),
                new("Medic", ["Medic"], defaultSlotSetting),
                new("Key", ["Key"], defaultSlotSetting),
            ];
        }

        public override bool Validate()
        {
            var isChanged = false;
            CustomSlots ??= [];
            foreach (var slot in CustomSlots)
                if (slot.Validate())
                    isChanged = true;
            return isChanged;
        }

        public override void CopyFrom(IConfigBase other)
        {
            if (other is not CustomSlotSetting otherSetting) return;
            CustomSlots = new CustomSlot[otherSetting.CustomSlots.Length];
            for (var i = 0; i < otherSetting.CustomSlots.Length; i++)
                CustomSlots[i] = otherSetting.CustomSlots[i].Clone();
        }

        public override void LoadFromFile(string filePath, bool autoSaveOnLoad = true)
        {
            try
            {
                ConfigManager.CreateDirectoryIfNotExists();

                if (!File.Exists(filePath))
                {
                    ModLogger.LogWarning($"Config file '{filePath}' does not exist. Loading default config.");
                    LoadDefault();
                    if (autoSaveOnLoad) SaveToFile(filePath);
                    return;
                }

                var json = File.ReadAllText(filePath);
                var customSlots = JsonConvert.DeserializeObject<CustomSlot[]>(json, ConfigManager.JsonSettings);
                CustomSlots = customSlots ?? [];
                if (Validate() && autoSaveOnLoad) SaveToFile(filePath);
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
            if (autoSaveOnLoad) SaveToFile(filePath);
        }

        public override void SaveToFile(string filePath, bool withBackup = true)
        {
            try
            {
                ConfigManager.CreateDirectoryIfNotExists();

                if (withBackup && File.Exists(filePath)) ConfigManager.CreateBackupFile(filePath);

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