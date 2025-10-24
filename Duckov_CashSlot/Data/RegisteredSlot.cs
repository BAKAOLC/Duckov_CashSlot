using Duckov.Utilities;

namespace Duckov_CashSlot.Data
{
    public class RegisteredSlot(string key, Tag[] requiredTags, SlotSettings settings)
    {
        public string Key { get; } = key;
        public Tag[] RequiredTags { get; } = requiredTags;
        public SlotSettings Settings { get; } = settings;
    }
}