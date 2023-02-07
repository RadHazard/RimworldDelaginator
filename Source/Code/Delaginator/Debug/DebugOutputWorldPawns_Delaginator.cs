using System;
using System.Globalization;
using System.Linq;
using System.Text;
using Delaginator.WorldPawns;
using JetBrains.Annotations;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Delaginator.Debug
{
    [UsedImplicitly]
    public static class DebugOutputWorldPawns_Delaginator
    {
        private const string CATEGORY = "World pawns - Delaginator";
        
        /// <summary>
        /// Logs all the world pawns individually, along with the reason they are being kept around (if any)
        /// </summary>
        [DebugOutput(CATEGORY, true)]
        [UsedImplicitly]
        public static void LogWorldPawnsWithReason()
        {
            var worldPawns = Find.WorldPawns;
            var keepReasons = WorldPawnDebugInfo.CalculateKeptPawns();
            
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("======= World Pawns =======");
            foreach (var situation in (WorldPawnSituation[]) Enum.GetValues(typeof (WorldPawnSituation)))
            {
                if (situation != WorldPawnSituation.None)
                {
                    stringBuilder.AppendLine();
                    stringBuilder.AppendLine($"==== {situation} ====");
                    foreach (var factionGroup in worldPawns.GetPawnsBySituation(situation)
                                 .GroupBy(x => x.Faction)
                                 .OrderBy(g => g.Key?.Name ?? ""))
                    {
                        stringBuilder.AppendLine($"---- {factionGroup.Key?.Name ?? "(None)"} ----");
                        foreach (var p in factionGroup.OrderBy(WorldPawnDebugInfo.GetPawnUniqueName))
                        {
                            AppendPawnInfo(p, stringBuilder);
                            if (keepReasons.TryGetValue(p, out var reason))
                                stringBuilder.Append($" [{reason}]");
                            stringBuilder.AppendLine();

                        }
                    }
                }
            }
            stringBuilder.AppendLine("===========================");
            Log.Message(stringBuilder.ToString());
        }
        
        /// <summary>
        /// Logs all world pawns that are pregnant.  Pregnancies reserve their parents from the garbage collector, and
        /// so pregnancies can cause pawns to stick around even when they'd otherwise be garbage collected.
        /// </summary>
        [DebugOutput(CATEGORY, true)]
        [UsedImplicitly]
        public static void LogPregnantWorldPawns()
        {
            var worldPawns = Find.WorldPawns;
            var keepReasons = WorldPawnDebugInfo.CalculateKeptPawns();
            
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("======= World Pawns =======");
            foreach (var situation in (WorldPawnSituation[]) Enum.GetValues(typeof (WorldPawnSituation)))
            {
                if (situation != WorldPawnSituation.None)
                {
                    stringBuilder.AppendLine();
                    stringBuilder.AppendLine($"==== {situation} ====");
                    foreach (var factionGroup in worldPawns.GetPawnsBySituation(situation)
                                 .GroupBy(x => x.Faction)
                                 .OrderBy(g => g.Key?.Name ?? ""))
                    {
                        stringBuilder.AppendLine($"---- {factionGroup.Key?.Name ?? "(None)"} ----");
                        foreach (var p in factionGroup
                                     .Where(p => p.health.hediffSet.HasHediff(HediffDefOf.Pregnant) ||
                                                 p.health.hediffSet.HasHediff(HediffDefOf.PregnantHuman))
                                     .OrderBy(WorldPawnDebugInfo.GetPawnUniqueName))
                        {
                            AppendPawnInfo(p, stringBuilder);
                            if (keepReasons.TryGetValue(p, out var reason))
                                stringBuilder.Append($" [{reason}]");
                            stringBuilder.AppendLine();

                        }
                    }
                }
            }
            stringBuilder.AppendLine("===========================");
            Log.Message(stringBuilder.ToString());
        }
        
        /// <summary>
        /// Forcefully cleans up all pregnancies on dead, destroyed world pawns.  These were not getting cleaned up,
        /// and the parent reservation was causing them and all their relatives to stick around forever.
        /// </summary>
        [DebugOutput(CATEGORY, true)]
        [UsedImplicitly]
        public static void AbortDeadPregnantWorldPawns()
        {
            var keepReasons = WorldPawnDebugInfo.CalculateKeptPawns();
            
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("======= Pawns Aborted =======");
            foreach (var factionGroup in Patches_CleanDeadPregnancies.GetDeadPregnantPawns()
                         .GroupBy(p => p.Faction)
                         .OrderBy(f => f.Key?.Name ?? ""))
            {
                stringBuilder.AppendLine($"---- {factionGroup.Key?.Name ?? "(None)"} ----");
                foreach (var p in factionGroup.OrderBy(WorldPawnDebugInfo.GetPawnUniqueName))
                {
                    var pregnancy = p.health.hediffSet.GetFirstHediff<Hediff_Pregnant>();
                    p.health.RemoveHediff(pregnancy);
                    
                    AppendPawnInfo(p, stringBuilder);
                    stringBuilder.AppendLine();
                }
            }

            stringBuilder.AppendLine("===========================");
            Log.Message(stringBuilder.ToString());
        }

        private static void AppendPawnInfo(Pawn pawn, StringBuilder stringBuilder)
        {
            stringBuilder.Append($"{WorldPawnDebugInfo.GetPawnUniqueName(pawn)}, {pawn.KindLabel}");
            if (pawn.royalty != null && pawn.royalty.AllTitlesForReading.Count > 0)
            {
                stringBuilder.Append(", ");
                var titles = pawn.royalty.AllTitlesForReading
                    .Select(t => t.def.GetLabelFor(pawn));
                stringBuilder.Append(string.Join(", ", titles));
            }

            stringBuilder.Append(
                $"({pawn.ageTracker.AgeBiologicalYearsFloat.ToString(CultureInfo.InvariantCulture)})");
        }
    }
}