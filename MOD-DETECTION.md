# Optional-mod detection and patch installation

TB Optimizer never requires BFR, Extended Builder Reach, Keystone, SuperCursor, TimberPhysics or Simple Workshop Control to be installed. None appears in `manifest.json` as a required dependency.

## Installation policy

1. At mod startup `OptimizationToggles.Initialize` loads the stored preferences from `tb-optimizer-options.json`. Missing preferences default to **on**. An explicit unchecked/false value always wins.
2. `ModAvailability.ShouldInstall(key)` requires both an enabled preference **and** positive detection of **all** required mod-specific types in assemblies **already loaded in the process**. It never loads DLLs or scans Steam Workshop folders. Unknown keys fail closed.
3. If any expected type is missing, that optional patch **does not install at all**. This includes its hooks on shared/base-game methods: EBR must not attach its frame drain, Keystone must not hook game startup for fauna prewarming, and SWC must not attach its global behavior safety guard.
4. If a mod is detected, the patcher must *also* check its required methods, signatures and IL where applicable, before installing its Harmony hooks. Presence of a loaded class **does not prove the mod is enabled by the Timberborn mod manager**, or compatible with every future release. Treat it as a necessary runtime gate, not a guarantee of active mod status.
5. The Settings window shows missing optional mod features unchecked and disabled (greyed out), without overwriting their saved preference. Reopen settings after changing the mod list and restart Timberborn to apply startup patches. Changes never live-unpatch Harmony.

| Keys | Mod | Required loaded class markers | Additional checks |
| --- | --- | --- | --- |
| `BFR` | Beavers For Real | `Calloatti.BeaversForReal.BFRManager` | `ProcessLocalizedChange` method and coordinate shape |
| `EBR` | Extended Builder Reach | `ExtendedBuilderReach.Components.ExtendedDemolishableAccessible` | Frame-drain hook must install before EBR listener interception |
| `FaunaBudget`, `FaunaCache`, `FaunaPrewarm` | Keystone | `Keystone.Mod.Fauna.FaunaSpawnDrainer` | Target recipe/spawn methods; fauna prewarming additionally requires game template services |
| `SuperCursor` | SuperCursor | `SuperCursor.Services.SuperCursorTool` | `ProcessInfo` callback shape |
| `PhysicsCatchup` | TimberPhysics | `TimberPhysics.Core.PhysicsSimulator`, `TimberPhysics.Core.PhysicalObjectRegistry` | Physics timer / simulation method signatures |
| `PhysicsCollider` | TimberPhysics | `TimberPhysics.Terrain.TerrainColliderService`, `TimberPhysics.Core.PhysicsSimulator` | Terrain collider service fields, structures and hooks |
| `SWC` | Simple Workshop Control | `TonWolfe.SimpleWorkshopControl.SimpleWorkshopControler`, `PlantMonitor`, `PlantingControler` | Workplace/harvest safety must install before the scan and allocation fast paths |

Base-game features (`PreviewNav`, `PreviewPool`, `PreviewSweep`, `BlueprintCache`, `BulkBuild`, `SoilReset`) do **not** depend on optional mods. `GcSmoother` is likewise a game/runtime feature and stays user-configurable: it independently rechecks Unity incremental-GC runtime support every 300 update callbacks instead of disabling the setting at main-menu startup.

## Limitations and testing

Detection happens when startup patches are installed; it does **not** promise reliable distinction between an assembly loaded but disabled by the game's mod manager and an enabled mod. A stronger, authoritative enabled-mod registration check should be adopted once a validated Timberborn 1.1 API is available. Do not claim otherwise.

For each optional mod: test with its DLL completely absent; installed but disabled (may still be loaded); enabled with compatible signatures; enabled with changed signatures; explicit user opt-out; and a restart after re-enabling. Verify no optional Harmony hooks touch shared game methods when the mod is absent. Live gameplay checks remain necessary; compilation alone cannot establish safe scheduling or behavior.

Implementation: [`ModAvailability.cs`](ModAvailability.cs), [`ModStarter.cs`](ModStarter.cs), [`OptimizationToggles.cs`](OptimizationToggles.cs), and [`SettingsIntegration.cs`](SettingsIntegration.cs).
