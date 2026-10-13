using System.Collections.Generic;
namespace Timberborn.Modding
{
    public class ModRepository { [System.Obsolete("Never call EnabledMods directly: compile stub return type does not match Timberborn 1.1 runtime; use reflection.", true)] public IReadOnlyList<Mod> EnabledMods { get; } = new List<Mod>(); }
    public class Mod { public ModManifest Manifest { get; } = new ModManifest(); }
    public class ModManifest { public string Id { get; set; } = ""; }
}
