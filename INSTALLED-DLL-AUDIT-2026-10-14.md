# TB Optimizer installed DLL audit — 2026-10-14 (v1.1.119)

Reference archive: `timberborn-build-refs-20261014-013425.zip`.
Total: 1,118 DLLs (502 game, 603 Workshop, 13 local).
Inspected actual CLR metadata of active v1.1 mod DLLs, avoiding older
version directories that coexist in the archive.

| Integration | Verified installed version | Metadata audit result |
| --- | --- | --- |
| Extended Builder Reach | 11.0.0 | `ExtendedDemolishableAccessible.UpdateAccesses`, `OnNavMeshUpdated`, `bounds`, registry `NotifyAll/RegisterNavMeshListener/UnregisterNavMeshListener`, `NavMeshUpdate.Bounds/TerrainCoordinates/UpdatedRoads` found |
| SWC | 1.1.38 | Component classes, Tick and UpdateBlockedState found. **Timberborn.Fields.HarvestStarter.FindYielder takes 3 args**, contrary to optimizer's zero-arg lookup |
| BFR | 2.2.1 | `Calloatti.BeaversForReal.BFRManager.ProcessLocalizedChange` found |
| Keystone | 1.3.3 | `Keystone.Mod.Fauna.FaunaSpawnDrainer.UpdateSingleton/Spawn/FindRecipeForBlueprint` found (local compatibility DLL matches) |
| SuperCursor | 10.0.6 | `SuperCursor.Services.SuperCursorTool.ProcessInfo` found |
| Bober's Laws of Motion (TimberPhysics) | 1.1.1.0 | `PhysicsSimulator.UpdateSingleton/_timer/_physicalObjectRegistry`, `PhysicalObjectRegistry.StepAll`, `TerrainColliderService` layout found |
| TimberUi | 11.0.1 | Installed DLL found and available for compatibility comparison |
| Mod Settings | 1.1.1.0 | Exact v1.1 assemblies present; startup ABI mismatches fixed in v1.1.114–1.1.118 |

## SWC mismatch and safeguard

The actual Timberborn.Fields.dll has a single `FindYielder` method with
parameters named `receivingInventory`, `inRangeYielders`,
`prioritizedName` (the third is a string); its CLR return type is
`YielderSearchResult`. The original method IL includes one
`GetYielders; pop; ldarg.0; ldfld _worker; callvirt get_Workplace`
sequence. Previous code looked up `FindYielder()` and therefore could
not install the safety patch.

The v1.1.119 method matcher now selects the unique three-parameter
signature; original exception-region and unique-insertion-point
validation remain mandatory before changing any IL. If unsupported,
the patch is skipped and original SWC gameplay remains active.

## Remaining limitations

- This audit did not execute the game or apply Harmony patches in a live
  Timberborn process.
- Several optional performance fast paths are deliberately
  version-sensitive and may skip even when classes exist.
- The SWC allocation replacement targets `RefreshGoods` and
  `FindEmptySpot` do not exist in installed SWC v1.1.38; this existing
  optional patch skips safely without changing gameplay.
