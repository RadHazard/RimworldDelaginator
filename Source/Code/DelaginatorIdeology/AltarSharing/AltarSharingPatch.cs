using HarmonyLib;
using RimWorld;
using Verse;

namespace DelaginatorIdeology.AltarSharing
{
    [HarmonyPatch(typeof(ThoughtWorker_Precept_AltarSharing))]
    static class Patch_ThoughtWorker_Precept_AltarSharing
    {
        [HarmonyPatch("ShouldHaveThought")]
        [HarmonyPrefix]
        private static bool ShouldHaveThoughtPatch(Pawn p, ref ThoughtState __result)
        {
            if (DelaginatorIdeologyMod.Settings.altarSharing)
            {
                __result = SharedAltar(p) != null;
                return false;
            }
            return true;
        }

        [HarmonyPatch("PostProcessDescription")]
        [HarmonyPrefix]
        private static bool PostProcessDescriptionPatch(Pawn p, string description, ref string __result)
        {
            if (DelaginatorIdeologyMod.Settings.altarSharing)
            {
                __result = description.Formatted(SharedAltar(p).Named("ALTAR"));
                return false;
            }
            return true;
        }

        private static Thing SharedAltar(Pawn pawn)
        {
            if (!pawn.Spawned || pawn.Ideo == null)
                return null;

            foreach (Thing item in pawn.Map.listerThings.ThingsMatching(ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial)))
            {
                Comp_AltarSharing compStyleable = item.TryGetComp<Comp_AltarSharing>();
                if (compStyleable != null && compStyleable.Ideology == pawn.Ideo && compStyleable.IsSharedAltar)
                    return item;
            }
            return null;
        }
    }
}
