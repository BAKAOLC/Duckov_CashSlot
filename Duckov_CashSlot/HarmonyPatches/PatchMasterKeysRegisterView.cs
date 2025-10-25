using Duckov.MasterKeys.UI;
using HarmonyLib;
using ItemStatsSystem.Items;

namespace Duckov_CashSlot.HarmonyPatches
{
    [HarmonyPatch]
    internal class PatchMasterKeysRegisterView
    {
        [HarmonyPatch(typeof(MasterKeysRegisterView), "OnSlotContentChanged")]
        [HarmonyPrefix]
        private static bool MasterKeysRegisterView_OnSlotContentChanged_Prefix(Slot slot)
        {
            if (slot.Content == null) return true;
            if (slot.Content.TypeID != ModConstant.KeyRingTypeID) return true;

            var item = slot.Unplug();
            if (item == null) return true;

            if (!ItemUtilities.SendToPlayerCharacterInventory(item)) ItemUtilities.SendToPlayerStorage(item);
            ModLogger.Log(
                "Prevented placing Key Ring into Master Keys Register. Returned to player inventory/storage.");

            return false;
        }
    }
}