using Verse;
using Verse.AI;
using HarmonyLib;
using System.Reflection;
using RPG_Inventory_Remake_Common;

namespace RPG_Inventory_Remake
{
    [StaticConstructorOnStartup]
    public class Pawn_CarryTracker_CarriedThing_RPGI_Patch
    {
        static Pawn_CarryTracker_CarriedThing_RPGI_Patch()
        {
            MethodInfo original = AccessTools.Method(typeof(Pawn_CarryTracker), "get_CarriedThing");
            MethodInfo postfix = AccessTools.Method(typeof(Pawn_CarryTracker_CarriedThing_RPGI_Patch), "Postfix");
            Utility.Harmony.Patch(original, null, new HarmonyMethod(postfix));
        }

        public static void Postfix(Pawn_CarryTracker __instance, ref Thing __result)
        {
            Job job = __instance.pawn.CurJob;
            if (job != null)
            {
                if (job.def == RPGI_JobDefOf.RPGI_Unload)
                {
                    if (job.targetA.HasThing)
                    {
                        __result = job.targetA.Thing;
                    }
                }
            }
        }
    }
}
