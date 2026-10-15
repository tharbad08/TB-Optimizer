# TB Optimizer real-assembly API audit — 2026-10-14

**Reference:** uploaded `timberborn-build-refs-20261014-005851.zip`,
Timberborn build 1.1.2.7. The game assemblies and Workshop Mod Settings
version-1.1 assemblies were inspected using actual ECMA-335 .NET metadata,
not the compile-only stubs. The archive does not include the required
third-party DLLs for EBR, SWC, Keystone, BFR, SuperCursor or TimberPhysics,
or the TimberUi DLL.

| Assembly | Verified runtime declaration | Status |
| --- | --- | --- |
| Timberborn.Modding | `ModRepository.get_EnabledMods(): IEnumerable<Mod>` | **Confirmed crash in 1.1.115**; reflection fallback already in 1.1.116; corrected the stale compile stub in 1.1.118 |
| Timberborn.Modding | `Mod.get_Manifest(): ModManifest` and `ModManifest.get_Id(): string` | Reflection paths valid; Id property is read-only |
| ModSettings.Common | `TextSettings..ctor(TextAnchor, int, Vector4)` | **Confirmed crash in 1.1.114**; runtime-safe ctor already in 1.1.115 |
| ModSettings.Common | `ReadonlyTextModSetting..ctor(ModSettingDescriptor, TextSettings)` | Matches |
| ModSettings.Core | `ModSettingsOwner..ctor(ISettings, Registry, ModRepository)` | Matches |
| ModSettings.Core | `protected abstract ModSettingsOwner.ModId` | Fixed stub/override in 1.1.117 |
| ModSettings.Core | `OnBeforeLoad(): void`, `ChangeableOn: ModSettingsContext` | Matches |
| ModSettings.Core | `AddCustomModSetting<T>(ModSetting<T>, string)`; `AddNonPersistentModSetting(NonPersistentSetting)` | Matches |
| ModSettings.Core | `ModSetting<T>(T, ModSettingDescriptor)`, `SetValue(T)`, `DefaultValue`, `ValueChanged`, `Reset()` | Matches |
| ModSettings.Core | `ModSettingDescriptor.Create(string)`, `SetTooltip(string)`, `SetEnableCondition(Func<bool>)` | Matches |
| ModSettings.Core | `NonPersistentSetting..ctor(ModSettingDescriptor)`, `Reset()` | Matches |
| Timberborn.ModManagerScene | `IModStarter.StartMod(IModEnvironment): void`; `IModEnvironment.ModPath: string` | Matches |
| Bindito.Core | `ContextAttribute..ctor(string)`; `Configurator.Configure(): abstract void` | Matches; corrected stub abstract declaration |
| Timberborn.SingletonSystem | `SingletonLifecycleService.UpdateSingletons(): void` | Reflected hook matches |
| UnityEngine.CoreModule | `GarbageCollector.get_isIncremental(): bool` and `CollectIncremental(ulong): bool` | Reflected GC probe matches |

**Result:** No additional *confirmed* static signature mismatch was detected in
the audited startup/settings APIs or the selected core hooks. This is not a
gameplay validation and does not prove compatibility with every optional mod.
Use the built v1.1.118 artifact for a new in-game smoke test; keep this PR draft.
