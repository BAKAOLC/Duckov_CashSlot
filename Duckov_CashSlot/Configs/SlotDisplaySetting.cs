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
        public int PetInventoryDisplayColumns { get; set; } = ModConstant.PetInventoryDisplayColumns;
        
        public bool PetSlotDisplayAboveInventory { get; set; } // 宠物槽位栏显示在宠物物品栏上方
        public bool NewSuperPetDisplayCompact { get; set; } // 是否启用新版本兼容性样式显示
        public bool AllowModifyOtherModPetDisplay { get; set; } // 是否允许修改其他模组的宠物物品栏显示

        public override void LoadDefault()
        {
            InventorySlotDisplayRows = ModConstant.InventorySlotDisplayRows;
            PetSlotDisplayRows = ModConstant.PetSlotDisplayRows;
            PetSlotDisplayColumns = ModConstant.PetSlotDisplayColumns;
            PetInventoryDisplayColumns = ModConstant.PetInventoryDisplayColumns;
            PetSlotDisplayAboveInventory = false;
            NewSuperPetDisplayCompact = false;
            AllowModifyOtherModPetDisplay = false;
        }

        // ReSharper disable InvertIf
        public override bool Validate()
        {
            var isChanged = false;

            if (InventorySlotDisplayRows < 1)
            {
                ModLogger.LogWarning("InventorySlotDisplayRows is less than 1. Resetting to default.");
                InventorySlotDisplayRows = ModConstant.InventorySlotDisplayRows;
                isChanged = true;
            }

            if (PetSlotDisplayRows < 1)
            {
                ModLogger.LogWarning("PetSlotDisplayRows is less than 1. Resetting to default.");
                PetSlotDisplayRows = ModConstant.PetSlotDisplayRows;
                isChanged = true;
            }

            if (PetSlotDisplayColumns < 1)
            {
                ModLogger.LogWarning("PetSlotDisplayColumns is less than 1. Resetting to default.");
                PetSlotDisplayColumns = ModConstant.PetSlotDisplayColumns;
                isChanged = true;
            }

            if (PetInventoryDisplayColumns < 1)
            {
                ModLogger.LogWarning("PetInventoryDisplayColumns is less than 1. Resetting to default.");
                PetInventoryDisplayColumns = ModConstant.PetInventoryDisplayColumns;
                isChanged = true;
            }

            return isChanged;
        }
        // ReSharper restore InvertIf

        public override void CopyFrom(IConfigBase other)
        {
            if (other is not SlotDisplaySetting otherSetting) return;
            InventorySlotDisplayRows = otherSetting.InventorySlotDisplayRows;
            PetSlotDisplayRows = otherSetting.PetSlotDisplayRows;
            PetSlotDisplayColumns = otherSetting.PetSlotDisplayColumns;
            PetInventoryDisplayColumns = otherSetting.PetInventoryDisplayColumns;
            PetSlotDisplayAboveInventory = otherSetting.PetSlotDisplayAboveInventory;
            NewSuperPetDisplayCompact = otherSetting.NewSuperPetDisplayCompact;
            AllowModifyOtherModPetDisplay = otherSetting.AllowModifyOtherModPetDisplay;
        }
    }
}