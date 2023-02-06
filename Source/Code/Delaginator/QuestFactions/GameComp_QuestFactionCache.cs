using System.Collections.Generic;
using System.Linq;
using Delaginator.Utilities;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace Delaginator.QuestFactions
{
    /// <summary>
    /// A cache for pawn factions associated with quests
    /// </summary>
    [UsedImplicitly]
    public class GameComp_QuestFactionCache : GameComponent
    {
        private const int CHECK_RATE = 10000; // Once every 4 in-game hours

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
        /// Called every tick
        /// </summary>
        public override void GameComponentTick()
        {
            // Do some error checking every now and again so we can be sure the quest cache isn't breaking because of mods
            var error = false;
            var hashTick = Find.TickManager.TicksGame % CHECK_RATE;

            switch (hashTick)
            {
                // Spread out the error checking over different ticks
                case 0:
                {
                    // Check no inactive quests are in the cache
                    foreach (var quest in activeQuestCache)
                    {
                        if (quest.State != QuestState.Ongoing)
                        {
                            Log.Error($"[Delaginator] Quest {quest.name} was in the active quest cache but is not active" +
                                      $"(was {quest.State}). Is another mod ending quests improperly?");
                            error = true;
                        }
                    }

                    break;
                }
                case CHECK_RATE / 3:
                {
                    // Check all active quests are in cache
                    foreach (var quest in Find.QuestManager.QuestsListForReading)
                    {
                        if (quest.State == QuestState.Ongoing && !activeQuestCache.Contains(quest))
                        {
                            Log.Error($"[Delaginator] Quest {quest.name} was not in the active quest cache but is active." +
                                      " Is another mod starting quests improperly?");
                            error = true;
                        }
                    }

                    break;
                }
                case CHECK_RATE * 2 / 3:
                {
                    // Check all appropriate pawns are in the cache
                    var pawns = activeQuestCache.SelectMany(q => q.PartsListForReading)
                        .OfType<QuestPart_ExtraFaction>()
                        .SelectMany(qp => qp.affectedPawns)
                        .ToHashSet();
                    var cachedPawns = pawnFactionCache.Keys.ToHashSet();

                    if (!pawns.SetEquals(cachedPawns))
                    {
                        foreach (var pawn in pawns.Where(p => !cachedPawns.Contains(p)))
                        {
                            Log.Error($"[Delaginator] Pawn {pawn.LabelShort} should have been in quest faction cache but was not." +
                                      " Is another mod adding pawns to quest factions improperly?");
                            error = true;
                        }

                        foreach (var pawn in cachedPawns.Where(p => !pawns.Contains(p)))
                        {
                            Log.Error($"[Delaginator] Pawn {pawn.Label} was in quest faction cache but should not have been." +
                                      " Is another mod removing pawns from quest factions improperly?");
                            error = true;
                        }
                    }

                    break;
                }
            }

            if (error)
            {
                Log.Warning("[Delaginator] Resetting the quest faction cache. Disable the 'Quest Factions' patch if this error persists.");
                ReloadCaches();
            }
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
        /// Called when a new quest is initiated
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
