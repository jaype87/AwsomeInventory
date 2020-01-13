using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using UnityEngine;

namespace RPG_Inventory_Remake
{
    public class ModStarter : Mod
    {
        public static Setting settings;

        public ModStarter(ModContentPack content) : base(content)
        {
            settings = GetSettings<Setting>();
            JobGiver_RPGIUnload.JobInProgress = false;
            if (LoadedModManager.RunningModsListForReading.Any(m => m.Name == "Combat Extended"))
            {
                RPG_GearTab_CE.IsCE = true;
            }
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.CheckboxLabeled("Corgi_UseLoadout".Translate(), ref settings.UseLoadout, null);
            listingStandard.End();

            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "RPG Style Inventory Remake";
        }
    }
}
