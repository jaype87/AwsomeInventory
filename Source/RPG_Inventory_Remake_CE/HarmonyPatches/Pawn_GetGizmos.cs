using Harmony;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Verse;
using RPG_Inventory_Remake_Common;

namespace RPG_Inventory_Remake_CE
{
    [StaticConstructorOnStartup]
    public class Pawn_GetGizmos_RPGI_Patch
    {

        static Pawn_GetGizmos_RPGI_Patch()
        {
            MethodInfo original = AccessTools.Method(typeof(Pawn), "GetGizmos");
            MethodInfo postfix = AccessTools.Method(typeof(Pawn_GetGizmos_RPGI_Patch), "Postfix");
            Utility._harmony.Patch(original, null, new HarmonyMethod(postfix));
        }

        public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> gizmos, Pawn __instance)
        {

            ToggleGearTab toggleGearTab = new ToggleGearTab(typeof(RPG_GearTab_CE));
            foreach (Gizmo gizmo in gizmos)
            {
                yield return gizmo;
            }
            yield return toggleGearTab;
        }
    }
}
