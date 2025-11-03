using System;
using System.Collections.Generic;

namespace Duckov_CashSlot.Data
{
    public class CustomSlot(string key, string[] requiredTags, string[] excludedTags, SlotSettings settings)
    {
        public string Key { get; private set; } = key;
        public string[] RequiredTags { get; private set; } = requiredTags;
        public string[] ExcludedTags { get; private set; } = excludedTags;
        public SlotSettings Settings { get; } = settings;

        public bool Validate()
        {
            Key ??= string.Empty;
            RequiredTags ??= [];
            ExcludedTags ??= [];

            var isChanged = false;

            if (string.IsNullOrWhiteSpace(Key))
            {
                Key = Guid.NewGuid().ToString();
                isChanged = true;
            }

            var validTags = ValidateTags(RequiredTags);
            if (validTags.Length != RequiredTags.Length)
                isChanged = true;
            RequiredTags = validTags;

            var validExcludedTags = ValidateTags(ExcludedTags);
            if (validExcludedTags.Length != ExcludedTags.Length)
                isChanged = true;
            ExcludedTags = validExcludedTags;

            return isChanged;
        }

        private static string[] ValidateTags(string[] tags)
        {
            var validTags = new List<string>();
            foreach (var tag in tags)
            {
                var trimmedTag = tag.Trim();
                if (string.IsNullOrEmpty(trimmedTag) ||
                    !TagManager.GetTagByName(trimmedTag) ||
                    validTags.Contains(trimmedTag))
                    continue;

                validTags.Add(trimmedTag);
            }

            return validTags.ToArray();
        }

        public CustomSlot Clone()
        {
            return new(Key, (string[])RequiredTags.Clone(), (string[])ExcludedTags.Clone(), Settings);
        }
    }
}