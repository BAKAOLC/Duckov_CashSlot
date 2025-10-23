using HarmonyLib;

namespace Duckov_CashSlot.HarmonyPatches
{
    [HarmonyPatch(typeof(LevelManager), "InitLevel")]
    // ReSharper disable once InconsistentNaming
    internal class LevelManager_InitLevel
    {
        private static void Prefix()
        {
            ModLogger.Log("Applying slot registrations before level initialization...");
            SlotManager.ApplySlotRegistrations();
            ModLogger.Log("Applied slot registrations before level initialization.");
        }
    }
}