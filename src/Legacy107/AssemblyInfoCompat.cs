// StS2 0.107.1 does not expose the v0.109+ AssemblyInfo helper.
// Reconstruct owner detection from the pre-0.109 ModManager registry rather than assuming unknown assemblies are vanilla.
using System.Reflection;
using MegaCrit.Sts2.Core.Models;

namespace MegaCrit.Sts2.Core.Modding;

internal static class AssemblyInfo
{
    internal static Dictionary<Type, (Mod? mod, bool isBaseGame)>? MockTypes { get; set; }

    internal static Mod? ModForType(Type type, out bool isBaseGame)
    {
        if (MockTypes?.TryGetValue(type, out var mock) == true)
        {
            isBaseGame = mock.isBaseGame;
            return mock.mod;
        }

        Assembly assembly = type.Assembly;
        isBaseGame = assembly == typeof(AbstractModel).Assembly;
        if (isBaseGame)
            return null;

        return ModManager.Mods?.FirstOrDefault(mod => mod.assembly == assembly);
    }
}
