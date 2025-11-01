﻿using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Duckov.UI;
using UnityEngine;

namespace Duckov_CashSlot
{
    internal static class Utility
    {
        internal static void PrintStackTrace()
        {
            try
            {
                var st = new StackTrace(1, false);
                var frames = st.GetFrames() ?? [];
                var lines = frames
                    .Select(f =>
                    {
                        var m = f.GetMethod();
                        var dt = m?.DeclaringType;
                        var tn = dt != null ? dt.FullName : "<null>";
                        return $"  at {tn}.{m?.Name}()";
                    });
                ModLogger.Log("Stack Trace:\n" + string.Join("\n", lines));
            }
            catch (Exception ex)
            {
                ModLogger.LogError("Failed to get stack trace: " + ex);
            }
        }

        internal static void SetSlotCollectionScrollable(
            ItemSlotCollectionDisplay slotCollectionDisplay,
            int showRows = 2)
        {
            var gridLayoutObject = slotCollectionDisplay.transform.Find("GridLayout");
            if (gridLayoutObject == null)
            {
                ModLogger.LogError("Failed to find GridLayout in SlotCollectionDisplay.");
                return;
            }
            
            if (!GetComponent<DynamicElementLayout>(gridLayoutObject.gameObject, out var dynamicElementLayout,
                    true))
            {
                ModLogger.LogError("Failed to add DynamicElementLayout to GridLayout.");
                return;
            }

            dynamicElementLayout.SetMaxRows(showRows);
        }

        internal static bool GetComponent<T>(
            GameObject obj,
            [NotNullWhen(true)] out T component,
            bool autoAddWhenMissing = false) where T : Component
        {
            component = obj.GetComponent<T>();
            if (component != null) return true;

            if (!autoAddWhenMissing) return false;
            component = obj.AddComponent<T>();

            return component != null;
        }
    }
}