using System.Linq;
using RimWorld;
using Verse;

namespace DelaginatorIdeology.AltarSharing
{
    /// <summary>
    /// A comp for calculating whether an altar is sharing a room with another altar (for desecration checks)
    /// </summary>
    public class Comp_AltarSharing : ThingComp
    {
        [Unsaved]
        private CompStyleable compStyleable;

        private CompStyleable CompStyleable
        {
            get
            {
                if (compStyleable == null)
                    compStyleable = parent.TryGetComp<CompStyleable>();
                return compStyleable;
            }
        }

        /// <summary>
        /// Whether or not this altar is sharing a room with another altar
        /// </summary>
        /// <value><c>true</c> if is shared altar; otherwise, <c>false</c>.</value>
        public bool IsSharedAltar { get; private set; }

        /// <summary>
        /// Whether or not this altar is sharing a room with another altar
        /// </summary>
        /// <value><c>true</c> if is shared altar; otherwise, <c>false</c>.</value>
        public Ideo Ideology => CompStyleable?.SourcePrecept?.ideo;

        /// <summary>
        /// Tick that happens every tick
        /// </summary>
        public override void CompTick()
        {
            base.CompTick();
            // Things only have one kind of tick.  If we're ticking here, CompTickLong will never happen naturally and
            // so we need to call it ourselves
            if (parent.IsHashIntervalTick(2000))
                CompTickLong();
        }


        /// <summary>
        /// Tick that happens only once every 250 ticks
        /// </summary>
        public override void CompTickRare()
        {
            base.CompTickRare();
            // Things only have one kind of tick.  If we're ticking here, CompTickLong will never happen naturally and
            // so we need to call it ourselves
            if (parent.IsNestedHashIntervalTick(250, 2000))
                CompTickLong();
        }

        /// <summary>
        /// Tick that happens only once every 2000 ticks
        /// </summary>
        public override void CompTickLong()
        {
            base.CompTickLong();
            IsSharedAltar = false;

            if (DelaginatorIdeologyMod.Settings.altarSharing && Ideology != null)
            {
                Room room = parent.GetRoom(RegionType.Set_All);
                if (room != null && !room.TouchesMapEdge)
                {
                    foreach (Thing thing in room.ContainedAndAdjacentThings.Where(t => t != parent))
                    {
                        CompStyleable thingStylable = thing.TryGetComp<CompStyleable>();
                        if (thingStylable?.SourcePrecept?.ideo != Ideology)
                        {
                            IsSharedAltar = true;
                            break;
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Comp properties for the altar sharing comp.
    /// </summary>
    public class CompProperties_AltarSharing : CompProperties
    {
        public CompProperties_AltarSharing()
        {
            compClass = typeof(Comp_AltarSharing);
        }
    }
}
