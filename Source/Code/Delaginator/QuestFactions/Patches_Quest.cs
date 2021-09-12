using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Delaginator.QuestFactions
{
    [HarmonyPatch(typeof(Quest))]
    public static class Patches_Quest
    {
        [HarmonyPatch(nameof(Quest.Initiate))]
        [HarmonyPostfix]
        private static void InitiatePatch(Quest __instance)
        {
            GameComp_QuestFactionCache.Comp.QuestInitiated(__instance);
        }

        [HarmonyPatch(nameof(Quest.End))]
        [HarmonyPostfix]
        private static void EndPatch(Quest __instance)
        {
            GameComp_QuestFactionCache.Comp.QuestEnded(__instance);
        }
    }
}
