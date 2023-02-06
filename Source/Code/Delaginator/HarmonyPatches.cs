using System.Reflection;
using HarmonyLib;
using JetBrains.Annotations;
using Verse;

namespace Delaginator
{
    [StaticConstructorOnStartup]
    [UsedImplicitly]
    internal static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            var harmony = new Harmony("pausbrak.delaginator");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
