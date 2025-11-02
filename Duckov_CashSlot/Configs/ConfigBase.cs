using System;
using System.IO;
using Newtonsoft.Json;

namespace Duckov_CashSlot.Configs
{
    public abstract class ConfigBase : IConfigBase
    {
        // ReSharper disable once MemberCanBeProtected.Global
        public abstract void LoadDefault();

        public abstract void Validate();

        public virtual void LoadFromFile(string filePath, bool autoSaveOnLoad = true)
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
                JsonConvert.PopulateObject(json, this, ConfigManager.JsonSettings);
                Validate();
                if (autoSaveOnLoad) SaveToFile(filePath);
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"Failed to load config from file '{filePath}': {ex.Message}");
                LoadDefault();
                if (autoSaveOnLoad) SaveToFile(filePath);
            }
        }

        public virtual void SaveToFile(string filePath, bool withBackup = true)
        {
            try
            {
                ConfigManager.CreateDirectoryIfNotExists();

                if (withBackup && File.Exists(filePath)) ConfigManager.CreateBackupFile(filePath);

                var json = JsonConvert.SerializeObject(this, ConfigManager.JsonSettings);
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"Failed to save config to file '{filePath}': {ex.Message}");
            }
        }
    }
}