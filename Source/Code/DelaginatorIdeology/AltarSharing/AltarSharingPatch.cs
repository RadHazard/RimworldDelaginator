﻿using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace DelaginatorIdeology.AltarSharing
{
    [HarmonyPatch(typeof(ThoughtWorker_Precept_AltarSharing))]
    [UsedImplicitly]
    internal static class AltarSharingPatch
    {
        [HarmonyPatch("ShouldHaveThought")]
        [HarmonyPrefix]
        [UsedImplicitly]
        private static bool ShouldHaveThoughtPatch(Pawn p, ref ThoughtState __result)
        {
            if (DelaginatorIdeologyMod.Settings.altarSharing)
            {
                __result = SharedAltar(p) != null;
                return false;
            }
            return true;
        }

        [HarmonyPatch(nameof(ThoughtWorker_Precept_AltarSharing.PostProcessDescription))]
        [HarmonyPrefix]
        [UsedImplicitly]
        private static bool PostProcessDescriptionPatch(Pawn p, string description, ref string __result)
        {
            if (DelaginatorIdeologyMod.Settings.altarSharing)
            {
                __result = description.Formatted(SharedAltar(p).Named("ALTAR"));
                return false;
            }
            return true;
        }

        private static Thing? SharedAltar(Pawn pawn)
        {
            if (!pawn.Spawned || pawn.Ideo == null)
                return null;

            return pawn.Map.GetComponent<MapComp_AltarSharing>()?.SharedAltar(pawn.Ideo);
        }
    }
}
