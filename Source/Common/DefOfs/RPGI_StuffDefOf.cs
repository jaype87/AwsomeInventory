using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace RPG_Inventory_Remake_Common
{
    [DefOf]
    public static class RPGI_StuffDefOf
    {
        public static ThingDef RPGIGenericResource;

        static RPGI_StuffDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(RPGI_StuffDefOf));
        }
    }
}
