using ModSettings.Core; using ModSettings.CoreUI; using UnityEngine.UIElements;
namespace ModSettings.CommonUI
{
    public sealed class ModSettingElement : IModSettingElement
    {
        public ModSettingElement(VisualElement root, ModSetting modSetting) { Root=root; ModSetting=modSetting; }
        public VisualElement Root { get; }
        public ModSetting ModSetting { get; }
        public bool ShouldBlockInput => false;
    }
}

namespace ModSettings.Common { public sealed class ReadonlyTextModSetting : ModSettings.Core.NonPersistentSetting { public ReadonlyTextModSetting(ModSettings.Core.ModSettingDescriptor descriptor, TextSettings settings):base(descriptor) {} public sealed class TextSettings { public TextSettings() {} } } }
