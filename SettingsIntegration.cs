using Bindito.Core;
using ModSettings.CommonUI;
using ModSettings.Core;
using ModSettings.CoreUI;
using Timberborn.CoreUI;
using Timberborn.Modding;
using Timberborn.Options;
using Timberborn.OptionsGame;
using Timberborn.SettingsSystem;
using Timberborn.SingletonSystem;
using TimberUi.CommonUi;
using UnityEngine.UIElements;

namespace BenchmarkAndOptimizerV11;

[Context("MainMenu")]
public sealed class MainMenuConfig : Configurator
{
    public override void Configure() =>
        this.BindSingleton<OptimizerSettingsOwner>()
            .MultiBindSingleton<IModSettingElementFactory, OptimizerSettingElementFactory>();
}

[Context("Game")]
public sealed class GameConfig : Configurator
{
    public override void Configure() =>
        this.BindSingleton<OptimizerSettingsOwner>()
            .MultiBindSingleton<IModSettingElementFactory, OptimizerSettingElementFactory>();
}

public sealed class OptimizerSettingsOwner(
    ISettings settings,
    ModSettingsOwnerRegistry registry,
    ModRepository repository
) : ModSettingsOwner(settings, registry, repository)
{
    public override string ModId => "TBOptimizer";
    public override ModSettingsContext ChangeableOn => ModSettingsContext.All;
    public TBOptimizerUiSetting Optimizations { get; } = new();
}

public sealed class TBOptimizerUiSetting()
    : NonPersistentSetting(ModSettingDescriptor.Create("TB Optimizer"))
{
    // Native Mod Settings calls this from its Reset to Default button.
    public override void Reset() => OptimizationToggles.ResetToDefaults();
}

/// <summary>
/// Uses the native Mod Settings window (590px default) and its existing outer
/// ScrollView. Do not set ModSettingsBox width globally: other mods, including
/// TimberCommons, share the same singleton settings window.
/// </summary>
public sealed class TBOptimizerPanel : VisualElement
{
    public TBOptimizerPanel()
    {
        this.SetPadding(10f);
        this.AddGameLabel("TB Optimizer", bold: true, centered: true)
            .SetMarginBottom(4f);
        this.AddGameLabel("Changes apply after restarting Timberborn.", centered: true)
            .SetMarginBottom(10f);

        // ModSettingsBox already hosts its content inside a vertical ScrollView.
        // An inner ScrollView causes double-scroll areas and clipped explanations.
        var list = this.AddChild();
        list.SetWidthPercent(100f);

        string? currentGroup = null;
        foreach (var item in OptimizationToggles.Options)
        {
            if (currentGroup != item.Group)
            {
                currentGroup = item.Group;
                list.AddGameLabel(currentGroup, bold: true, centered: true)
                    .SetMargin(17f, 0f, 12f, 0f);
            }

            var setting = list.AddChild().SetMarginBottom(9f);
            setting.SetWidthPercent(100f);

            // Match Mod Settings' native two-column rows, with a left label
            // and a right-aligned checkbox, rather than packing both together.
            var header = setting.AddRow();
            header.AddToClassList("mod-setting__wrapper");
            header.SetWidthPercent(100f);

            var caption = header.AddGameLabel(item.Label);
            caption.SetFlexGrow(1f);
            caption.style.whiteSpace = WhiteSpace.Normal;

            var key = item.Key;
            var toggle = header.AddToggle(
                "", onValueChanged: value => OptimizationToggles.Set(key, value));
            toggle.SetMarginRight(12f);
            toggle.SetValueWithoutNotify(
                ModAvailability.IsAvailable(key) && OptimizationToggles.Enabled(key));

            var available = ModAvailability.IsAvailable(key);
            toggle.SetEnabled(available);

            // Short explanations were a deliberate design requirement: retain
            // them under the setting, but visually de-emphasize secondary text.
            var explanation = setting.AddGameLabel(item.Description);
            explanation.SetMargin(0f, 25f, 2f, 0f);
            explanation.style.whiteSpace = WhiteSpace.Normal;
            explanation.style.opacity = 0.78f;

            if (key == "GcSmoother")
            {
                var status = setting.AddGameLabel(IncrementalGcSmoother.StatusDescription);
                status.SetMargin(0f, 25f, 2f, 0f);
                status.style.whiteSpace = WhiteSpace.Normal;
            }
            else if (!available)
            {
                var status = setting.AddGameLabel("Mod not loaded — setting unavailable.");
                status.SetMargin(0f, 25f, 2f, 0f);
                status.style.whiteSpace = WhiteSpace.Normal;
            }
        }
    }
}

public sealed class OptimizerSettingElementFactory : IModSettingElementFactory
{
    public int Priority { get; }
    public bool TryCreateElement(ModSetting setting, out IModSettingElement? element)
    {
        if (setting is TBOptimizerUiSetting)
        {
            element = new ModSettingElement(new TBOptimizerPanel(), setting);
            return true;
        }
        element = null;
        return false;
    }
}
