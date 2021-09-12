using UnityEngine;
using Verse;

namespace Delaginator
{

    public class DelaginatorMod : Mod
    {
        /// <summary>
        /// A convenience property to get the settings statically
        /// </summary>
        /// <value>The settings.</value>
        public static DelaginatorSettings Settings { get; private set; }

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
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            //TODO translate
            listingStandard.CheckboxLabeled("Quest Factions", ref Settings.getExtraFactions, "Patches the method to get factions from quests with a version that uses a cache");

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

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref getExtraFactions, nameof(getExtraFactions), true);
        }
    }
}
