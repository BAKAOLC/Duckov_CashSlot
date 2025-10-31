using System.Collections.Generic;
using System.Linq;
using Duckov.Utilities;
using ItemStatsSystem;

namespace Duckov_CashSlot
{
    public static class TagManager
    {
        private static readonly HashSet<Item> TrackedItems = [];
        private static Tag DontDropOnDeadTag => GameplayDataSettings.Tags.DontDropOnDeadInSlot;

        public static Tag? GetTagByName(string tagName)
        {
            return GameplayDataSettings.Tags.AllTags.FirstOrDefault(t => t.name == tagName);
        }

        public static void SetDontDropOnDeadTag(Item item)
        {
            if (item == null) return;
            if (item.Tags.Contains(DontDropOnDeadTag)) return;

            item.Tags.Add(DontDropOnDeadTag);
            TrackedItems.Add(item);

            ModLogger.Log($"Added 'DontDropOnDeadInSlot' tag to item '{item.TypeID}'.");
        }

        public static void CheckRemoveDontDropOnDeadTag(Item item)
        {
            if (item == null) return;
            if (!item.Tags.Contains(DontDropOnDeadTag)) return;
            if (!TrackedItems.Contains(item)) return;

            TrackedItems.Remove(item);
            item.Tags.Remove(DontDropOnDeadTag);

            ModLogger.Log($"Removed 'DontDropOnDeadInSlot' tag from item '{item.TypeID}'.");
        }
    }
}