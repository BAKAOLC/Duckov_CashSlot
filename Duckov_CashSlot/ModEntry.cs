using Duckov_CashSlot.Configs;
using Duckov_CashSlot.UI;
using UnityEngine;

namespace Duckov_CashSlot
{
    public static class ModEntry
    {
        private static GameObject? _configUIGameObject;

        public static void Initialize()
        {
            SlotManager.Initialize();
            CustomSlotManager.Initialize();
            _ = SlotDisplaySetting.Instance; // Ensure SlotDisplaySetting is loaded
            _ = UIConfig.Instance; // Ensure UIConfig is loaded

            var cashTag = TagManager.GetTagByName("Cash");
            if (cashTag != null) SlotManager.RegisterTagLocalization(cashTag, "Item_Cash");

            InitializeConfigUI();
        }

        public static void Uninitialize()
        {
            var allConfigUIs = Object.FindObjectsByType<ConfigUI>(FindObjectsSortMode.None);
            foreach (var configUI in allConfigUIs)
                if (configUI != null)
                {
                    configUI.enabled = false;
                    Object.DestroyImmediate(configUI.gameObject);
                }

            if (_configUIGameObject != null)
            {
                Object.DestroyImmediate(_configUIGameObject);
                _configUIGameObject = null;
            }

            CustomSlotManager.Uninitialize();
            SlotManager.ClearRegisteredSlots();
            SlotManager.Uninitialize();
        }

        private static void InitializeConfigUI()
        {
            if (ModBehaviour.Instance == null)
            {
                ModLogger.LogWarning("ModBehaviour.Instance is null. Cannot initialize ConfigUI.");
                return;
            }

            _configUIGameObject = new("Duckov_CashSlot_ConfigUI");
            _configUIGameObject.AddComponent<ConfigUI>();
            Object.DontDestroyOnLoad(_configUIGameObject);

            ModLogger.Log("ConfigUI initialized.");
        }
    }
}