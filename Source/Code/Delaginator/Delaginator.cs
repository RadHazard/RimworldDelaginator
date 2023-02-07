using JetBrains.Annotations;
using UnityEngine;
using Verse;

namespace Delaginator
{
    [UsedImplicitly]
    public class DelaginatorMod : Mod
    {
        /// <summary>
        /// A convenience property to get the settings statically
        /// </summary>
        /// <value>The settings.</value> 
        public static DelaginatorSettings Settings { get; private set; } = null!; // Mod classes are instantiated early
                                                                                  // in loading so this shouldn't matter

        public DelaginatorMod(ModContentPack content) : base(content)
        {
            Settings = GetSettings<DelaginatorSettings>();
        }

        /// <summary>
        /// Draws the settings window
        /// </summary>
        /// <param name="inRect">In rect.</param>
        public override void DoSettingsWindowContents(Rect inRect)
        {
            var listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            //TODO translate
            listingStandard.CheckboxLabeled("Quest Factions", ref Settings.getExtraFactions,
                "Patches the method to get factions from quests with a version that uses a cache");
            listingStandard.CheckboxLabeled("Clean Dead Pregnancies", ref Settings.cleanDeadPregnancies,
                "Patches the pawn garbage collector to clean up dead, pregnant world pawns");

            listingStandard.End();
        }

        /// <summary>
        /// Override SettingsCategory to show up in the list of settings.
        /// </summary>
        /// <returns> The (translated) mod name. </returns>
        public override string SettingsCategory()
        {
            //TODO translate
            return "Delaginator";
        }
    }

    /// <summary>
    /// Mod settings for Delaginator
    /// </summary>
    public class DelaginatorSettings : ModSettings
    {
        /// <summary>
        /// The patch for the getting quest factions
        /// </summary>
        public bool getExtraFactions = true;
        
        /// <summary>
        /// The patch for cleaning dead pawn pregnancies
        /// </summary>
        public bool cleanDeadPregnancies = true;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref getExtraFactions, nameof(getExtraFactions), true);
            Scribe_Values.Look(ref cleanDeadPregnancies, nameof(cleanDeadPregnancies), true);
        }
    }
}
