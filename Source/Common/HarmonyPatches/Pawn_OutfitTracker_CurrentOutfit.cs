using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Verse;
using Harmony;
using RimWorld;

#if RPG_Inventory_Remake
using RPG_Inventory_Remake.Loadout;
#endif

namespace RPG_Inventory_Remake_Common
{
    [StaticConstructorOnStartup]
    public static class Pawn_OutfitTracker_CurrentOutfit
    {
        static Pawn_OutfitTracker_CurrentOutfit()
        {
            MethodInfo original = AccessTools.Property(typeof(Pawn_OutfitTracker), "CurrentOutfit").GetSetMethod();
            MethodInfo postfix = AccessTools.Method(typeof(Pawn_OutfitTracker_CurrentOutfit), "Postfix");
            Utility._harmony.Patch(original, null, new HarmonyMethod(postfix));
        }

        [SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "<Pending>")]
        public static void Postfix(Outfit value, Pawn_OutfitTracker __instance)
        {
            if (value is RPGILoadout loadout)
            {
                __instance.pawn.SetLoadout(loadout);
            }
        }
    }
}
