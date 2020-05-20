// <copyright file="Pawn_GetGizmos_AwesomeInventory_Patch.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AwesomeInventory.Loadout;
using AwesomeInventory.UI;
using HarmonyLib;
using RimWorld;
using Verse;

namespace AwesomeInventory.HarmonyPatches
{
    /// <summary>
    /// Patch into pawn's gizmos, so to provide a Gear gizmo to open gear tab.
    /// </summary>
    [StaticConstructorOnStartup]
    public static class Pawn_GetGizmos_AwesomeInventory_Patch
    {
        static Pawn_GetGizmos_AwesomeInventory_Patch()
        {
            MethodInfo original = AccessTools.Method(typeof(Pawn), "GetGizmos");
            MethodInfo postfix = AccessTools.Method(typeof(Pawn_GetGizmos_AwesomeInventory_Patch), "Postfix");
            //Utility.Harmony.Patch(original, null, new HarmonyMethod(postfix));
        }

        /// <summary>
        /// Patch into pawn's gizmos, so to provide a Gear gizmo to open gear tab.
        /// </summary>
        /// <param name="gizmos"> Gizmos to add to. </param>
        /// <param name="__instance"> Pawn who owns the gizmos. </param>
        /// <returns> A collection of <see cref="Gizmo"/> that will display on screen. </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "Harmony Patch")]
        public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> gizmos, Pawn __instance)
        {
            if (gizmos == null)
                gizmos = new List<Gizmo>();

            foreach (Gizmo gizmo in gizmos)
            {
                yield return gizmo;
            }
        }
    }
}
