using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Delaginator.WorldPawns
{
    [HarmonyPatch(typeof(WorldPawnGC))]
    [UsedImplicitly]
    internal static class Patches_CleanDeadPregnancies
    {
        /// <summary>
        /// Cleans up the pregnancies of dead, destroyed pawns before every garbage collector pass.  Pregnancy hediffs
        /// always reserve their parents until the hediff is removed, preventing the parents from being garbage
        /// collected.  This is normally fine, but dead world pawns never lose the hediff, meaning they will stick
        /// around forever (as they count as one of the parents and their own reservation is keeping them around).
        ///
        /// This also causes all relative of theirs to never be garbage collected even if nothing else is keeping them
        /// around.  This can be a massive problem in some situations, particularly if you have a mod that lets wild
        /// animals breed.
        /// </summary>
        [HarmonyPatch(nameof(WorldPawnGC.PawnGCPass))]
        [HarmonyPrefix]
        [UsedImplicitly]
        private static void CleanDeadPregnantPawns()
        {
            if (DelaginatorMod.Settings.cleanDeadPregnancies)
            {
                // We only clean up pregnancies of dead, destroyed pawns.
                // These are guaranteed to never show up again under any circumstance.
                foreach (var p in GetDeadPregnantPawns())
                {
                    var pregnancy = p.health.hediffSet.GetFirstHediff<Hediff_Pregnant>();
                    p.health.RemoveHediff(pregnancy);
                }
            }
        }

        /// <summary>
        /// Gets all pawns that are dead, destroyed, and pregnant.
        /// </summary>
        /// <returns>An enumerable of dead, pregnant pawns</returns>
        public static IEnumerable<Pawn> GetDeadPregnantPawns()
        {
            return Find.WorldPawns.AllPawnsDead
                .Where(p => p.Destroyed)
                .Where(p => p.health.hediffSet.HasHediff(HediffDefOf.Pregnant) ||
                            p.health.hediffSet.HasHediff(HediffDefOf.PregnantHuman));
        }
    }
}