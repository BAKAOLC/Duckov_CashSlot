using Duckov.Utilities;
using HarmonyLib;
using ItemStatsSystem;

namespace Duckov_CashSlot.HarmonyPatches
{
    [HarmonyPatch(typeof(Item), nameof(Item.Initialize))]
    // ReSharper disable once InconsistentNaming
    internal class Item_Initialize
    {
        private const int KeyRingTypeID = 836;
        private static Tag? _keyTag;

        // ReSharper disable InconsistentNaming
        private static void Postfix(Item __instance)
            // ReSharper restore InconsistentNaming
        {
            if (__instance.TypeID != KeyRingTypeID) return;

            _keyTag ??= TagManager.GetTagByName("Key");
            if (_keyTag == null)
            {
                ModLogger.LogError("Failed to get 'Key' tag from TagManager.");
                return;
            }

            if (__instance.Tags.Contains(_keyTag)) return;

            __instance.Tags.Add(_keyTag);
            ModLogger.Log($"Added 'Key' tag to item '{__instance.DisplayName}' (TypeID: {__instance.TypeID}).");
        }
    }
}