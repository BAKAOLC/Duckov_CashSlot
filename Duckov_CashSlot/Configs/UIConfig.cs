using System;
using UnityEngine;

namespace Duckov_CashSlot.Configs
{
    public class UIConfig : ConfigBase
    {
        private static readonly Lazy<UIConfig> LazyInstance = new(() =>
            ConfigManager.LoadConfigFromFile<UIConfig>("UIConfig.json"));

        public static UIConfig Instance => LazyInstance.Value;

        public KeyCode ToggleKey { get; set; } = KeyCode.Equals;

        public override void LoadDefault()
        {
            ToggleKey = KeyCode.Equals;
        }

        public override bool Validate()
        {
            return false;
        }
    }
}