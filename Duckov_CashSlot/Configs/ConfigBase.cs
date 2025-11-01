using System;
using System.IO;
using Newtonsoft.Json;

namespace Duckov_CashSlot.Configs
{
    public abstract class ConfigBase
    {
        // ReSharper disable once MemberCanBeProtected.Global
        public abstract void LoadDefault();

        public virtual void LoadFromFile(string filePath)
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
                JsonConvert.PopulateObject(json, this, ConfigManager.JsonSettings);
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"Failed to load config from file '{filePath}': {ex.Message}");
                LoadDefault();
                SaveToFile(filePath);
            }
        }

        public virtual void SaveToFile(string filePath)
        {
            try
            {
                ConfigManager.CreateDirectoryIfNotExists();

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