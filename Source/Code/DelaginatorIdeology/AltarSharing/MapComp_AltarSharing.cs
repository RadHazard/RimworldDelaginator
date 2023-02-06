using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace DelaginatorIdeology.AltarSharing
{
    /// <summary>
    /// A map component that regularly checks all altars for desecration (so that pawns don't have to do it themselves)
    /// </summary>
    [UsedImplicitly]
    public class MapComp_AltarSharing : MapComponent
    {
        private const int TICK_INTERVAL = 250;

        private Dictionary<Ideo, Thing> sharedAltars = new();

        private readonly int hashOffset;

        public MapComp_AltarSharing(Map map) : base(map)
        {
            hashOffset = map.uniqueID.HashOffset();
        }

        /// <summary>
        /// Ticks the component
        /// </summary>
        public override void MapComponentTick()
        {
            base.MapComponentTick();
            if (DelaginatorIdeologyMod.Settings.altarSharing && (Find.TickManager.TicksGame + hashOffset) % TICK_INTERVAL == 0)
            {
                RecalcSharedAltars();
            }
        }

        /// <summary>
        /// Exposes the data for saving/loading.
        /// </summary>
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref sharedAltars, nameof(sharedAltars), LookMode.Reference, LookMode.Reference);
        }

        /// <summary>
        /// Returns an altar of the given ideology that's desecrated by the presence of another altar, if one exists
        /// </summary>
        /// <returns>The altar.</returns>
        /// <param name="ideo">Ideo.</param>
        public Thing SharedAltar(Ideo ideo)
        {
            return sharedAltars.TryGetValue(ideo);
        }

        /// <summary>
        /// Recalculates which altars are shared
        /// </summary>
        private void RecalcSharedAltars()
        {
            sharedAltars.Clear();

            // Grab all altars on the map, grouped by room
            // (Vanilla considers any stylable thing with an associated ideology an "Altar" for the purpose of this thought)
            var altarRooms = map.listerThings.ThingsMatching(ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial))
                    .Select(GetThingAndIdeo)
                    .Where(ti => ti.ideo != null)
                    .GroupBy(ti => ti.thing.GetRoom());

            foreach (var room in altarRooms)
            {
                // If this room has more than one ideology represented in it, all the buildings are desecrated
                if (room.GroupBy(ti => ti.ideo).Count() > 1)
                {
                    foreach (var (thing, ideo) in room)
                    {
                        sharedAltars[ideo] = thing;
                    }
                }
            }
        }

        /// <summary>
        /// Returns a tuple containing the thing and its associated ideology, if any
        /// </summary>
        /// <returns>The ideo.</returns>
        /// <param name="thing">Thing.</param>
        private static (Thing thing, Ideo ideo) GetThingAndIdeo(Thing thing)
        {
            return (thing, thing.TryGetComp<CompStyleable>()?.SourcePrecept?.ideo);
        }
    }
}
