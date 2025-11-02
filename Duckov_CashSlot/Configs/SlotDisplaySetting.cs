using System;

namespace Duckov_CashSlot.Configs
{
    public class SlotDisplaySetting : ConfigBase
    {
        private static readonly Lazy<SlotDisplaySetting> LazyInstance = new(() =>
            ConfigManager.LoadConfigFromFile<SlotDisplaySetting>("SlotDisplaySetting.json"));

        public static SlotDisplaySetting Instance => LazyInstance.Value;

        public int InventorySlotDisplayRows { get; set; } = ModConstant.InventorySlotDisplayRows;
        public int PetSlotDisplayRows { get; set; } = ModConstant.PetSlotDisplayRows;
        public int PetSlotDisplayColumns { get; set; } = ModConstant.PetSlotDisplayColumns;
        public bool NewSuperPetDisplayCompact { get; set; } = false;

        public override void LoadDefault()
        {
            InventorySlotDisplayRows = ModConstant.InventorySlotDisplayRows;
            PetSlotDisplayRows = ModConstant.PetSlotDisplayRows;
            PetSlotDisplayColumns = ModConstant.PetSlotDisplayColumns;
            NewSuperPetDisplayCompact = false;
        }

        public override void Validate()
        {
            if (InventorySlotDisplayRows < 1)
            {
                ModLogger.LogWarning("InventorySlotDisplayRows is less than 1. Resetting to default.");
                InventorySlotDisplayRows = ModConstant.InventorySlotDisplayRows;
            }

            if (PetSlotDisplayRows < 1)
            {
                ModLogger.LogWarning("PetSlotDisplayRows is less than 1. Resetting to default.");
                PetSlotDisplayRows = ModConstant.PetSlotDisplayRows;
            }

            // ReSharper disable once InvertIf
            if (PetSlotDisplayColumns < 1)
            {
                ModLogger.LogWarning("PetSlotDisplayColumns is less than 1. Resetting to default.");
                PetSlotDisplayColumns = ModConstant.PetSlotDisplayColumns;
            }
        }
    }
}