namespace Duckov_CashSlot
{
    public static class ModEntry
    {
        public static void Initialize()
        {
            SlotManager.Initialize();
            CustomSlotManager.Initialize();

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