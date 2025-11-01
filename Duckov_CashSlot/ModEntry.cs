using Duckov_CashSlot.Configs;

namespace Duckov_CashSlot
{
    public static class ModEntry
    {
        public static void Initialize()
        {
            SlotManager.Initialize();
            CustomSlotManager.Initialize();
            _ = SlotDisplaySetting.Instance; // Ensure SlotDisplaySetting is loaded

            var cashTag = TagManager.GetTagByName("Cash");
            if (cashTag != null) SlotManager.RegisterTagLocalization(cashTag, "Item_Cash");
        }

        public static void Uninitialize()
        {
            CustomSlotManager.Uninitialize();
            SlotManager.ClearRegisteredSlots();
            SlotManager.Uninitialize();
        }
    }
}