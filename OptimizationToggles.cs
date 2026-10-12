using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace BenchmarkAndOptimizerV11;

/// <summary>
/// Persistent switches evaluated BEFORE Harmony patches are installed.
/// Changes made in settings take effect on next full game start.
/// </summary>
internal static class OptimizationToggles
{
    internal sealed record Entry(string Key, string Label, string Description, string Group, bool DefaultEnabled = true);
    internal static readonly Entry[] Options =
    {
        new("PreviewNav", "Preview navigation batching", "Coalesces preview add/remove notifications.\nPreserves validation and navigation results.", "Base game"),
        new("PreviewPool", "Unused preview pool fast path", "Avoids creating previews for a brush\nthat was never displayed.", "Base game"),
        new("PreviewSweep", "Efficient preview hide sweep", "Removes repeated LINQ/HashSet allocations\nwhile keeping preview validation intact.", "Base game"),
        new("BlueprintCache", "Blueprint ComponentSpec cache", "Caches immutable blueprint component lookups\nused by construction and preview creation.", "Base game"),
        new("BulkBuild", "Faster bulk component creation", "Uses verified warmed ComponentSpecs.\nKeeps normal component ordering and hooks.", "Base game"),
        new("SoilReset", "Soil contamination reset", "Optimizes contamination cleanup work.\nDoes not throttle soil simulation ticks.", "Base game"),
        new("GcSmoother", "Incremental GC smoothing", "Schedules small GC slices when Unity supports it.\nNo effect if incremental GC is unavailable.", "Base game"),
        new("BFR", "BFR local navigation batching", "Groups sparse terrain changes in 8x8 areas.\nAvoids unnecessary large-area scans.", "Other mods"),
        new("EBR", "EBR spatial navigation updates", "Filters unaffected builders by terrain bounds.\nDefers large notification batches safely.", "Other mods"),
        new("FaunaBudget", "Keystone fauna spawn budget", "Limits expensive actual spawns to one\nper update without dropping queued spawns.", "Other mods"),
        new("FaunaCache", "Keystone fauna recipe cache", "Avoids repeated recipe searches\nwhen spawning fauna.", "Other mods"),
        new("FaunaPrewarm", "Keystone fauna prewarming", "Prewarms Cow, Bull and Deer templates\nafter game loading.", "Other mods"),
        new("SuperCursor", "SuperCursor update smoothing", "Reduces frequent informational refreshes.\nDoes not delay player tool actions.", "Other mods"),
        new("PhysicsCatchup", "TimberPhysics catch-up limiter", "Limits expensive accumulated physics substeps.\nMay drop excess catch-up under overload.", "Other mods"),
        new("PhysicsCollider", "TimberPhysics collider merging", "Combines terrain colliders in 32x32 chunks.\nUpdates changed chunks in batches.", "Other mods"),
        new("SWC", "SWC scans and harvest safety", "Optimizes SWC scans and protects workplaces.\nRuns only if a compatible SWC is loaded.", "Other mods"),
    };
    private static readonly Dictionary<string, bool> Values = new(StringComparer.Ordinal);
    private static string _path = "";

    internal static void Initialize(string root)
    {
        _path = Path.Combine(root, "tb-optimizer-options.json");
        Values.Clear();
        try
        {
            if (File.Exists(_path))
            {
                var data = JsonConvert.DeserializeObject<Dictionary<string,bool>>(File.ReadAllText(_path));
                if (data != null) foreach (var item in data) Values[item.Key] = item.Value;
            }
        }
        catch { /* Corrupt settings fail open to established optimizations. */ }
    }

    internal static bool Enabled(string key) => Values.TryGetValue(key, out var value) ? value :
        Array.Find(Options, item => item.Key == key)?.DefaultEnabled ?? false;
    internal static bool Configured(string key) => Values.ContainsKey(key);
    internal static void Set(string key, bool value)
    {
        Values[key] = value;
        try { if (_path.Length > 0) File.WriteAllText(_path,
            JsonConvert.SerializeObject(Values, Formatting.Indented)); }
        catch { /* Never throw from an options callback. */ }
    }
}
