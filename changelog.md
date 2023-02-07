# v1.1.0 - 1.4
**General**
- Updated to 1.4
**Debugging**
- Patched WorldPawnGC.AddAllRelationships() to fix an NPE that could occur if there was an issue fetching relationships
- Added a world pawn logging function that displays more information about world pawns, including why they are being kept around.
**Patches - Base game**
- Clean up dead world pawn pregnancies before garbage collection
  - Previously, dead world pawns with pregnancies were keeping themselves permanantly around, because the pregnancy hediff was reserving both parents and never got removed.  This could cascade and cause all of their relatives to be kept around even after they should have been garbage collected.
  - Now, before every garbage collection run, every world pawn that is dead and destroyed (meaning they will never come back) has any pregnancy hediffs removed.  This removes the reservation and allows them and their relatives to be garbage collected as normal.

# v1.0.0 - Initial Release
**Patches - Base game**
- Optimize the "Get Extra Faction" call for pawns
  - Previously, any time a pawn wished to know if they were part of a quest faction (a relatively common check, run every update for every pawn when drawing the colonist bar, among other places), they would need to scan the entire quest list to find active quests, then scan those quests for questparts with factions, then scan the list of associated pawns from those questpart to see if they were in said list.  This scaled with the number of pawns * the number of quests (including archived quests), as well as by the number of pawns * the number of quest pawns for all active quests.
  - Now, the list of active quests is cached in a game component.  The list of quest factions for active quests is also cached.  Both caches are updated only when the quest list changes or pawns are added/removed from a quest faction, so they have no tick overhead.  Fetching the list of active factions for a pawn from the cache is constant-time, and fetching quest factions for an archived quest scales with the number of pawns associated with that archived quest only.
**Patches - Ideology**
- Optimize the "Shared Altar" thoughtworker
  - Previously, pawns would individually scan the map themselves for all altars sharing rooms, which scaled based on the number of pawns * number of things on the map * number of things in the same room as every altar
  - Now, a map component regularly check all altars for sharing rooms, which scales with the number of things on the map only.  Each pawn's thoughtworker now just asks the component for the current status, which scales only with the number of pawns.
