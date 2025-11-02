using System;
using System.Collections.Generic;

namespace Duckov_CashSlot.Data
{
    public class CustomSlot(string key, string[] requiredTags, SlotSettings settings)
    {
        public string Key { get; private set; } = key;
        public string[] RequiredTags { get; private set; } = requiredTags;
        public SlotSettings Settings { get; } = settings;

        public bool Validate()
        {
            Key ??= string.Empty;
            RequiredTags ??= [];

            var isChanged = false;

            if (string.IsNullOrWhiteSpace(Key))
            {
                Key = Guid.NewGuid().ToString();
                isChanged = true;
            }

            var validTags = new List<string>();
            foreach (var tag in RequiredTags)
            {
                var trimmedTag = tag.Trim();
                if (trimmedTag != tag) isChanged = true;
                if (string.IsNullOrEmpty(trimmedTag) ||
                    !TagManager.GetTagByName(trimmedTag) ||
                    validTags.Contains(trimmedTag))
                {
                    isChanged = true;
                    continue;
                }

                validTags.Add(trimmedTag);
            }

            return isChanged;
        }

        public CustomSlot Clone()
        {
            return new(Key, (string[])RequiredTags.Clone(), Settings);
        }
    }
}