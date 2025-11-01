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

        public override void LoadDefault()
        {
            InventorySlotDisplayRows = ModConstant.InventorySlotDisplayRows;
            PetSlotDisplayRows = ModConstant.PetSlotDisplayRows;
        }
    }
}