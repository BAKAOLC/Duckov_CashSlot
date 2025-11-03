using Duckov.Utilities;

namespace Duckov_CashSlot.Data
{
    public class RegisteredSlot(string key, Tag[] requiredTags, Tag[] excludedTags, SlotSettings settings)
    {
        public string Key { get; } = key;
        public Tag[] RequiredTags { get; } = requiredTags;
        public Tag[] ExcludedTags { get; } = excludedTags;
        public SlotSettings Settings { get; } = settings;
    }
}