// <copyright file="Pawn_GetGizmos_RPGI_Patch.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the GPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System.Collections.Generic;
using System.Reflection;
using AwesomeInventory.Common;
using HarmonyLib;
using Verse;

namespace RPG_Inventory_Remake_Common
{
    /// <summary>
    /// Patch into pawn's gizmos, so to provide a Gear gizmo to open gear tab.
    /// </summary>
    [StaticConstructorOnStartup]
    public static class Pawn_GetGizmos_RPGI_Patch
    {
        static Pawn_GetGizmos_RPGI_Patch()
        {
            MethodInfo original = AccessTools.Method(typeof(Pawn), "GetGizmos");
            MethodInfo postfix = AccessTools.Method(typeof(Pawn_GetGizmos_RPGI_Patch), "Postfix");
            Utility.Harmony.Patch(original, null, new HarmonyMethod(postfix));
        }

        /// <summary>
        /// Patch into pawn's gizmos, so to provide a Gear gizmo to open gear tab.
        /// </summary>
        /// <param name="gizmos"> Gizmos to add to. </param>
        /// <param name="__instance"> Pawn who owns the gizmos. </param>
        /// <returns> A collection of <see cref="Gizmo"/> that will display on screen. </returns>
        public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> gizmos, Pawn __instance)
        {
            if (gizmos == null)
                gizmos = new List<Gizmo>();

            ToggleGearTab toggleGearTab = new ToggleGearTab(typeof(RPG_GearTab));
            foreach (Gizmo gizmo in gizmos)
            {
                yield return gizmo;
            }

            yield return toggleGearTab;
        }
    }
}
