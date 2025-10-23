using System;
using System.Linq;
using System.Reflection;
using Duckov_CashSlot.Enums;
using Duckov.Utilities;
using HarmonyLib;

namespace Duckov_CashSlot
{
    public class ModBehaviour : Duckov.Modding.ModBehaviour
    {
        private Harmony? _harmony;

        private void Awake()
        {
            ModLogger.Log($"{Constant.ModName} Loaded");
        }

        private void OnEnable()
        {
            var patched = PatchAll();
            if (!patched) ModLogger.LogError("Failed to apply Harmony patches. Mod functionality may be impaired.");

            SlotManager.Initialize();
            RegisterSlot();
        }

        private void OnDisable()
        {
            SlotManager.ClearRegisteredSlots();
            SlotManager.Uninitialize();

            var unpatched = UnpatchAll();
            if (!unpatched) ModLogger.LogError("Failed to remove Harmony patches. Mod unloading may be impaired.");
        }

        private void OnDestroy()
        {
            SlotManager.Uninitialize();

            var unpatched = UnpatchAll();
            if (!unpatched)
                ModLogger.LogError("Failed to remove Harmony patches on destroy. Mod unloading may be impaired.");
        }

        private bool PatchAll()
        {
            try
            {
                _harmony = new(Constant.HarmonyId);
                _harmony.PatchAll(Assembly.GetExecutingAssembly());
                ModLogger.Log("Harmony Patches Applied Successfully");
                return true;
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"Error Applying Harmony Patches: {ex}");
                return false;
            }
        }

        private bool UnpatchAll()
        {
            try
            {
                if (_harmony == null) return true;
                _harmony.UnpatchAll(_harmony.Id);
                _harmony = null;
                ModLogger.Log("Harmony Patches Removed Successfully");
                return true;
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"Error Removing Harmony Patches: {ex}");
                return false;
            }
        }

        private static void RegisterSlot()
        {
            RegisterSlotBySingleTag("Cash", ShowIn.Pet, true);
            RegisterSlotBySingleTag("Medic", ShowIn.Pet, true);
            RegisterSlotBySingleTag("Key", ShowIn.Pet, true);

            var cashTag = GetTagByName("Cash");
            if (cashTag != null) SlotManager.RegisterTagLocalization(cashTag, "Item_Cash");
        }

        private static Tag? GetTagByName(string tagName)
        {
            return GameplayDataSettings.Tags.AllTags.FirstOrDefault(t => t.name == tagName);
        }

        private static void RegisterSlotBySingleTag(string tagName, ShowIn showIn, bool forbidDeathDrop)
        {
            var tag = GetTagByName(tagName);
            if (tag == null)
            {
                ModLogger.LogError($"{tagName} tag not found! Cannot register slot.");
                return;
            }

            SlotManager.RegisterSlot(tagName, [tag], showIn, forbidDeathDrop);
        }
    }
}