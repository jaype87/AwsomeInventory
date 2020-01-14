using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RPG_Inventory_Remake
{
    public class Setting : ModSettings
    {
        public bool UseLoadout;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref UseLoadout, "UseLoadout");
        }
    }
}
