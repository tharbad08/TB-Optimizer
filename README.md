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

### Optional mod detection

**Every third-party patch is gated by loaded-mod class detection AND the user's enabled preference.** Missing optional mods skip all associated Harmony hooks (including any base-game methods they would otherwise intercept). See [MOD-DETECTION.md](MOD-DETECTION.md) for the complete mod/key matrix, startup order, greyed-out settings, compatibility checks and limitations. Loaded classes are not definitive proof that the game's mod manager enabled the mod.

### Reset to Default

The game's **Reset to Default** button clears TB Optimizer's explicit checkbox overrides in `tb-optimizer-options.json`. The panel reopens showing defaults. Existing per-feature default-on behavior is restored, but installed Harmony patches still change only after restarting Timberborn.


### Timberborn 1.1 API compatibility — v1.1.116

A previous test build (v1.1.115) failed to open the main menu on game 1.1.2.7
with `MissingMethodException` for
`ModRepository.get_EnabledMods(): IReadOnlyList<Mod>`. The compiled
`Timberborn.Modding` stub specified a return type not guaranteed by the
actual 1.1 runtime. The settings panel now obtains the enabled-mod collection
by runtime reflection (property name only), safely enumerates it, and reads the
enabled IDs without statically linking to stub property return types. If the
registry is missing or uninitialized, optional optimization checkboxes are
disabled instead of crashing. The compile stub makes direct property calls
an error; CI also checks for this regression. No gameplay or patch startup
logic was changed, and installed EBR patch integration is retained.

This is a *compile/static check*, not an in-game validation. Test both main
menu and game settings, then verify optional-mod enable/disable status.


### Native Mod Settings API declaration audit — v1.1.117

Checked directly against the Mod Settings maintainer's published sources in
`eMkaQQ/timberborn-modding`: `ModSettingsOwner.ModId` (protected abstract,
corrected in stub and override), `ChangeableOn`, `OnBeforeLoad`,
`AddCustomModSetting`, `AddNonPersistentModSetting`, `ModSetting<T>`
constructor, `DefaultValue`, `SetValue`, `ValueChanged`,
`ModSettingDescriptor.Create/SetTooltip/SetEnableCondition`,
`NonPersistentSetting.Reset` and the three-argument `TextSettings`
constructor. No further direct native settings API mismatch was found in these
symbols. The remaining game-API and optional-mod Harmony targets are discovered
through reflection with signature checks and fail-closed behavior; they still
require the real Timberborn and optional-mod DLLs for definitive signature
validation. The earlier v1.1.116 `EnabledMods` return-type crash fix remains.


### Real game/framework assembly audit — v1.1.118

Compared mod startup, native Mod Settings and selected runtime-hook signatures
against the actual `Timberborn.Modding.dll`, `Timberborn.ModManagerScene.dll`,
`Timberborn.SingletonSystem.dll`, `UnityEngine.CoreModule.dll`,
`Bindito.Core.dll` and **Mod Settings 1.1** DLLs supplied on 2026-10-14.

The v1.1.114 `TextSettings` constructor mismatch and v1.1.115
`EnabledMods` return-type mismatch are confirmed. The real
`ModRepository.EnabledMods` returns `IEnumerable<Mod>` (not
`IReadOnlyList<Mod>`). Updated the compile stub to the verified
declaration while continuing to forbid direct calls, preserving the
runtime-safe reflection used in v1.1.116. Corrected the Bindito
`Configurator.Configure()` stub declaration to `abstract`, and made
`ModManifest.Id` getter-only as it is in the game.

Verified other native settings calls, `IModStarter.StartMod`,
`IModEnvironment.ModPath`, Unity incremental-GC signatures and
`SingletonLifecycleService.UpdateSingletons` with no additional
confirmed mismatches. Additional DLLs are needed to verify
optional-mod hook internals and TimberUi (not in the supplied archive).
Details: [API-COMPATIBILITY-2026-10-14.md](API-COMPATIBILITY-2026-10-14.md).


### Full installed-mod DLL audit — v1.1.119

The `timberborn-build-refs-20261014-013425.zip` archive contains
1118 DLLs: 502 game, 603 Workshop and 13 local. The active compatibility
references for Extended Builder Reach v11, SWC v1.1.38,
BFR v2.2.1, Keystone v1.3.3, Super Cursor v10.0.6,
Bober's Laws of Motion v1.1.1.0 and TimberUi v11.0.1 were identified
from their exact version directories and manifests, not older versions in
the same Workshop download folders.

**Confirmed additional mismatch:** Timberborn 1.1's
`HarvestStarter.FindYielder` takes three arguments
(`receivingInventory`, `inRangeYielders`, `prioritizedName`)
and returns `YielderSearchResult`. Our SWC harvest/workplace guard had
been asking for an impossible parameterless version; thus the SWC guard,
four-tick stagger and allocation scan could not activate even when the
installed SWC had matching components.

v1.1.119 selects only the exact three-argument method shape. Its existing
safety criteria remain mandatory: no exception blocks and exactly one
workplace-check insertion point in the original IL. The supplied
`Timberborn.Fields.dll` method body was additionally inspected: it has
one instance of the expected `GetYielders / pop / ldarg.0 /
_worker / Worker.Workplace` sequence. Unsupported future game builds
still disable the safety guard and thereby the SWC scan optimizations
instead of risking a broken worker or altered reservations.

**Other integrations:** EBR v11's `UpdateAccesses` / registry / nav update
members, BFR's `ProcessLocalizedChange`, Keystone's fauna drainer,
SuperCursor's `ProcessInfo`, and Bober's Laws' physics/collider
entry points all exist under the probed names and layouts.
This metadata audit verifies presence and nominal shapes, not gameplay
correctness or the success of every Harmony patch; test in game before
merging. The SWC allocation fast-path currently probes
`RefreshGoods` and `FindEmptySpot`, which are absent from installed
SWC v1.1.38, so it correctly skips those optional replacement hooks.
