using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace RPG_Inventory_Remake_Common
{
    [DefOf]
    public static class RPGI_JobDefOf
    {
        public static JobDef RPGI_Unload;
        public static JobDef RPGI_Fake;
        public static JobDef RPGI_ApparelOptions;
        public static JobDef RPGI_Map_Equip;


        static RPGI_JobDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(RPGI_JobDefOf));
        }
    }
}
