using System;
using System.Reflection;
using HarmonyLib;

namespace BenchmarkAndOptimizerV11;

/// <summary>
/// The Unity incremental GC capability is an engine setting, not a property of
/// an individual Timberborn save. Keep polling: a false value at the main menu
/// must not permanently suppress the optional in-game GC hook.
/// </summary>
internal static class IncrementalGcSmoother
{
    private const long StartHeapBytes = 5632L * 1024L * 1024L;
    private const long HighHeapBytes = 6144L * 1024L * 1024L;
    private const long VeryHighHeapBytes = 6656L * 1024L * 1024L;

    private const ulong NormalBudgetNanoseconds = 500_000UL;
    private const ulong HighBudgetNanoseconds = 1_000_000UL;
    private const ulong VeryHighBudgetNanoseconds = 2_000_000UL;

    private const BindingFlags StaticPublic = BindingFlags.Static | BindingFlags.Public;
    private static Func<ulong, bool>? _collectIncremental;
    private static Func<bool>? _isIncremental;
    // Set only after an actual runtime failure, NOT when isIncremental is false.
    private static bool _runtimeFailed;

    // API presence and the *current value* of isIncremental are separate facts.
    // Retry binding if an engine assembly was not yet available at startup.
    internal static bool IsSupported => EnsureBound();

    internal static bool IsIncrementalNow
    {
        get
        {
            if (_runtimeFailed || !EnsureBound()) return false;
            try { return _isIncremental!(); }
            catch { return false; }
        }
    }

    internal static string StatusDescription
    {
        get
        {
            if (_runtimeFailed)
                return "GC helper stopped after a runtime error; restart required.";
            if (!EnsureBound())
                return "Unity GC API not found yet; rechecked every 300 updates.";
            return IsIncrementalNow
                ? "Unity incremental GC enabled; heap threshold still applies."
                : "Unity incremental GC currently off; checking every 300 updates.";
        }
    }

    internal static void Initialize() => EnsureBound();

    private static bool EnsureBound()
    {
        if (_collectIncremental is not null && _isIncremental is not null)
            return true;

        try
        {
            var type = AccessTools.TypeByName("UnityEngine.Scripting.GarbageCollector");
            if (type is null) return false;

            var collect = type.GetMethod(
                "CollectIncremental", StaticPublic,
                binder: null, types: new[] { typeof(ulong) }, modifiers: null);
            if (collect is null || !collect.IsStatic || collect.ReturnType != typeof(bool))
                return false;

            // Unity versions have represented isIncremental as either a
            // property or a static field. Handle both without setting it.
            Func<bool>? enabled = null;
            var prop = type.GetProperty("isIncremental", StaticPublic);
            var getter = prop?.GetGetMethod();
            if (prop?.PropertyType == typeof(bool) && getter is { IsStatic: true })
                enabled = (Func<bool>)getter.CreateDelegate(typeof(Func<bool>));

            if (enabled is null)
            {
                var field = type.GetField("isIncremental", StaticPublic);
                if (field is { IsStatic: true } && field.FieldType == typeof(bool))
                    enabled = () => (bool)field.GetValue(null)!;
            }

            if (enabled is null) return false;

            _collectIncremental = (Func<ulong, bool>)collect.CreateDelegate(typeof(Func<ulong, bool>));
            _isIncremental = enabled;
            return true;
        }
        catch
        {
            // A failed reflection probe does not mean the engine cannot later
            // expose its API. Retry at the next 300-update checkpoint.
            return false;
        }
    }

    /// <summary>
    /// Called only every 300 game singleton-update callbacks. No heap sampling
    /// or GC work is done when incremental collection is currently unavailable.
    /// </summary>
    internal static void CheckAndCollect()
    {
        if (_runtimeFailed || !EnsureBound()) return;

        try
        {
            // Do not persist this false result; the next interval rechecks it.
            if (!_isIncremental!()) return;

            // This is *managed heap*, not total process RAM/VRAM.
            long heapBytes = GC.GetTotalMemory(false);
            if (heapBytes < StartHeapBytes) return;

            ulong budget = heapBytes >= VeryHighHeapBytes
                ? VeryHighBudgetNanoseconds
                : heapBytes >= HighHeapBytes
                    ? HighBudgetNanoseconds
                    : NormalBudgetNanoseconds;
            _collectIncremental!(budget);
        }
        catch (Exception)
        {
            // A genuine runtime failure is not retried indefinitely; the
            // explicit setting remains saved and normal Unity GC is unchanged.
            _runtimeFailed = true;
        }
    }
}
