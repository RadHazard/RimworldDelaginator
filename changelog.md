# v1.0.0 - Initial Release
**Patches - Base game**
- Optimize the "Get Extra Faction" call for pawns
  - Previously, any time a pawn wished to know if they were part of a quest faction (a relatively common check, run every update for every pawn when drawing the colonist bar, among other places), they would need to scan the entire quest list to find active quests, then scan those quests for questparts with factions, then scan the list of associated pawns from those questpart to see if they were in said list.  This scaled with the number of pawns * the number of quests (including archived quests), as well as by the number of pawns * the number of quest pawns for all active quests.
  - Now, the list of active quests is cached in a game component.  The list of quest factions for active quests is also cached.  Both caches are updated only when the quest list changes or pawns are added/removed from a quest faction, so they have no tick overhead.  Fetching the list of active factions for a pawn from the cache is constant-time, and fetching quest factions for an archived quest scales with the number of pawns associated with that archived quest only.
**Patches - Ideology**
- Optimize the "Shared Altar" thoughtworker
  - Previously, pawns would individually scan the map themselves for all altars sharing rooms, which scaled based on the number of pawns * number of things on the map * number of things in the same room as every altar
  - Now, a map component regularly check all altars for sharing rooms, which scales with the number of things on the map only.  Each pawn's thoughtworker now just asks the component for the current status, which scales only with the number of pawns.
