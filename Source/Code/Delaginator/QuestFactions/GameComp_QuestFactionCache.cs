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
        public static GameComp_QuestFactionCache Comp { get; private set; }

        private readonly HashSet<Quest> activeQuestCache = new HashSet<Quest>();
        private readonly MultiDict<Pawn, ExtraFaction> pawnFactionCache = new MultiDict<Pawn, ExtraFaction>();

        // Note that this constructor has to exist or the game can't create the comp
        public GameComp_QuestFactionCache(Game game) { }

        /// <summary>
        /// Finalizes the initialization.
        /// </summary>
        public override void FinalizeInit()
        {
            base.FinalizeInit();
            Comp = Current.Game.GetComponent<GameComp_QuestFactionCache>();

            foreach (var quest in Find.QuestManager.QuestsListForReading.Where(q => q.State == QuestState.Ongoing))
            {
                QuestInitiated(quest);
            }
        }

        //public override void 

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
            outExtraFactions.Clear();
            outExtraFactions.AddRange(pawnFactionCache[pawn]);
        }
    }
}
