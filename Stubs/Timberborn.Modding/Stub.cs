using System;
using System.Collections.Generic;

namespace Timberborn.Modding
{
    // Signature verified against Timberborn.Modding.dll from game 1.1.2.7.
    // The old IReadOnlyList<Mod> property did not exist at runtime and caused
    // MissingMethodException on startup in TB Optimizer 1.1.115.
    public class ModRepository
    {
        [Obsolete("Never bind EnabledMods directly in TB Optimizer: use runtime reflection with a safe fallback.", true)]
        public IEnumerable<Mod> EnabledMods => Array.Empty<Mod>();
    }

    public class Mod
    {
        public ModManifest Manifest => null!;
    }

    public class ModManifest
    {
        public string Id => "";
    }
}
