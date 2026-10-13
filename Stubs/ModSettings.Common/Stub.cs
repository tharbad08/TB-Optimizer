using ModSettings.Core;
namespace ModSettings.Common
{
    public sealed class ReadonlyTextModSetting : NonPersistentSetting
    {
        public ReadonlyTextModSetting(ModSettingDescriptor descriptor, TextSettings settings) : base(descriptor) { }
        public override void Reset() { }

        // This is a compile-only type stub, NOT a substitute for the real
        // runtime .ctor(UnityEngine.TextAnchor, int, UnityEngine.Vector4).
        // A private ctor makes accidentally calling a fake public .ctor()
        // a compilation error, preventing the startup crash in v1.1.114.
        public sealed class TextSettings
        {
            private TextSettings() { }
        }
    }
}
