using System.Reflection;
using HarmonyLib;
using Verse;

namespace Delaginator
{
    [StaticConstructorOnStartup]
    static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            var harmony = new Harmony("pausbrak.delaginator");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
