using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace Delaginator.QuestFactions
{
    [HarmonyPatch(typeof(QuestPart_ExtraFaction))]
    [UsedImplicitly]
    internal static class Patches_QuestPart_ExtraFaction
    {
        [HarmonyPatch(nameof(QuestPart_ExtraFaction.Notify_QuestSignalReceived))]
        [HarmonyPostfix]
        [UsedImplicitly]
        private static void Notify_QuestSignalReceivedPatch(Signal signal, QuestPart_ExtraFaction __instance)
        {
            // Notify the cache if a pawn is removed from the faction
            if (signal.tag == __instance.inSignalRemovePawn && signal.args.TryGetArg("SUBJECT", out Pawn pawn))
                GameComp_QuestFactionCache.Comp.PawnRemoved(pawn, __instance.extraFaction);
        }

        [HarmonyPatch(nameof(QuestPart_ExtraFaction.ReplacePawnReferences))]
        [HarmonyPostfix]
        [UsedImplicitly]
        private static void ReplacePawnReferencesPatch(Pawn replace, Pawn with, QuestPart_ExtraFaction __instance)
        {
            if (__instance.affectedPawns.Contains(with)) // Only call this if the pawn was actually replaced
            {
                // Notify the cache of the removed and added pawns
                GameComp_QuestFactionCache.Comp.PawnRemoved(replace, __instance.extraFaction);
                GameComp_QuestFactionCache.Comp.PawnAdded(with, __instance.extraFaction);
            }
        }
    }
}
