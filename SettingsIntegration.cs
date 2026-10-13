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
                // Mod Settings v1.1 defines TextSettings(TextAnchor, int, Vector4)
                // with optional source arguments, NOT a parameterless CLR ctor.
                // Discover its runtime constructor instead of relying on a stub ABI.
                try
                {
                    AddNonPersistentModSetting(new ReadonlyTextModSetting(
                        ModSettingDescriptor.Create(currentGroup),
                        NativeTextHeaderSettings.Create()));
                }
                catch (Exception ex)
                {
                    // A future Mod Settings change should remove the heading,
                    // never prevent the entire Timberborn main menu from loading.
                    Console.WriteLine("[TBOptimizer] Native section header skipped: " + ex);
                }
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


// The real eMka ModSettings.Common.TextSettings ctor requires parameters even
// though its C# definition gives those parameters default values. The old
// stub exposed an imaginary .ctor(), causing MissingMethodException on start.
// Reflection is done only once per group header, never in gameplay hot paths.
internal static class NativeTextHeaderSettings
{
    internal static ReadonlyTextModSetting.TextSettings Create()
    {
        var textSettingsType = typeof(ReadonlyTextModSetting.TextSettings);
        var constructor = Array.Find(
            textSettingsType.GetConstructors(),
            item => item.GetParameters().Length == 3);
        if (constructor is null)
            throw new MissingMethodException(
                textSettingsType.FullName,
                ".ctor(TextAnchor, int, Vector4)");

        var parameters = constructor.GetParameters();
        var arguments = new object?[parameters.Length];
        for (var i = 0; i < parameters.Length; i++)
        {
            var parameter = parameters[i];
            var type = parameter.ParameterType;
            if (type.IsEnum && parameter.Name == "textAlignment")
                arguments[i] = Enum.Parse(type, "MiddleCenter");
            else if (type == typeof(int) && parameter.Name == "fontSize")
                arguments[i] = 16;
            else if (parameter.HasDefaultValue &&
                     parameter.DefaultValue != DBNull.Value &&
                     parameter.DefaultValue != System.Reflection.Missing.Value)
                arguments[i] = parameter.DefaultValue;
            else
                arguments[i] = type.IsValueType ? Activator.CreateInstance(type) : null;
        }

        return (ReadonlyTextModSetting.TextSettings)constructor.Invoke(arguments);
    }
}
