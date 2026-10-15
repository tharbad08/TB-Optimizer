# Optional-mod detection and patch installation

## Settings panel: authoritative enabled-mod detection (v1.1.114)

The **settings UI** now checks the active `ModRepository.EnabledMods` list against verified manifest IDs, in addition to checking required runtime types. An installed-but-disabled mod therefore gets disabled checkboxes in the settings panel, even when its assembly remains loaded. Base-game options are unchanged.

**Startup patches are different:** `ModStarter.StartMod` currently uses `ShouldInstall(key)` and runtime-type detection only, because the mod repository is not yet verified as safely accessible at that early lifecycle stage. Therefore the settings panel's enabled-mod check **does not guarantee** a disabled-but-loaded mod avoids startup Harmony hooks. This remaining migration requires early-stage DI/API verification and in-game testing, not an assumption.


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


## Verified against installed DLLs (2026-10-14)

The 2026-10-14 installed-mod reference ZIP includes both active and
obsolete Workshop version folders. TB Optimizer's targets were checked
against active v1.1 mod directories, not every archived version.

- SWC v1.1.38: valid PlantMonitor, PlantingControler and
  SimpleWorkshopControler types, fields and tick methods. Timberborn
  1.1.2.7 has a **three-argument** HarvestStarter.FindYielder; v1.1.119
  corrects the safety guard's method lookup and retains the mandatory
  original-IL validation. Its allocation fast-path expects legacy inner
  scanner methods absent from current SWC; the fast-path is skipped.
- EBR v11: ExtendedDemolishableAccessible.UpdateAccesses,
  NavMeshListenerEntityRegistry and NavMeshUpdate members found.
- Keystone v1.3.3, BFR v2.2.1, SuperCursor v10.0.6 and
  TimberPhysics v1.1.1.0: target type/method names exist.
- These checks are static DLL metadata inspections, not in-game proofs.
