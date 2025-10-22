using Duckov.UI;
using HarmonyLib;

namespace Duckov_CashSlot.HarmonyPatches
{
    [HarmonyPatch(typeof(MoneyDisplay), "OnDestroy")]
    // ReSharper disable once InconsistentNaming
    internal class MoneyDisplay_OnDestroy
    {
        // ReSharper disable once InconsistentNaming
        private static void Prefix(MoneyDisplay __instance)
        {
            MoneyDisplay_OnEnable.Unregister(__instance);
        }
    }
}