using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Verse;
using UnityEngine;
using Harmony;
using RimWorld;

namespace RPG_Inventory_Remake
{
    public class ModStarter : Mod
    {
        // TODO add a getter method to update realtime value
        public static Setting settings;

        // TODO Revisit ModStarter
        public ModStarter(ModContentPack content) : base(content)
        {
            settings = GetSettings<Setting>();
            if (LoadedModManager.RunningModsListForReading.Any(m => m.Name == "Combat Extended"))
            {
                RPG_GearTab_CE.IsCE = true;
                // typeof(DefOfHelper).GetMethod("BindDefsFor", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, new object[] { typeof(RPGLoadout.CE_StatDefOf) });
            }
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.ColumnWidth = inRect.width / 3;
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
