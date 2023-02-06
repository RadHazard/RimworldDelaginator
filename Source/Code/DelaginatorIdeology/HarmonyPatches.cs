using System.Reflection;
using HarmonyLib;
using JetBrains.Annotations;
using Verse;

namespace DelaginatorIdeology
{
    [StaticConstructorOnStartup]
    [UsedImplicitly]
    internal static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            var harmony = new Harmony("pausbrak.delaginator.ideology");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
