using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Delaginator.QuestFactions
{
    [HarmonyPatch(typeof(QuestUtility))]
    public static class Patches_QuestUtility
    {
        [HarmonyPatch(nameof(QuestUtility.GetExtraFactionsFromQuestParts))]
        [HarmonyPrefix]
        private static bool GetExtraFactionsFromQuestPartsPatch(Pawn pawn, List<ExtraFaction> outExtraFactions, Quest forQuest)
        {
            if (DelaginatorMod.Settings.getExtraFactions)
            {
                GameComp_QuestFactionCache.Comp.GetExtraFactionsCached(pawn, outExtraFactions, forQuest);
                return false;
            }
            return true;
        }
    }
}
