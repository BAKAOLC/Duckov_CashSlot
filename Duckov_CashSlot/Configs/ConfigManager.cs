﻿using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;

namespace Duckov_CashSlot.Configs
{
    public static class ConfigManager
    {
        public static readonly JsonSerializerSettings JsonSettings = new()
        {
            TypeNameHandling = TypeNameHandling.Auto,
            Formatting = Formatting.Indented,
            Converters = [new StringEnumConverter()],
        };

        public static string ConfigBaseDirectory => $"{Application.dataPath}/../ModConfigs/Duckov_CashSlot";

        public static void CreateDirectoryIfNotExists()
        {
            if (!Directory.Exists(ConfigBaseDirectory)) Directory.CreateDirectory(ConfigBaseDirectory);
        }

        public static T LoadConfigFromFile<T>(string configName) where T : ConfigBase
        {
            var configFilePath = Path.Combine(ConfigBaseDirectory, configName);
            var configInstance = Activator.CreateInstance<T>();
            configInstance.LoadFromFile(configFilePath);
            return configInstance;
        }

        public static void SaveConfigToFile<T>(T configInstance, string configName) where T : ConfigBase
        {
            var configFilePath = Path.Combine(ConfigBaseDirectory, configName);
            configInstance.SaveToFile(configFilePath);
        }

        public static void CreateBackupFile(string originalFilePath)
        {
            try
            {
                if (!File.Exists(originalFilePath)) return;
                var backupFilePath = $"{originalFilePath}.bak";
                File.Copy(originalFilePath, backupFilePath, true);
                ModLogger.Log($"Backup created at: {backupFilePath}");
            }
            catch (IOException e)
            {
                ModLogger.LogError($"Failed to create backup file: {e.Message}");
            }
        }
    }
}