using System.Collections.Generic;
using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace Delaginator.QuestFactions
{
    [HarmonyPatch(typeof(Quest))]
    [UsedImplicitly]
    internal static class Patches_Quest
    {
        [HarmonyPatch(nameof(Quest.Initiate))]
        [HarmonyPostfix]
        [UsedImplicitly]
        private static void InitiatePatch(Quest __instance)
        {
            GameComp_QuestFactionCache.Comp.QuestInitiated(__instance);
        }

        [HarmonyPatch(nameof(Quest.End))]
        [HarmonyPostfix]
        [UsedImplicitly]
        private static void EndPatch(Quest __instance)
        {
            GameComp_QuestFactionCache.Comp.QuestEnded(__instance);
        }
    }
}
