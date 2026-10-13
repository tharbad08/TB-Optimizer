using System;
using Bindito.Core;
using ModSettings.Common;
using ModSettings.Core;
using Timberborn.Modding;
using Timberborn.SettingsSystem;

namespace BenchmarkAndOptimizerV11;

// Use the existing Mod Settings mod UI. Its built-in setting factories provide
// the game's native rows, toggles, tooltips, disabled states and Reset button.
// No Unity UI Toolkit IStyle setters or custom VisualElement factories needed.
[Context("MainMenu")]
public sealed class MainMenuConfig : Configurator
{
    public override void Configure() =>
        this.BindSingleton<OptimizerSettingsOwner>();
}

[Context("Game")]
public sealed class GameConfig : Configurator
{
    public override void Configure() =>
        this.BindSingleton<OptimizerSettingsOwner>();
}

public sealed class OptimizerSettingsOwner(
    ISettings settings,
    ModSettingsOwnerRegistry registry,
    ModRepository repository
) : ModSettingsOwner(settings, registry, repository)
{
    public override string ModId => "TBOptimizer";
    public override ModSettingsContext ChangeableOn => ModSettingsContext.All;

    protected override void OnBeforeLoad()
    {
        string? currentGroup = null;

        foreach (var option in OptimizationToggles.Options)
        {
            if (!string.Equals(currentGroup, option.Group, StringComparison.Ordinal))
            {
                currentGroup = option.Group;
                // The native ReadonlyTextModSetting factory renders the
                // section heading without custom Unity UI code.
                AddNonPersistentModSetting(new ReadonlyTextModSetting(
                    ModSettingDescriptor.Create(currentGroup),
                    new ReadonlyTextModSetting.TextSettings()));
            }

            // Native BoolModSettingElementFactory owns the toggle layout,
            // binding and disabled appearance. Cache presence for this panel:
            // mod activation already requires a game restart.
            var available = ModAvailability.IsAvailable(option.Key);
            var native = new OptimizerBoolSetting(option, available);
            native.ValueChanged += (_, enabled) =>
                OptimizationToggles.Set(option.Key, enabled);
            AddCustomModSetting(native, option.Key);

            // JSON is the established source of preferences. Synchronize any
            // old native ISettings value with that source during migration.
            native.SetValue(OptimizationToggles.Enabled(option.Key));

            // Keep the original 1-2 line descriptions using the framework's
            // read-only text setting instead of styling game labels ourselves.
            AddNonPersistentModSetting(new ReadonlyTextModSetting(
                ModSettingDescriptor.Create(option.Description),
                new ReadonlyTextModSetting.TextSettings()));
        }
    }
}

internal sealed class OptimizerBoolSetting : ModSetting<bool>
{
    private readonly string _key;

    internal OptimizerBoolSetting(OptimizationToggles.Entry option, bool available)
        : base(option.DefaultEnabled,
            ModSettingDescriptor.Create(option.Label)
                .SetTooltip(option.Description.Replace("\n", " "))
                .SetEnableCondition(() => available))
    {
        _key = option.Key;
    }

    public override void Reset()
    {
        // Reset JSON overrides even when the native value was already equal
        // to the default. ModSettingsOwner invokes this on Reset to Default.
        OptimizationToggles.Set(_key, DefaultValue);
        base.Reset();
    }
}
