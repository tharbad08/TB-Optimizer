using System.Collections.Generic;
namespace Timberborn.Modding
{
    public class ModRepository { public IReadOnlyList<Mod> EnabledMods { get; } = new List<Mod>(); }
    public class Mod { public ModManifest Manifest { get; } = new ModManifest(); }
    public class ModManifest { public string Id { get; set; } = ""; }
}
