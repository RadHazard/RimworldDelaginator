using JetBrains.Annotations;
using UnityEngine;
using Verse;

namespace DelaginatorIdeology
{
    [UsedImplicitly]
    public class DelaginatorIdeologyMod : Mod
    {
        /// <summary>
        /// A convenience property to get the settings statically
        /// </summary>
        /// <value>The settings.</value>
        public static DelaginatorIdeologySettings Settings { get; private set; }

        public DelaginatorIdeologyMod(ModContentPack content) : base(content)
        {
            Settings = GetSettings<DelaginatorIdeologySettings>();
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
            listingStandard.CheckboxLabeled("Altar Sharing", ref Settings.altarSharing, "Patches the Shared Altar thought with a more performant check");

            listingStandard.End();
        }

        /// <summary>
        /// Override SettingsCategory to show up in the list of settings.
        /// </summary>
        /// <returns> The (translated) mod name. </returns>
        public override string SettingsCategory()
        {
            //TODO translate
            return "Delaginator - Ideology";
        }
    }

    /// <summary>
    /// Mod settings for Delaginator
    /// </summary>
    public class DelaginatorIdeologySettings : ModSettings
    {
        /// <summary>
        /// The patch for the Altar Sharing thought
        /// </summary>
        public bool altarSharing = true;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref altarSharing, nameof(altarSharing), true);
        }
    }
}
