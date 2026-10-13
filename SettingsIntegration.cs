using System;
using Bindito.Core;
using ModSettings.CommonUI;
using ModSettings.Core;
using ModSettings.CoreUI;
using Timberborn.Modding;
using Timberborn.SettingsSystem;
using TimberUi.CommonUi;
using UnityEngine.UIElements;

namespace BenchmarkAndOptimizerV11;

// Keep native Mod Settings rows, tooltips, toggles, disabled states and Reset.
// Only the two noninteractive group headings need a full-width UI element.
[Context("MainMenu")]
public sealed class MainMenuConfig : Configurator
{
    public override void Configure() =>
        this.BindSingleton<OptimizerSettingsOwner>()
            .MultiBindSingleton<IModSettingElementFactory, OptimizerGroupHeaderFactory>();
}

[Context("Game")]
public sealed class GameConfig : Configurator
{
    public override void Configure() =>
        this.BindSingleton<OptimizerSettingsOwner>()
            .MultiBindSingleton<IModSettingElementFactory, OptimizerGroupHeaderFactory>();
}

public sealed class OptimizerSettingsOwner(
    ISettings settings,
    ModSettingsOwnerRegistry registry,
    ModRepository repository
) : ModSettingsOwner(settings, registry, repository)
{
    protected override string ModId => "TBOptimizer";
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
                AddNonPersistentModSetting(new OptimizerGroupHeader(currentGroup));
            }

            // Let the framework's BoolModSettingElementFactory render settings.
            // Presence is evaluated each time the settings owner loads, while
            // activation and saved preferences still require a game restart.
            var available = ModAvailability.IsAvailableInSettings(option.Key, _repository);
            var native = new OptimizerBoolSetting(option, available);
            AddCustomModSetting(native, option.Key);

            // The JSON settings remain the authoritative startup preferences.
            native.SetValue(OptimizationToggles.Enabled(option.Key));
            native.ValueChanged += (_, enabled) =>
                OptimizationToggles.Set(option.Key, enabled);
        }
    }
}

// Native ReadonlyTextModSetting uses a compact settings descriptor row that
// does not fill the panel; centering its text centers only that narrow row.
// A dedicated, nonpersistent section element fills the native panel width.
// It does not replace the framework's interactive setting components.
internal sealed class OptimizerGroupHeader : NonPersistentSetting
{
    internal string Text { get; }

    internal OptimizerGroupHeader(string text)
        : base(ModSettingDescriptor.Create(text)) => Text = text;
}

public sealed class OptimizerGroupHeaderFactory : IModSettingElementFactory
{
    // Higher than built-in factories (priority 0), and only handles our type.
    public int Priority => 20;

    public bool TryCreateElement(ModSetting setting, out IModSettingElement? element)
    {
        if (setting is not OptimizerGroupHeader heading)
        {
            element = null;
            return false;
        }

        var root = new VisualElement();
        root.SetWidthPercent(100f);
        root.SetMargin(18f, 0f, 12f, 0f);

        // TimberUi's built-in large, bold, centered game text keeps the
        // correct font/theme without using unverified Unity IStyle setters.
        var title = root.AddGameLabel(
            heading.Text, size: UiBuilder.GameLabelSize.Big,
            bold: true, centered: true);
        title.SetWidthPercent(100f);

        element = new ModSettingElement(root, setting);
        return true;
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
        // Reset JSON overrides even if the native value is already default.
        OptimizationToggles.Set(_key, DefaultValue);
        base.Reset();
    }
}
