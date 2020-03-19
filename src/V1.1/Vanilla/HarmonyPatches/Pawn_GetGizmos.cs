// <copyright file="Pawn_GetGizmos.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the GPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System.Collections.Generic;
using System.Reflection;
using AwesomeInventory.UI;
using AwesomeInventory.Utilities;
using HarmonyLib;
using Verse;

namespace RPG_Inventory_Remake
{
    [StaticConstructorOnStartup]
    public class Pawn_GetGizmos_RPGI_Patch
    {

        static Pawn_GetGizmos_RPGI_Patch()
        {
            MethodInfo original = AccessTools.Method(typeof(Pawn), "GetGizmos");
            MethodInfo postfix = AccessTools.Method(typeof(Pawn_GetGizmos_RPGI_Patch), "Postfix");
            Utility.Harmony.Patch(original, null, new HarmonyMethod(postfix));
        }

        public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> gizmos, Pawn __instance)
        {

            ToggleGearTab toggleGearTab = new ToggleGearTab(typeof(AwesomeInventoryTabBase));
            foreach (Gizmo gizmo in gizmos)
            {
                yield return gizmo;
            }
            yield return toggleGearTab;
        }
    }
}
