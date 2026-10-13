# EBR PR #5 combined test in TB Optimizer 1.1.112

Source: [maintainer-friendly EBR PR #5](https://github.com/tharbad08/TimberbornMods/pull/5), branch `pr/ebr-maintainer-friendly-v11`.

`Optimizations/Mods/ExtendedBuilderReach/ExtendedBuilderReachNavOptimizer.cs` is based on the PR's latest spatial dispatcher, not the previous TB Optimizer implementation. It retains 8×8 XY buckets, exact 3D bounds checks, first-notification migration, pooled candidate arrays, 1,024-candidate threshold and deferred 256-listener/8ms frame budget.

## Port-specific integration

- `ModStarter.cs` checks the saved `EBR` toggle and loaded EBR type before installing. It checks `CanInstall()` before even installing the frame drain: when EBR already includes its own PR #5 dispatcher, TB Optimizer backs off.
- The upstream PR modifies `ExtendedDemolishableAccessible.UpdateAccesses` to call `BoundsChanged` when bounds change. TB Optimizer cannot require replacing the Workshop EBR DLL, so this port uses a Harmony prefix/postfix around that method to reindex **only if the computed bounds changed**.
- Installation order patches `NotifyAll` before interception of listener registration; incomplete patch installation unpatches its own three registry callbacks and the bounds hook.
- The rest of TB Optimizer, including the compact Mod Settings panel, retains the `main` branch's 1.1.111 implementation.
- The upstream PR includes a no-op diagnostics helper for normal builds. No benchmark/CSV logger is introduced.

## Test scope

Use standard EBR without PR #5's embedded optimizer to test external integration. Test creating/deleting demolition targets, changing extended reach settings, repeatedly editing terrain/roads, rapid builds and demolitions, and save/load, then compare actual x1/x3 game speed and FPS. Recheck that no EBR optimizer is installed when EBR is absent or disabled in settings. If you update EBR to a version that contains PR #5, confirm TB Optimizer detects the native dispatcher and skips external hooks.

**Validation:** CI can prove source compilation and packaging; it cannot demonstrate in-game compatibility, event ordering or the new bounds-change adapter. This is an experimental combined test build.
