using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Timberborn.Modding;

namespace BenchmarkAndOptimizerV11;

/// <summary>
/// Determines which optional mod patches may be INSTALLED. This scans only
/// assemblies already loaded in the current game process. It never loads an
/// assembly, scans Workshop folders, or attempts to activate another mod.
///
/// A loaded type is a conservative runtime marker, not proof that every method
/// in a future mod version is compatible. Individual patches must still
/// validate their targets before modifying them.
/// </summary>
internal static class ModAvailability
{
    // Every third-party setting key MUST appear here. Keys not listed in
    // either this map or BaseGameKeys fail closed, never default to "available".
    // All listed types are required for the corresponding patch.
    private static readonly IReadOnlyDictionary<string, string[]> RequiredModTypes =
        new Dictionary<string, string[]>(StringComparer.Ordinal)
        {
            ["BFR"] = new[] { "Calloatti.BeaversForReal.BFRManager" },
            ["EBR"] = new[] { "ExtendedBuilderReach.Components.ExtendedDemolishableAccessible" },
            ["FaunaBudget"] = new[] { "Keystone.Mod.Fauna.FaunaSpawnDrainer" },
            ["FaunaCache"] = new[] { "Keystone.Mod.Fauna.FaunaSpawnDrainer" },
            ["FaunaPrewarm"] = new[] { "Keystone.Mod.Fauna.FaunaSpawnDrainer" },
            ["SuperCursor"] = new[] { "SuperCursor.Services.SuperCursorTool" },
            ["PhysicsCatchup"] = new[]
            {
                "TimberPhysics.Core.PhysicsSimulator",
                "TimberPhysics.Core.PhysicalObjectRegistry"
            },
            ["PhysicsCollider"] = new[]
            {
                "TimberPhysics.Terrain.TerrainColliderService",
                "TimberPhysics.Core.PhysicsSimulator"
            },
            ["SWC"] = new[]
            {
                "TonWolfe.SimpleWorkshopControl.SimpleWorkshopControler",
                "TonWolfe.SimpleWorkshopControl.PlantMonitor",
                "TonWolfe.SimpleWorkshopControl.PlantingControler"
            },
        };

    // Names from the corresponding Workshop mod manifests, not guesses based
    // on DLL/type names. Mod Settings can use the game's enabled-mod registry
    // rather than assuming that a loaded assembly implies an enabled mod.
    private static readonly IReadOnlyDictionary<string, string> RequiredModIds =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["BFR"] = "Calloatti.BeaversForReal",
            ["EBR"] = "ExtendedBuilderReach",
            ["FaunaBudget"] = "SylvanGames.Keystone",
            ["FaunaCache"] = "SylvanGames.Keystone",
            ["FaunaPrewarm"] = "SylvanGames.Keystone",
            ["SuperCursor"] = "SuperCursor",
            ["PhysicsCatchup"] = "eMka.TimberPhysics",
            ["PhysicsCollider"] = "eMka.TimberPhysics",
            ["SWC"] = "TonWolfe.SimpleWorkshopControl"
        };

    internal static bool IsAvailableInSettings(string key, ModRepository repository)
    {
        if (!IsAvailable(key)) return false;
        if (BaseGameKeys.Contains(key)) return true;
        if (!RequiredModIds.TryGetValue(key, out var requiredId)) return false;
        try
        {
            return repository.EnabledMods.Any(mod => mod.Manifest.Id == requiredId);
        }
        catch
        {
            return false;
        }
    }

    private static readonly HashSet<string> BaseGameKeys = new(StringComparer.Ordinal)
    {
        "PreviewNav", "PreviewPool", "PreviewSweep", "BlueprintCache",
        "BulkBuild", "SoilReset", "GcSmoother"
    };

    internal static bool IsOptionalModFeature(string key) =>
        RequiredModTypes.ContainsKey(key);

    internal static bool IsAvailable(string key)
    {
        if (BaseGameKeys.Contains(key)) return true;
        if (!RequiredModTypes.TryGetValue(key, out var names)) return false;
        try
        {
            Assembly[] loaded = AppDomain.CurrentDomain.GetAssemblies();
            foreach (string fullName in names)
            {
                bool found = false;
                foreach (Assembly assembly in loaded)
                {
                    if (assembly.GetType(fullName, throwOnError: false, ignoreCase: false) is null)
                        continue;
                    found = true;
                    break;
                }
                if (!found) return false;
            }
            return true;
        }
        catch
        {
            // Any lookup failure must NOT allow a third-party patch to arm.
            return false;
        }
    }

    internal static bool ShouldInstall(string key) =>
        OptimizationToggles.Enabled(key) && IsAvailable(key);
}
