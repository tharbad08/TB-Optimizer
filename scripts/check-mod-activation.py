#!/usr/bin/env python3
"""Fail CI if a third-party optimization is registered without its availability gate.

This intentionally checks registration source only. It does not claim runtime
mod activation or prove the loaded mod is enabled by the game's mod manager.
"""
from pathlib import Path
import re

root = Path(__file__).resolve().parents[1]
starter = (root / "ModStarter.cs").read_text(encoding="utf-8")
availability = (root / "ModAvailability.cs").read_text(encoding="utf-8")
toggles = (root / "OptimizationToggles.cs").read_text(encoding="utf-8")

optional = {
    "BFR", "EBR", "FaunaBudget", "FaunaCache", "FaunaPrewarm",
    "SuperCursor", "PhysicsCatchup", "PhysicsCollider", "SWC",
}
defined = set(re.findall(r'new\\("([A-Za-z]+)"', toggles))
mapped = set(re.findall(r'\\["([A-Za-z]+)"\\] = new\\[\\]', availability))
registered = set(re.findall(r'ModAvailability\\.ShouldInstall\\("([A-Za-z]+)"\\)', starter))

errors = []
if not optional <= defined:
    errors.append(f"Missing optional UI keys: {optional - defined}")
if not optional <= mapped:
    errors.append(f"Missing optional detection mappings: {optional - mapped}")
if not optional <= registered:
    errors.append(f"Missing optional startup gates: {optional - registered}")

# All calls installing an optional mod patch must be controlled by a startup
# ShouldInstall guard. Allow no direct third-party patches in the startup file.
mod_patch_types = [
    "BfrLocalizedChangeOptimizerPatcher", "ExtendedBuilderReachNavOptimizer",
    "EbrDeferredDrainHook", "FaunaSpawnBudgetPatcher",
    "FaunaRecipeLookupCachePatcher", "KeystoneFaunaTemplatePrewarmer",
    "SuperCursorRefreshSmoother", "TimberPhysicsCatchUpLimiterPatcher",
    "TimberPhysicsTerrainColliderMergerPatcher", "SWCHarvestSafety",
    "SWCScanOptimizer", "SWCAllocationPatcher",
]
for name in mod_patch_types:
    if name not in starter:
        errors.append(f"Expected optional patch registration missing: {name}")

if re.search(r'OptimizationToggles\\.Enabled\\("(?:' + '|'.join(sorted(optional)) + r')"\\)', starter):
    errors.append("Optional patch bypasses loaded-mod detection")

if errors:
    raise SystemExit("\\n".join(errors))
print(f"Optional mod activation guards validated: {len(optional)} setting keys")
