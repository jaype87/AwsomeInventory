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
