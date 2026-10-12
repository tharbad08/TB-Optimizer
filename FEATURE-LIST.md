# TB Optimizer 1.1.105 — settings and optimization list

Each implemented option has a checkbox under **TB Optimizer** in Mod Settings. Settings persist in `tb-optimizer-options.json`; restart Timberborn after changing one. If a supported third-party mod is absent, its patch normally does not install. These are source-implementation statuses, **not** claims of in-game performance gains or compatibility with every installed version.

## Base game / shared systems

| Setting key | Checkbox label | Behavior | Default |
| --- | --- | --- | --- |
| `PreviewNav` | Preview navigation batching | Batch preview navmesh add/remove notifications; retain native placement validation | On |
| `PreviewSweep` | Efficient preview hide sweep | Avoid LINQ / HashSet allocations in preview cleanup where ordering is verified | On |
| `PreviewPool` | Unused preview pool fast path | Avoid initializing never-used lazy brush previews | On |
| `BlueprintCache` | Blueprint ComponentSpec cache | Reuse immutable blueprint specification lookups, not live mutable components | On |
| `BulkBuild` | Faster bulk component creation | Optimize warmed component batches while preserving initialization order and compatible third-party hooks | On |
| `SoilReset` | Soil contamination reset | Optimize reset and clear operations; never throttle normal soil ticks | On |

## Third-party mods

| Setting key | Target | Behavior | Default |
| --- | --- | --- | --- |
| `EBR` | Extended Builder Reach | Spatially filter navigation listeners (8x8), defer huge notifications (256 listeners / 8 ms per frame) only when the drain hook attaches | On |
| `BFR` | Beavers For Real | Batch sparse navigation changes by 8x8 areas | On |
| `FaunaBudget` | Keystone | Process at most one successful fauna spawn per update while retaining pending requests | On |
| `FaunaCache` | Keystone | Cache successful fauna recipe lookups | On |
| `FaunaPrewarm` | Keystone | Prewarm Cow, Bull and Deer after game initialization | On |
| `SuperCursor` | SuperCursor | Adaptive 20/10/4 Hz informational refresh, without delaying tool input | On |
| `PhysicsCatchup` | TimberPhysics | Cap physics backlog to four 0.02-second substeps and a ~50 ms budget; **excess backlog may be dropped** | On |
| `PhysicsCollider` | TimberPhysics | Merge compatible terrain colliders in 32x32 dirty chunks; restore on error | On |
| `SWC` | Simple Workshop Control | Harvest/workplace safety, four-tick scans and version-checked allocation-reducing inner loops (automatic when SWC is loaded) | On (when loaded) |

**SWC compatibility:** uses runtime discovery, with no bundled SWC DLL. Future SWC versions may change signatures or method IL and cause the optimization to skip. The separate [SWC fast-scanner prototype](https://github.com/tharbad08/TimberbornMods/blob/chatgpt/tb-optimizer-swc-compat/BenchmarkAndOptimizerV11Port/Optimizations/Mods/SWC/SWCFastScanner.cs) is not wired in this release. The port is **not yet equivalent** to the older modified SWC DLL's reusable harvest buffers and inner-loop allocation reductions. The workplace safety guard and scan scheduler have passed compilation, **not live gameplay validation**.

## Research / intentionally not active

| Setting | State | Why |
| --- | --- | --- |
| `GcSmoother` | On if supported, greyed out otherwise | CollectIncremental is called at most every 300 frames, with bounded budgets and heap thresholds; only where Unity reports incremental GC is enabled |
| Startup DMD/type scanner guards | Not included | Separate `TBSave` research / startup safety work; no functioning TB Optimizer checkbox |
| Per-system update intervals / tick protection | Not included | Belongs to the separate benchmark/CSV profiler; global dispatcher throttling previously stalled work and is excluded |

## Important safety boundaries

- **No** global tick throttling, save serialization rewrite, dynamic-assembly scan guard or benchmark/CSV profiler in TB Optimizer.
- SWC's scheduling can delay observation of a new harvested good or planting spot by up to several ticks. Confirm behavior after weather contamination events, workplace changes, farmer/forester replanting, and save/load.
- New checkboxes appear in a grouped vertical list and have a short explanation below each. Restart to apply changes.
- Baseline every speed claim against the same save, camera and weather. Record **FPS and actual x1/x3 game speed**.
- Source/limitations: [OPTIMIZATIONS.md](OPTIMIZATIONS.md) and [FAILED_EXPERIMENTS.md](FAILED_EXPERIMENTS.md).

## Loaded-mod activation and disable rules

The settings screen checks currently **loaded runtime types** rather than Workshop folder presence. Missing optional mods and unsupported incremental GC have disabled/greyed-out toggles with an explanation. If a mod is loaded, its optimization is enabled by default unless its key was explicitly set to false in `tb-optimizer-options.json`. The program never changes an explicit off setting when a mod becomes loaded. Reopen Mod Settings after activating/deactivating mods and restart the game to apply patches. Detection confirms loaded types, **not** full compatibility; patch methods independently verify expected signatures/IL before installing. GC is sampled at most every 300 updates and only requests incremental slices when supported. The SWC inner-scan delegate port is guarded by method/signature checks, and retains native scanning when binding fails; live gameplay validation remains required.
