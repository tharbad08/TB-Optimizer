using System;
using Bindito.Core;
using ModSettings.Core;
using ModSettings.Common;
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

    private readonly ModRepository _repository = repository;

    protected override void OnBeforeLoad()
    {
        string? currentGroup = null;

        foreach (var option in OptimizationToggles.Options)
        {
            if (!string.Equals(currentGroup, option.Group, StringComparison.Ordinal))
            {
                currentGroup = option.Group;
                AddNonPersistentModSetting(new ReadonlyTextModSetting(
                    ModSettingDescriptor.Create(currentGroup),
                    new ReadonlyTextModSetting.TextSettings()));
            }

            // Native BoolModSettingElementFactory owns the toggle layout,
            // binding and disabled appearance. Cache presence for this panel:
            // mod activation already requires a game restart.
            var available = ModAvailability.IsAvailableInSettings(option.Key, _repository);
            var native = new OptimizerBoolSetting(option, available);
            AddCustomModSetting(native, option.Key);

            // Prefer existing JSON settings during migration. Native ISettings
            // receives this value before we register the JSON change callback,
            // so initialization cannot accidentally overwrite user opt-outs.
            native.SetValue(OptimizationToggles.Enabled(option.Key));
            native.ValueChanged += (_, enabled) =>
                OptimizationToggles.Set(option.Key, enabled);

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
