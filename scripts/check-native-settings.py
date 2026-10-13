#!/usr/bin/env python3
"""Guard native Mod Settings integration against the v1.1.112 UI and v1.1.114 TextSettings startup crashes."""
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
assert "repository.EnabledMods" in modcheck
assert "style.opacity" not in settings
assert "float opacity" not in stub, "Do not reintroduce invalid Unity IStyle.set_opacity(float) stub"
assert "OptimizerSettingElementFactory" not in settings, "Use framework-native checkbox UI"
assert "ModSettingsBox()" not in settings, "Never resize global Mod Settings window"
print("Native Mod Settings static regression checks passed.")
