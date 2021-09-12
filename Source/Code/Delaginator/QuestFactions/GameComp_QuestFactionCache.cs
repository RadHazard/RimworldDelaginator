using System.Collections.Generic;
using System.Linq;
using Delaginator.Utilities;
using RimWorld;
using Verse;

namespace Delaginator.QuestFactions
{
    /// <summary>
    /// A cache for pawn factions associated with quests
    /// </summary>
    public class GameComp_QuestFactionCache : GameComponent
    {
        // Cache the comp because it gets referenced a lot
        // Note that the cache must be reset any time a new game is loaded
        public static GameComp_QuestFactionCache Comp { get; private set; }

        private readonly HashSet<Quest> activeQuestCache = new HashSet<Quest>();
        private readonly MultiDict<Pawn, ExtraFaction> pawnFactionCache = new MultiDict<Pawn, ExtraFaction>();

        private bool loaded;

        public GameComp_QuestFactionCache(Game game)
        {
            Comp = this;
        }

        /// <summary>
        /// Reloads the quest cache
        /// </summary>
        public void ReloadCaches()
        {
            activeQuestCache.Clear();
            pawnFactionCache.Clear();
            foreach (var quest in Find.QuestManager.QuestsListForReading.Where(q => q.State == QuestState.Ongoing))
            {
                QuestInitiated(quest);
            }
        }

        /// <summary>
        /// Called when a new quest is initated
        /// </summary>
        /// <param name="quest">Quest.</param>
        internal void QuestInitiated(Quest quest)
        {
            activeQuestCache.Add(quest);
            foreach (var questPart in quest.PartsListForReading.OfType<QuestPart_ExtraFaction>())
                foreach (var pawn in questPart.affectedPawns)
                    PawnAdded(pawn, questPart.extraFaction);
        }

        /// <summary>
        /// Called when a quest is ended
        /// </summary>
        /// <param name="quest">Quest.</param>
        internal void QuestEnded(Quest quest)
        {
            activeQuestCache.Remove(quest);
            foreach (var questPart in quest.PartsListForReading.OfType<QuestPart_ExtraFaction>())
                foreach (var pawn in questPart.affectedPawns)
                    PawnRemoved(pawn, questPart.extraFaction);
        }

        /// <summary>
        /// Called when a pawn is added to an extra faction
        /// </summary>
        /// <param name="pawn">Pawn.</param>
        /// <param name="extraFaction">Extra faction.</param>
        internal void PawnAdded(Pawn pawn, ExtraFaction extraFaction)
        {
            pawnFactionCache.Add(pawn, extraFaction);
        }

        /// <summary>
        /// Called when a pawn is removed from an extra faction
        /// </summary>
        /// <param name="pawn">Pawn.</param>
        /// <param name="extraFaction">Extra faction.</param>
        internal void PawnRemoved(Pawn pawn, ExtraFaction extraFaction)
        {
            pawnFactionCache.Remove(pawn, extraFaction);
        }

        /// <summary>
        /// Gets the extra factions associated with a pawn
        /// </summary>
        /// <param name="pawn">Pawn.</param>
        /// <param name="outExtraFactions">Out extra factions.</param>
        /// <param name="forQuest">For quest.</param>
        public void GetExtraFactionsCached(Pawn pawn, List<ExtraFaction> outExtraFactions, Quest forQuest)
        {
            // This is annoying, but we can't do the cache loading in the initialization. The game initializes maps before
            // game components are initialized, and the maps trigger a WealthWatcher recount on initialization, which uses
            // this method.  WorldComponents are initialized earlier, but that happens so early it's before references are
            // even resolved, so we wouldn't have been able to build the cache at that point.
            // Ludeon, why isn't there a PostResolveReferences() method? :(
            if (!loaded)
            {
                ReloadCaches();
                loaded = true;
            }

            // Note that the faction cache includes all factions from all Ongoing quests.
            // Vanilla always checks those, even if forQuest is set
            outExtraFactions.Clear();
            outExtraFactions.AddRange(pawnFactionCache[pawn]);

            // If the quest is ongoing, factions were already included in the cache.
            // If not, we need to add them ourselves
            if (forQuest != null && forQuest.State != QuestState.Ongoing)
            {
                outExtraFactions.AddRange(forQuest.PartsListForReading
                        .OfType<QuestPart_ExtraFaction>()
                        .Where(qp => qp.affectedPawns.Contains(pawn))
                        .Select(qp => qp.extraFaction));
            }

        }
    }
}
