# AI agent guide — TB Optimizer (Timberborn 1.1)

Read this before editing this repository. This is the standalone, runtime-optimized project guide; the shared, expanded lessons are in [tharbad08/TimberbornMods/Agents.MD](https://github.com/tharbad08/TimberbornMods/blob/master/Agents.MD). If modifying someone else's project, honor their **own** `AGENTS.md`/`Agents.MD` ahead of style guidance here.

## Goals and hard safety boundaries

- TB Optimizer contains guarded, individually switchable gameplay optimizations. **Do not add global tick skipping, global intervals, CSV benchmarking, experimental startup-assembly guards or freeze profiling** without explicit user permission. `BenchmarkAndOptimizerV11Port`, `TBSave` and `TBGC` hold that other work.
- Preserve player gameplay and map/save safety. Do not silently defer, drop, reorder or suppress jobs, growth ticks, collider edits or navigation callbacks. Invariant-breaking performance shortcuts are not optimizations.
- Never assume an optional mod is installed or enabled. Guard each integration against *missing* and *disabled* mods, and against API changes; fail to vanilla when a compatible hook cannot be proven.
- **Only `tharbad08` as the author** in user-controlled manifests, project credits and new commits. Do not expose user's real name, private paths or credentials. Respect legitimate upstream contributor credits.

## APIs, native services and references

1. Before new code, search **Timberborn game services**, **TimberUi**, **Mod Settings**, **Harmony**, and the target mod's source for an existing capability. Reuse native settings, notifications, timers/lifecycle, coordinates, serialization and registry APIs rather than implementing stand-ins.
2. **The game and dependency DLLs are authoritative.** Validate exact assembly version, member signature, return type and visibility. See `ModdingAssetUpdater/out` where supplied. Handwritten `Stubs/` files are only compile-time fallbacks; a green CI build against them is NOT proof of 1.1 binary compatibility. Real failure: a fabricated `IStyle.set_opacity(float)` stub led to a reproducible `MissingMethodException` in Mod Settings.
3. For a runtime mismatch, inspect the actual `Player.log`, full stack trace, inner exceptions and the specific installed DLL. Gather game Managed DLLs AND mod DLLs under Workshop/local versioned folders; preserve paths and hashes. Do not claim to have tested binaries not present.
4. Use Bindito-managed `ILoadableSingleton`/`IPostLoadableSingleton`/`IUpdatableSingleton` when lifecycle/timing semantics match. A Harmony postfix on `UpdateSingletons` measures update callbacks, not automatically render frames. For patches, verify IL and target signatures, unpatch partial installs by our Harmony ID, and never leave side effects half-applied.
5. In Timberborn **grid X/Y are horizontal, Z elevation**; Unity **Y is up**. Consult `CoordinateSystem` for grid/world conversion.

## Settings — use Mod Settings' actual implementation

- `SettingsIntegration.cs` should use **native** `ModSettings.Core.ModSetting<bool>` with the framework's `BoolModSettingElementFactory`. Use the **separate** `ModSettings.Common.ReadonlyTextModSetting` for `Base game` and `Other mods` section headings, and the real `ModSettings.Common` assembly. Validate its constructor against installed DLL; do not invent `TextSettings()` when the real constructor requires parameters.
- Native settings own rows, checkboxes, enable conditions, tooltips, scrolling, Reset to Default and persistence. **Do not set the global `ModSettingsBox` width to 885px**, create nested scroll areas, or fake the UI with custom `IStyle` setters. Labels should be concise (~40 chars) and headings separate; avoid the repetitive `Base game |` prefix and horizontal scrolling.
- The older `OptimizationToggles.cs` stores `tb-optimizer-options.json` as startup source. Native settings also persist via `ISettings`. **This is known duplicate storage and migration work still to do:** preserve explicit OFF choices through restarts, read settings before patch installation, and only delete the JSON backend once early startup access is confirmed and migration tested. Never presume the settings window's mirrored values are already the startup source.
- Test main-menu and in-game settings open, scroll, headings, option availability, reset, reopen, save/restart and active patch selection. “Compiles” is not “opens.”

## Optional mods

- See `ModAvailability.cs` and `MOD-DETECTION.md`. Loaded-assembly/type detection ≠ enabled mod. **The v1.1.114 settings UI uses actual `ModRepository.EnabledMods` + verified manifest IDs**; early `ModStarter.StartMod` patch installation still uses type-based detection. Do NOT falsely report that startup checks are fixed. Verify early DI availability and change startup checks separately, with absent/disabled/enabled/mod-version tests.
- Current optional support: Beavers For Real, Extended Builder Reach, Keystone fauna, SuperCursor, TimberPhysics, Simple Workshop Control. Do not add dependency requirements for optional mods. If upstream includes an optimization, skip our duplicate Harmony hooks and independent deferred queues.
- Validate all shape-sensitive reflection calls and patch owners before installation. Unknown or partially patched versions fail closed to original behavior.

## Feature-specific engineering lessons

- **EBR:** Keep PR-compatible spatial indexing, 8×8 grid buckets, exact 3D bounds checks, registration/unregistration, changed reach bounds and deferred callback guarantees. The newer maintainer PR may invoke `BoundsChanged` natively; old Workshop EBR requires an external bounds-change adapter. Never run both indexes; verify terrain changes, demolition, builders' reach and save/load.
- **TimberPhysics:** 32×32 collider chunks must invalidate correctly, clean obsolete dictionaries and restore vanilla *transactionally* if construction fails. Previous mixed colliders caused duplicate-key crashes. Catch-up limiting deliberately drops physics debt; disclose this as a gameplay tradeoff.
- **SWC:** Do not copy more of native harvest/plant scan logic than necessary. If acceleration replaces scans, preserve tick/cooldown/blocked-state/safety semantics and check IL; restore native method if invalid. Avoid duplicating an upstream fast scanner.
- **GC:** Unity incremental GC is an engine feature, not something to silently enable. Extra collection slices need benchmarks, correct mode detection and measured benefit. Managed heap ≠ all process memory.
- **Preview/blueprint/soil:** Protect vanilla navigation/placement validity, component construction order, mod hooks, and all global tick cadence. Soil cleanup/reset may be optimized; slowing global soil, planting, work, or singleton dispatch caused or risked stalls. See `FAILED_EXPERIMENTS.md` before changing optimizers.

## Build, verify and deliver

1. Read `README.md`, `FEATURE-LIST.md`, `OPTIMIZATIONS.md`, `FAILED_EXPERIMENTS.md`, `MOD-DETECTION.md`, and `NATIVE-API-AUDIT-1.1.114.md` (on the experimental branch) before modifying relevant systems.
2. Check the actual project repository and latest upstream branch/commit before editing. Keep test branches separate from stable `main` until the user authorizes a merge. Use focused source branches and direct authorized merges inside our own repositories; **never open internal branch-to-branch PRs**. Open PRs only to other maintainers' repositories. Preserve any main-only files (including this guide) when merging.
3. CI must test optional-mod activation guards and native settings regression rules and compile. Whenever possible, add genuine 1.1 game and dependency DLL references—not only stubs. Do not ship a build with an unresolved/unknown API signature.
4. Deliver a **ready-to-install ZIP** containing the manifest and compiled DLL in `TBOptimizer/version-1.1/manifest.json` and `TBOptimizer/version-1.1/Scripts/TBOptimizer.dll`. Validate ZIP integrity, correct manifest ID/version/Description, dependencies, DLL version and branch commit. Avoid duplicated old versions in the user's mod folder.
5. Distinguish source review, build pass, archive inspection, main-menu load, settings-open test, save-load test and gameplay validation. Do not claim an unperformed test. Include a verified download and CI run when requested.
6. Write concrete failure explanations and verified status in docs; don't reattempt entries in `FAILED_EXPERIMENTS.md` without new evidence. Avoid noisy per-tick release logging.

## Upstream PRs and cross-repository contributions

- **Never open PRs between branches of our own repositories.** Keep internal work on branches; when the user approves a tested branch for `main`, merge directly while preserving main-only changes and ancestry. For closed internal PRs, keep source branches and commits available until explicitly told to delete them.
- Contributions to EBR, SuperCursor, Keystone, BFR and other maintainers **must target their external repository**. Read the target project's own `Agents.MD`/`AGENTS.md` and nearby source before changing anything; verify the latest upstream base and that the diff only touches that mod. Never use a fork-local staging PR as a replacement for an upstream PR.
- **Native integration over external patch ports:** TB Optimizer may need guarded Harmony hooks to optimize optional DLLs. When sending the improvement back to the mod author, directly change that mod's existing components and native services instead, where possible. If Harmony remains necessary, match the project's preferred `[HarmonyPatch]` attributes and existing `PatchAll()` flow; do not manually patch runtime methods when the first-party code can be changed.
- On GitHub **403** for an external PR, report the permission restriction accurately, keep the pushable head branch and give the upstream compare link plus a ready-to-paste **raw fenced Markdown** PR description. Do not claim a PR exists or create an internal PR instead.
- Explain the PR to a **human maintainer**: problem, design, implementation files, behavior preserved, trade-offs, dependency/lifecycle handling, and explicit verified vs unverified tests. A separately enabled EBR module in TB Optimizer can provide meaningful field-test evidence of the shared algorithm, but not proof that a different native upstream rewrite has compiled or worked in-game. Never merge superseded code merely because a closed PR contains it; compare with the newest actual source first.

## Adjacent Timberborn mod projects

Our common lessons also apply to **TB Doctor** (native Player.log + notifications, no guessed dismissal API, job safety), **Keystone/MoM** (optional excavation compatibility and cosmetic water spawning), **Folktails+** (faction-sensitive content and one merged installable mod), **All Weather Drill/Moddable Weather** (mid-event save/load state and base/modifier weather combinations), and maintainers' PRs (read their own `Agents.MD` and match conventions). Consult the cross-project [shared guide](https://github.com/tharbad08/TimberbornMods/blob/master/Agents.MD) before these integrations.
