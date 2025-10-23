using Duckov_CashSlot.Enums;

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
            RegisterSlotBySingleTag("Cash", ShowIn.Pet, true);
            RegisterSlotBySingleTag("Medic", ShowIn.Pet, true);
            RegisterSlotBySingleTag("Key", ShowIn.Pet, true);

            var cashTag = TagManager.GetTagByName("Cash");
            if (cashTag != null) SlotManager.RegisterTagLocalization(cashTag, "Item_Cash");
        }

        private static void RegisterSlotBySingleTag(string tagName, ShowIn showIn, bool forbidDeathDrop)
        {
            var tag = TagManager.GetTagByName(tagName);
            if (tag == null)
            {
                ModLogger.LogError($"{tagName} tag not found! Cannot register slot.");
                return;
            }

            SlotManager.RegisterSlot(tagName, [tag], showIn, forbidDeathDrop);
        }
    }
}