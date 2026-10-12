using System;
using HarmonyLib;

namespace BenchmarkAndOptimizerV11;

// Samples managed heap infrequently, only in runtimes with incremental GC enabled.
// No global tick skipping; the original singleton update always executes.
internal static class IncrementalGcFrameHook
{
    private static int _frames;
    internal static bool Install(Harmony harmony)
    {
        var type = AccessTools.TypeByName("Timberborn.SingletonSystem.SingletonLifecycleService");
        var update = type is null ? null : AccessTools.Method(type, "UpdateSingletons");
        var callback = AccessTools.Method(typeof(IncrementalGcFrameHook), nameof(AfterUpdate));
        if (update is null || callback is null) return false;
        try { harmony.Patch(update, postfix: new HarmonyMethod(callback)); return true; }
        catch { return false; }
    }

    public static void AfterUpdate()
    {
        if (++_frames < 300) return;
        _frames = 0;
        IncrementalGcSmoother.CheckAndCollect();
    }
}
