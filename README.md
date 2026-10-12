# TB Optimizer — Timberborn 1.1

This branch contains **only individually switchable gameplay optimizations**. It does not run benchmarks, write CSV/freeze logs or throttle any tick/update systems. It also does not patch the startup dynamic-assembly discovery paths.

- Complete final optimization list, settings keys, per-file implementation map and PR guidance: [OPTIMIZATIONS.md](OPTIMIZATIONS.md).
- **Do-not-repeat register** of crashes, failed optimizations, rejected shortcuts, inconclusive diagnostics and safe alternatives: [FAILED_EXPERIMENTS.md](FAILED_EXPERIMENTS.md).
- SWC (automatic when loaded unless explicitly switched off): harvest/workplace safety, four-tick scan scheduling, and guarded allocation-reducing harvest/planting scans. Details: [SWC reference and compatibility](OPTIMIZATIONS.md#simple-workshop-control-swc--documented-port-not-in-this-branchs-release) and [FEATURE-LIST.md](FEATURE-LIST.md).
- The separate [BenchmarkAndOptimizerV11Port](https://github.com/tharbad08/TimberbornMods/tree/chatgpt/BenchmarkAndOptimizerV11Port) branch owns interval controls, protected tick policy, CSV benchmarks and diagnostic profiling.
- The [TBSave](https://github.com/tharbad08/TimberbornMods/tree/chatgpt/TBSave) research branch holds startup crash-guard code and save/load profiling research; the startup guard is not itself a save optimization.

Restart after changing an individual feature toggle. No global dispatcher hooks, broad profiling or per-system sliders are installed by this mod.

## Mod Settings — v1.1.105

The settings panel groups individual toggles for base-game and optional third-party optimizations. The **SWC scans and harvest safety** option defaults to enabled for an available SWC mod unless explicitly disabled. Its runtime patches install in safety-first order and never replace the installed SWC DLL. Other retained optimizations preserve existing saved settings and restart semantics. Incremental GC now has an active frame hook, requesting bounded slices only if Unity reports incremental GC supported and the managed heap is sufficiently large. Startup dynamic-assembly guards and benchmark per-system intervals are **not shipped by TB Optimizer** and are not represented as functioning checkboxes. See [FEATURE-LIST.md](FEATURE-LIST.md).

### Availability-aware settings (v1.1.106)

Optional mods are detected through loaded runtime types. Their optimization checkboxes default on but become disabled and unchecked if those types are absent. Saved off preferences are retained when the mod is re-enabled. This is loaded-type detection, not a guarantee of method-level compatibility; each patch also guards its own signatures. Restart after changes. The new SWC Harmony port and conditional GC collector require live gameplay validation; compilation alone is not proof.

### GC runtime polling

The GC setting can be enabled in the main menu even if Unity currently reports incremental collection disabled. TB Optimizer installs a lightweight hook and rechecks the current engine mode every 300 `SingletonLifecycleService.UpdateSingletons` calls during gameplay. The counter measures **update callbacks**, not heap mutations; managed heap size is sampled only if incremental GC is enabled. Below 5.5 GiB managed heap no explicit slice is requested. No game GC mode is forced on.
