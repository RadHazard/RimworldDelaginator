using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Delaginator.Debug
{
    /// <summary>
    /// A class for getting debug information about world pawns
    /// </summary>
    internal static class WorldPawnDebugInfo
    {
        /// <summary>
        /// Gets the critical reason a pawn is being kept, if any.  Critically-kept pawns are never garbage collected.
        /// Replicates <see cref="WorldPawnGC.GetCriticalPawnReason"/> because that method is private
        /// </summary>
        /// <param name="pawn">The pawn to check</param>
        /// <returns>The critical reason the pawn is being kept, or null if they are not</returns>
        public static string? GetPawnCriticalKeptReason(Pawn pawn)
        {
            if (pawn.Discarded)
                return null;
            if (PawnUtility.EverBeenColonistOrTameAnimal(pawn) && pawn.RaceProps.Humanlike)
                return "Colonist";
            if (PawnGenerator.IsBeingGenerated(pawn))
                return "Generating";
            if (PawnUtility.IsFactionLeader(pawn))
                return "FactionLeader";
            if (PawnUtility.IsKidnappedPawn(pawn))
                return "Kidnapped";
            if (pawn.IsCaravanMember())
                return "CaravanMember";
            if (PawnUtility.IsTravelingInTransportPodWorldObject(pawn))
                return "TransportPod";
            if (PawnUtility.ForSaleBySettlement(pawn))
                return "ForSale";
            if (Find.WorldPawns.ForcefullyKeptPawns.Contains(pawn))
                return "ForceKept";
            if (pawn.SpawnedOrAnyParentSpawned)
                return "Spawned";
            if (!pawn.Corpse.DestroyedOrNull())
                return "CorpseExists";
            if (pawn.RaceProps.Humanlike && Current.ProgramState == ProgramState.Playing)
            {
                if (Find.PlayLog.AnyEntryConcerns(pawn))
                    return "InPlayLog";
                if (Find.BattleLog.AnyEntryConcerns(pawn))
                    return "InBattleLog";
            }
            if (Current.ProgramState == ProgramState.Playing && Find.TaleManager.AnyActiveTaleConcerns(pawn))
                return "InActiveTale";
            if (QuestUtility.IsReservedByQuestOrQuestBeingGenerated(pawn))
                return "ReservedByQuest";
            return Find.WorldPawns.PawnSourced(pawn) ? "CompPawnSource" : null;
        }
        
        /// <summary>
        /// Calculates the pawns being kept alive by the garbage collector
        /// </summary>
        /// <returns>A Dictionary containing the kept pawns and the reason they were kept</returns>
        public static Dictionary<Pawn, string> CalculateKeptPawns()
        {
            Dictionary<Pawn, string> keptPawns = new();
            
            foreach (var pawn in Find.WorldPawns.AllPawnsAliveOrDead)
            {
                var criticalPawnReason = GetPawnCriticalKeptReason(pawn);
                if (criticalPawnReason is not null)
                    keptPawns[pawn] = criticalPawnReason;
            }
            
            foreach (var key in Find.WorldPawns.AllPawnsAlive.Where(IsRandomlyKept)
                         .Where(p => !keptPawns.ContainsKey(p)).Take(10))
                keptPawns[key] = "RandomlyKept";
            
            var criticalPawns = keptPawns.Keys.ToArray();
            foreach (var pawn in criticalPawns)
            {
                AddAllRelationships(pawn, keptPawns);
                CheckForMemories(pawn, keptPawns);
            }

            return keptPawns;
        }
        
        /// <summary>
        /// Calculates the pawns being kept around due to relationships
        /// </summary>
        /// <param name="pawn">The pawn to check the relationships of</param>
        /// <param name="keptPawns"></param>
        private static void AddAllRelationships(Pawn pawn, IDictionary<Pawn, string> keptPawns)
        {
            if (pawn.relations == null)
                return;
            foreach (var relatedPawn in pawn.relations.RelatedPawns)
            {
                if (!keptPawns.ContainsKey(relatedPawn))
                {
                    var relation = relatedPawn.GetRelations(pawn).FirstOrDefault()?.ToString() ??
                                   "(Unknown Relationship)";
                    keptPawns[relatedPawn] = $"Relationship -- {relation} of {GetPawnUniqueName(pawn)}";
                }
            }
        }

        /// <summary>
        /// Calculates the pawns being kept around due to memories
        /// </summary>
        /// <param name="pawn">The pawn to check the memoires of</param>
        /// <param name="keptPawns">The dictionary of kept pawns</param>
        private static void CheckForMemories(Pawn pawn, IDictionary<Pawn, string> keptPawns)
        {
            if (pawn.needs?.mood?.thoughts?.memories == null)
                return;
            foreach (var memory in pawn.needs.mood.thoughts.memories.Memories.Where(memory => memory.otherPawn != null))
            {
                if (!keptPawns.ContainsKey(memory.otherPawn))
                    keptPawns[memory.otherPawn] = $"Memory -- Remembered by {GetPawnUniqueName(pawn)}";
            }
        }

        /// <summary>
        /// Gets a useful unique name of a pawn.  This is their actual name if it exists, or their random load ID if
        /// they don't have one.
        /// </summary>
        /// <returns></returns>
        public static string GetPawnUniqueName(Pawn pawn)
        {
            return pawn.Name?.ToStringFull ?? pawn.GetUniqueLoadID();
        } 

        /// <summary>
        /// The method vanilla uses to decide whether to randomly keep a pawn
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        private static bool IsRandomlyKept(Pawn p)
        {
            return p.RaceProps.Humanlike && Rand.ChanceSeeded(0.25f, p.thingIDNumber ^ 980675837);
        }
    }
}