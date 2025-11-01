using System;
using System.Collections.Generic;

namespace Duckov_CashSlot.Data
{
    public class CustomSlot(string key, string[] requiredTags, SlotSettings settings)
    {
        public string Key { get; private set; } = key;
        public string[] RequiredTags { get; } = requiredTags;
        public SlotSettings Settings { get; private set; } = settings;

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(Key)) Key = Guid.NewGuid().ToString();
            var validTags = new List<string>();
            foreach (var tag in RequiredTags)
            {
                var trimmedTag = tag.Trim();
                if (string.IsNullOrEmpty(trimmedTag)) continue;
                if (!TagManager.GetTagByName(trimmedTag)) continue;
                if (!validTags.Contains(trimmedTag))
                    validTags.Add(trimmedTag);
            }
        }
    }
}