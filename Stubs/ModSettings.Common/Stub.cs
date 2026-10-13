using ModSettings.Core;
namespace ModSettings.Common
{
    public sealed class ReadonlyTextModSetting : NonPersistentSetting
    {
        public ReadonlyTextModSetting(ModSettingDescriptor descriptor, TextSettings settings) : base(descriptor) { }
        public override void Reset() { }
        public sealed class TextSettings { public TextSettings() { } }
    }
}
