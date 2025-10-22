using HarmonyLib;

namespace Duckov_CashSlot.HarmonyPatches
{
    [HarmonyPatch(typeof(LevelManager), "InitLevel")]
    // ReSharper disable once InconsistentNaming
    internal class LevelManager_InitLevel
    {
        private static void Prefix()
        {
            CashSlotManager.AppendCashSlotToCharacter();
            ModLogger.Log("Appended cash slot to character definitions.");
        }
    }
}