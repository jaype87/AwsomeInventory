using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Harmony;
using Verse;
using RimWorld;

#if RPG_Inventory_Remake
using RPG_Inventory_Remake.Loadout;
#endif


namespace RPG_Inventory_Remake_Common
{
    [StaticConstructorOnStartup]
    public static class OutfitDatabase_TryDelete
    {
        static OutfitDatabase_TryDelete()
        {
            MethodInfo original = AccessTools.Method(typeof(OutfitDatabase), "TryDelete");
            MethodInfo postfix = AccessTools.Method(typeof(OutfitDatabase_TryDelete), "Postfix");
            Utility._harmony.Patch(original, null, new HarmonyMethod(postfix));
        }

        public static void Postfix(Outfit outfit, AcceptanceReport __result)
        {
            if (__result.Accepted)
            {
                if (outfit is RPGILoadout loadout)
                {
                    LoadoutManager.RemoveLoadout(loadout, true);
                }
            }
        }
    }
}
