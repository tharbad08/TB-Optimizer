# Native API audit and settings integration (v1.1.114)

## Changes implemented

- TB Optimizer settings now use the **Mod Settings** mod's native `ReadonlyTextModSetting` to display **Base game** and **Other mods** as distinct section headings. Native checkbox rows use only feature labels rather than repeating the group on every row.
- The heading class and constructor were verified against eMka's `ModSettings.Common` source. The project references the **`ModSettings.Common`** assembly name, separate from `ModSettings.CommonUI`. Unlike the earlier custom UI, it never calls custom `IStyle` setters.
- In the settings UI, optional features are enabled only if BOTH their types are loaded and their **manifest IDs are found in `ModRepository.EnabledMods`**. IDs are sourced from the BFR, EBR, Keystone, Super Cursor, TimberPhysics and SWC manifests. The core `ModSettingsBox` and bool setting factories remain native.
- Optimizations including the new EBR PR port are untouched in this UI test iteration.

## Deferred safety-sensitive migrations

1. **Startup-time mod detection:** `ModStarter.StartMod` runs before settings owners are loaded. Its `ModAvailability.ShouldInstall` currently checks loaded classes only. We must resolve a supported early enabled-mod registry in startup to avoid breaking optional mod activation. **Do not claim startup detection is now authoritative.** Settings UI availability *is* registry-checked.
2. **Single preference backend:** The optimizer's old `tb-optimizer-options.json` remains the **startup** source, with native `ISettings` mirrored in the settings view. Replacing it without a verified early `ISettings` reader risks ignoring explicit OFF switches on restart. No existing JSON was deleted in this test.
3. **True game DLL compile verification:** The project still compiles against stubs, not actual 1.1 Unity/Mod Settings DLLs. The previously invented `IStyle.opacity(float)` broke the runtime despite CI passing. The dangerous custom UI is removed and the headings use verified public framework APIs; CI success still does not prove in-game UI compatibility.
4. **Update hooks/GC/SWC:** Existing behavior stays unchanged pending measurement and engine timing analysis.

**Manual game test:** open settings; verify the two headings are standalone rather than inline, checkboxes align, absent mods are disabled; confirm your old OFF preferences remain OFF after restart and after reset. Test EBR separately by changing terrain/demolishing to verify delayed notifications.
