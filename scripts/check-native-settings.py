#!/usr/bin/env python3
"""Guard native Mod Settings integration against the v1.1.112 UI and v1.1.114 TextSettings and v1.1.115 EnabledMods startup crashes."""
from pathlib import Path

root = Path(__file__).resolve().parents[1]
settings = (root / "SettingsIntegration.cs").read_text(encoding="utf-8")
stub = (root / "Stubs/UnityEngine.UIElementsModule/Stub.cs").read_text(encoding="utf-8")
project = (root / "BenchmarkAndOptimizerV11Port.csproj").read_text(encoding="utf-8")
modcheck = (root / "ModAvailability.cs").read_text(encoding="utf-8")

assert "ReadonlyTextModSetting" in settings, "Missing native settings section headers"
header_stub = (root / "Stubs/ModSettings.Common/Stub.cs").read_text(encoding="utf-8")
assert "NativeTextHeaderSettings.Create()" in settings, "Headers must invoke verified runtime constructor"
assert "new ReadonlyTextModSetting.TextSettings()" not in settings, "Parameterless TextSettings caused 1.1.114 startup crash"
assert "private TextSettings()" in header_stub, "Compile stub must reject imaginary public ctor"
assert "public TextSettings()" not in header_stub, "Do not reintroduce invalid parameterless ABI"
assert "GetParameters().Length == 3" in settings, "eMka Mod Settings runtime constructor takes three arguments"
assert "catch (Exception ex)" in settings, "Future incompatible headers must not crash game boot"

assert "ModSettingDescriptor.Create(currentGroup)" in settings, "Group headings not separate"
assert 'Create(option.Label)' in settings, "Option labels must not repeat group prefix"
assert "ModSettings.Common/ModSettings.Common.Stub.csproj" in project
assert "ModAvailability.IsAvailableInSettings(option.Key, _repository)" in settings
# v1.1.115: a direct property call compiled with a fake IReadOnlyList return
# type, then threw MissingMethodException against Timberborn 1.1.
# A tiny compile stub must not authorize such calls again.
mod_stub = (root / "Stubs/Timberborn.Modding/Stub.cs").read_text(encoding="utf-8")
assert 'ReadRuntimeMember(repository, "EnabledMods")' in modcheck, "Use real runtime property metadata"
assert "repository.EnabledMods" not in modcheck, "Direct binding to fake stub causes startup crash"
assert "enabled is not IEnumerable" in modcheck, "Runtime registry must be enumerated independent of CLR return type"
assert 'ReadRuntimeMember(mod, "Manifest")' in modcheck
assert 'ReadRuntimeMember(manifest, "Id")' in modcheck
assert "catch (Exception)" in modcheck, "Uninitialized registry must fail closed"
assert "[Obsolete(" in mod_stub and "true)]" in mod_stub, "Stub must disallow direct EnabledMods calls"
# Verified against Timberborn.Modding.dll: IEnumerable<Mod>, not IReadOnlyList<Mod>.
assert "IEnumerable<Mod> EnabledMods" in mod_stub, "Stub ABI mismatch: real EnabledMods returns IEnumerable<Mod>"
assert "IReadOnlyList<Mod> EnabledMods" not in mod_stub, "Never reintroduce the v1.1.115 startup crash"
# Verified against Timberborn.Modding.dll: ModManifest.Id is a getter only.
assert 'public string Id => ""' in mod_stub, "Real ModManifest.Id is read-only"


assert "style.opacity" not in settings
assert "float opacity" not in stub, "Do not reintroduce invalid Unity IStyle.set_opacity(float) stub"
assert "OptimizerSettingElementFactory" not in settings, "Use framework-native checkbox UI"
assert "ModSettingsBox()" not in settings, "Never resize global Mod Settings window"
# Verified against Bindito.Core.dll: Configurator.Configure() is abstract.
bindito_stub = (root / "Stubs/Bindito.Core/Stub.cs").read_text(encoding="utf-8")
assert "public abstract void Configure();" in bindito_stub, "Bindito Configurator.Configure is abstract"
assert "public virtual void Configure() { }" not in bindito_stub
# UnityEngine.CoreModule: GarbageCollector.CollectIncremental(ulong): bool.
gc = (root / "Optimizations/BaseGame/GC/IncrementalGcSmoother.cs").read_text(encoding="utf-8")
assert 'new[] { typeof(ulong) }' in gc and 'collect.ReturnType != typeof(bool)' in gc
# Timberborn.SingletonSystem.SingletonLifecycleService.UpdateSingletons(): void.
hook = (root / "Optimizations/BaseGame/GC/IncrementalGcFrameHook.cs").read_text(encoding="utf-8")
assert '"UpdateSingletons"' in hook

core_stub = (root / "Stubs/ModSettings.Core/Stub.cs").read_text(encoding="utf-8")
assert 'protected override string ModId' in settings, "ModSettingsOwner.ModId is protected in eMka implementation"
assert 'protected abstract string ModId' in core_stub, "Core stub must match framework protected ModId"
assert 'public override string ModId' not in settings, "Prevent widened access declaration"

print("Native Mod Settings static regression checks passed.")

# Timberborn.Fields.dll 1.1.2.7: HarvestStarter.FindYielder has three
# arguments, not zero; the method body must retain the unique workplace
# guard insertion pattern before any Harmony patch is installed.
safety = (root / "Optimizations/Mods/SWC/SWCHarvestSafety.cs").read_text(encoding="utf-8")
assert 'DeclaredMethod(harvester, "FindYielder", Type.EmptyTypes)' not in safety, (
    "Timberborn 1.1.2.7 FindYielder signature is three parameters")
assert 'arguments.Length == 3' in safety
assert 'arguments[2].ParameterType == typeof(string)' in safety
assert 'arguments[0].Name == "receivingInventory"' in safety
assert 'arguments[1].Name == "inRangeYielders"' in safety
assert 'arguments[2].Name == "prioritizedName"' in safety
assert 'InjectionPoint(original, worker, workplace) < 0' in safety, (
    "Never enable SWC safety without verifying the original method IL")
assert 'original.Any(i => i.blocks.Count != 0)' in safety
