using HarmonyLib;
using Timberborn.ModManagerScene;

namespace BenchmarkAndOptimizerV11;

// TB Optimizer intentionally contains only gameplay-preserving feature patches.
// Interval throttling, benchmarking, CSV and freeze profiling are maintained
// exclusively by the BenchmarkAndOptimizerV11Port branch.
public sealed class ModStarter : IModStarter
{
    public void StartMod(IModEnvironment modEnvironment)
    {
        OptimizationToggles.Initialize(modEnvironment.ModPath);
        var harmony = new Harmony("shay.TBOptimizer");
        if (ModAvailability.ShouldInstall("BFR")) BfrLocalizedChangeOptimizerPatcher.Patch(harmony);
        if (ModAvailability.ShouldInstall("EBR") && EbrDeferredDrainHook.Install(harmony))
            ExtendedBuilderReachNavOptimizer.Patch(harmony);
        if (OptimizationToggles.Enabled("PreviewNav")) PreviewNavMeshBatcher.Patch(harmony);
        if (OptimizationToggles.Enabled("PreviewPool")) UnusedPreviewPoolFastPath.Patch(harmony);
        if (OptimizationToggles.Enabled("PreviewSweep")) FullyBuildablePreviewFastPath.Patch(harmony);
        if (OptimizationToggles.Enabled("BlueprintCache")) BlueprintSpecInstantiationCache.Patch(harmony);
        if (OptimizationToggles.Enabled("BulkBuild")) BulkComponentInstantiationFastPath.Patch(harmony);
        if (ModAvailability.ShouldInstall("FaunaBudget")) FaunaSpawnBudgetPatcher.Patch(harmony);
        if (ModAvailability.ShouldInstall("FaunaCache")) FaunaRecipeLookupCachePatcher.Patch(harmony);
        if (ModAvailability.ShouldInstall("FaunaPrewarm")) KeystoneFaunaTemplatePrewarmer.Patch(harmony);
        if (OptimizationToggles.Enabled("SoilReset")) SoilContaminationResetOptimizer.Patch(harmony);
        if (ModAvailability.ShouldInstall("SuperCursor")) SuperCursorRefreshSmoother.Patch(harmony);
        // SWC scanning must never run without its workplace/harvest safety guard.
        if (ModAvailability.ShouldInstall("SWC") && SWCHarvestSafety.Install(harmony))
        {
            SWCScanOptimizer.Install(harmony);
            SWCAllocationPatcher.Install(harmony);
        }
        if (ModAvailability.ShouldInstall("PhysicsCatchup")) TimberPhysicsCatchUpLimiterPatcher.Patch(harmony);
        if (ModAvailability.ShouldInstall("PhysicsCollider")) TimberPhysicsTerrainColliderMergerPatcher.Patch(harmony);
        if (ModAvailability.ShouldInstall("GcSmoother")) IncrementalGcFrameHook.Install(harmony);
    }
}

// Legacy optimizer modules still reference these harmless no-op diagnostic
// entry points. Nothing measures frames or creates diagnostic log files.
internal static class Runtime
{
    public static bool IsBenchmarking => false;
    public static bool Enabled => true;

    [System.Diagnostics.Conditional("OPTIMIZER_DIAGNOSTICS")]
    public static void Log(string message) { }
}

internal static class FreezeDetector
{
    [System.Diagnostics.Conditional("OPTIMIZER_DIAGNOSTICS")]
    public static void RecordBfrDetail(string name, long ticks) { }
    [System.Diagnostics.Conditional("OPTIMIZER_DIAGNOSTICS")]
    public static void RecordSoilDetail(string name, long ticks) { }
    [System.Diagnostics.Conditional("OPTIMIZER_DIAGNOSTICS")]
    public static void RecordNavigationDetail(string name, long ticks) { }
    [System.Diagnostics.Conditional("OPTIMIZER_DIAGNOSTICS")]
    public static void RecordInputDetail(string name, long ticks) { }
    [System.Diagnostics.Conditional("OPTIMIZER_DIAGNOSTICS")]
    public static void RecordTargetDetail(string name, long ticks) { }
    [System.Diagnostics.Conditional("OPTIMIZER_DIAGNOSTICS")]
    public static void RecordSystem(string name, long ticks) { }
}
