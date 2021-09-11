using System.Reflection;
using HarmonyLib;
using Verse;

namespace DelaginatorIdeology
{
    [StaticConstructorOnStartup]
    static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            var harmony = new Harmony("pausbrak.delaginator.ideology");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
