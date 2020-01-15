using Harmony;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Verse;

namespace RPG_Inventory_Remake
{
    [StaticConstructorOnStartup]
    public class Pawn_GetGizmos_RPGI_Patch
    {
        private static Vector2 _portrait = new Vector2(128, 128);
        // Gear_Helmet.png Designed By nickfz from <a href="https://pngtree.com/">Pngtree.com</a>
        private static readonly Texture2D _icon = ContentFinder<Texture2D>.Get("UI/Icons/Gear_Helmet_Colored", true);
        static Pawn_GetGizmos_RPGI_Patch()
        {
            MethodInfo original = AccessTools.Method(typeof(Pawn), "GetGizmos");
            MethodInfo postfix = AccessTools.Method(typeof(Pawn_GetGizmos_RPGI_Patch), "Postfix");
            Utility._harmony.Patch(original, null, new HarmonyMethod(postfix));
        }

        public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> gizmos, Pawn __instance)
        {

            ToggleGearTab toggleGearTab = new ToggleGearTab() { icon = _icon };
            foreach (Gizmo gizmo in gizmos)
            {
                yield return gizmo;
            }
            yield return toggleGearTab;
        }
    }
}
