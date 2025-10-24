using Duckov_CashSlot.Data;

namespace Duckov_CashSlot
{
    public static class ModEntry
    {
        public static void Initialize()
        {
            SlotManager.Initialize();
            RegisterSlot();
        }

        public static void Uninitialize()
        {
            SlotManager.ClearRegisteredSlots();
            SlotManager.Uninitialize();
        }

        private static void RegisterSlot()
        {
            var slotSettings = new SlotSettings(ShowIn.Pet, true, true);
            RegisterSlotBySingleTag("Cash", slotSettings);
            RegisterSlotBySingleTag("Medic", slotSettings);
            RegisterSlotBySingleTag("Key", slotSettings);

            var cashTag = TagManager.GetTagByName("Cash");
            if (cashTag != null) SlotManager.RegisterTagLocalization(cashTag, "Item_Cash");
        }

        private static void RegisterSlotBySingleTag(string tagName, SlotSettings settings)
        {
            var tag = TagManager.GetTagByName(tagName);
            if (tag == null)
            {
                ModLogger.LogError($"{tagName} tag not found! Cannot register slot.");
                return;
            }

            SlotManager.RegisterSlot(tagName, [tag], settings);
        }
    }
}