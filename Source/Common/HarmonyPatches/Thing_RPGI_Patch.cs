using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony;
using Verse;
using RimWorld;
using System.Reflection;
using System.Diagnostics.CodeAnalysis;

#if RPG_Inventory_Remake
using RPG_Inventory_Remake.Loadout;
#endif

namespace RPG_Inventory_Remake_Common
{
    [StaticConstructorOnStartup]
    public class Thing_RPGI_Patch
    {
        static Thing_RPGI_Patch()
        {
            MethodInfo original = AccessTools.Method(typeof(Thing), "SplitOff");
            MethodInfo postfix = AccessTools.Method(typeof(Thing_RPGI_Patch), "SplitOff_Postfix");
            Utility._harmony.Patch(original, null, new HarmonyMethod(postfix));
        }

        [SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "<Pending>")]
        public static void SplitOff_Postfix(Thing __instance, int count)
        {
            if (__instance.ParentHolder is Pawn pawn)
            {
                if (pawn.TryGetComp<compRPGILoadout>() is compRPGILoadout compRPGI)
                {
                    compRPGI.NotifiedSplitOff(__instance, count);
                }
            }
        }
    }
}
