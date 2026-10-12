using System;
using System.Reflection;
using HarmonyLib;

namespace BenchmarkAndOptimizerV11;

/// <summary>
/// Version-sensitive optional inner-scan fast paths. Uses the installed SWC's
/// method signatures. If any binding differs, the ordinary method runs.
/// Full scan semantics, harvest callbacks and native Tick scheduling remain owned
/// by SWC. The allocation-reducing scanner is never installed on patched SWC.
/// </summary>
internal static class SWCAllocationPatcher
{
    private static SWCFastScanner.Harvest? _harvest;
    private static SWCFastScanner.Plant? _plant;
    private static bool _harvestFailed, _plantFailed;

    internal static void Install(Harmony harmony)
    {
        var hType = AccessTools.TypeByName("TonWolfe.SimpleWorkshopControl.PlantMonitor");
        var pType = AccessTools.TypeByName("TonWolfe.SimpleWorkshopControl.PlantingControler");
        if (hType is null || pType is null) return;
        if (AccessTools.DeclaredMethod(hType, "PerfRefreshGoods") is not null ||
            AccessTools.DeclaredMethod(pType, "PerfFindEmptySpot") is not null)
            return; // Old performance DLL already implements this optimization.
        var refresh = AccessTools.DeclaredMethod(hType, "RefreshGoods", Type.EmptyTypes);
        var emptySpot = AccessTools.DeclaredMethod(pType, "FindEmptySpot", Type.EmptyTypes);
        if (refresh?.ReturnType != typeof(bool) || emptySpot?.ReturnType != typeof(bool)) return;
        var hp = SWCFastScanner.CreateHarvest(hType);
        var pp = SWCFastScanner.CreatePlant(pType);
        if (hp is null || pp is null) return;
        _harvest = hp;
        _plant = pp;
        try
        {
            harmony.Patch(refresh, prefix: new HarmonyMethod(
                AccessTools.Method(typeof(SWCAllocationPatcher), nameof(HarvestPrefix))));
            harmony.Patch(emptySpot, prefix: new HarmonyMethod(
                AccessTools.Method(typeof(SWCAllocationPatcher), nameof(PlantPrefix))));
        }
        catch
        {
            harmony.Unpatch(refresh, HarmonyPatchType.All, harmony.Id);
            harmony.Unpatch(emptySpot, HarmonyPatchType.All, harmony.Id);
            _harvest = null;
            _plant = null;
        }
    }

    private static bool HarvestPrefix(object __instance, ref bool __result)
    {
        if (_harvestFailed || _harvest is null) return true;
        try
        {
            __result = _harvest.Scan(__instance);
            return false;
        }
        catch (Exception ex)
        {
            _harvestFailed = true; // One failure permanently restores native code.
            System.Console.WriteLine("[TB Optimizer] SWC harvest scanner fallback: " + ex.Message);
            return true;
        }
    }

    private static bool PlantPrefix(object __instance, ref bool __result)
    {
        if (_plantFailed || _plant is null) return true;
        try
        {
            __result = _plant.Scan(__instance);
            return false;
        }
        catch (Exception ex)
        {
            _plantFailed = true;
            System.Console.WriteLine("[TB Optimizer] SWC planting scanner fallback: " + ex.Message);
            return true;
        }
    }
}
