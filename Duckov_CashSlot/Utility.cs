using System;
using System.Diagnostics;
using System.Linq;

namespace Duckov_CashSlot
{
    public static class Utility
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
    }
}