using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Delaginator.Debug
{
    [HarmonyPatch(typeof(WorldPawnGC))]
    [UsedImplicitly]
    internal static class Patches_FixGC
    {
        /// <summary>
        /// Fixes an NPE that happens when attempting to log the dotgraph for pawns with invalid relationships 
        /// </summary>
        [HarmonyPatch(nameof(WorldPawnGC.AddAllRelationships))]
        [HarmonyPrefix]
        [UsedImplicitly]
        private static bool AddAllRelationshipsFix(
            WorldPawnGC __instance,
            StringBuilder ___logDotgraph,
            HashSet<string> ___logDotgraphUniqueLinks,
            Pawn pawn,
            Dictionary<Pawn, string> keptPawns)
        {
            if (pawn.relations == null)
                return false;
            foreach (var relatedPawn in pawn.relations.RelatedPawns)
            {
                if (___logDotgraph != null)
                {
                    var str = string.Format("{0}->{1} [label=<{2}> color=\"purple\"];",
                        WorldPawnGC.DotgraphIdentifier(pawn),
                        WorldPawnGC.DotgraphIdentifier(relatedPawn),
                        pawn.GetRelations(relatedPawn).FirstOrDefault()); // An unnecessary ToString here is causing the NPE QwQ
                    if (!___logDotgraphUniqueLinks.Contains(str))
                    {
                        ___logDotgraphUniqueLinks.Add(str);
                        ___logDotgraph.AppendLine(str);
                    }
                }
                if (!keptPawns.ContainsKey(relatedPawn))
                    keptPawns[relatedPawn] = "Relationship";
            }

            return false;
        }
    }
}