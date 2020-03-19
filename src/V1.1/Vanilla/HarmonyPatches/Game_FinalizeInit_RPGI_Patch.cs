using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using System.Reflection;
using Verse;

namespace RPG_Inventory_Remake
{
    [StaticConstructorOnStartup]
    public class Game_FinalizeInit_RPGI_Patch
    {
        static Game_FinalizeInit_RPGI_Patch()
        {
            MethodInfo original = AccessTools.Method(typeof(Game), "FinalizeInit");
            MethodInfo postfix = AccessTools.Method(typeof(Game_FinalizeInit_RPGI_Patch), "Postfix");
            Utility._harmony.Patch(original, null, new HarmonyMethod(postfix));
        }

        public static void Postfix()
        {
            JobGiver_AwesomeInventory_Unload.JobInProgress = false;
            if (AccessTools.TypeByName("CombatExtended.ITab_Inventory") != null)
            {
                RPG_GearTab_CE.IsCE = true;
            }
        }
    }
}
