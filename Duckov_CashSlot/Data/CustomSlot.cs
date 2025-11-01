namespace Duckov_CashSlot.Data
{
    public class CustomSlot(string key, string[] requiredTags, SlotSettings settings)
    {
        public string Key { get; } = key;
        public string[] RequiredTags { get; } = requiredTags;
        public SlotSettings Settings { get; } = settings;
    }
}