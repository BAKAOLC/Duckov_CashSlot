namespace Duckov_CashSlot
{
    public static class ModEntry
    {
        public static void Initialize()
        {
            SlotManager.Initialize();
            CustomSlotManager.Initialize();
        }

        public static void Uninitialize()
        {
            CustomSlotManager.Uninitialize();
            SlotManager.ClearRegisteredSlots();
            SlotManager.Uninitialize();
        }
    }
}