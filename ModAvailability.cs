using System;
using HarmonyLib;

namespace BenchmarkAndOptimizerV11;

// Check loaded runtime types, not installed Workshop directories. This is deliberately
// read-only and does not load, initialize or activate an optional mod.
internal static class ModAvailability
{
    private static readonly string[] Bfr = { "Calloatti.BeaversForReal.BFRManager" };
    private static readonly string[] Ebr = { "ExtendedBuilderReach.Components.ExtendedDemolishableAccessible" };
    private static readonly string[] Keystone = { "Keystone.Mod.Fauna.FaunaSpawnDrainer" };
    private static readonly string[] SuperCursor = { "SuperCursor.Services.SuperCursorTool" };
    private static readonly string[] Physics = { "TimberPhysics.Core.PhysicsSimulator", "TimberPhysics.Core.PhysicalObjectRegistry" };
    private static readonly string[] Swc = { "TonWolfe.SimpleWorkshopControl.SimpleWorkshopControler", "TonWolfe.SimpleWorkshopControl.PlantMonitor", "TonWolfe.SimpleWorkshopControl.PlantingControler" };

    internal static bool IsAvailable(string key)
    {
        string[]? names = key switch
        {
            "BFR" => Bfr, "EBR" => Ebr,
            "FaunaBudget" or "FaunaCache" or "FaunaPrewarm" => Keystone,
            "SuperCursor" => SuperCursor,
            "PhysicsCatchup" or "PhysicsCollider" => Physics,
            "SWC" => Swc,
            "GcSmoother" => null,
            _ => Array.Empty<string>()
        };
        if (key == "GcSmoother") return true; // Poll runtime GC mode rather than disabling the toggle.
        if (names is null) return false;
        foreach (var name in names)
            if (AccessTools.TypeByName(name) is null) return false;
        return true;
    }

    internal static bool ShouldInstall(string key) =>
        OptimizationToggles.Enabled(key) && IsAvailable(key);
}
