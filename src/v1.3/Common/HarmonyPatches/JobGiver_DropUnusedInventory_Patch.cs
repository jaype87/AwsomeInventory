// <copyright file="JobGiver_DropUnusedInventory_Patch.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using AwesomeInventory.Loadout;
using HarmonyLib;
using RimWorld;
using Verse;

namespace AwesomeInventory.HarmonyPatches
{
    /// <summary>
    /// Prevent pawns from droping items that are in loadout.
    /// </summary>
    [StaticConstructorOnStartup]
    public static class JobGiver_DropUnusedInventory_Patch
    {
        static JobGiver_DropUnusedInventory_Patch()
        {
            MethodInfo original = typeof(JobGiver_DropUnusedInventory).GetMethod("Drop", BindingFlags.NonPublic | BindingFlags.Instance);
            MethodInfo prefix = typeof(JobGiver_DropUnusedInventory_Patch).GetMethod("Prefix", BindingFlags.Public | BindingFlags.Static);
            Utility.Harmony.Patch(original, new HarmonyMethod(prefix));
        }

        /// <summary>
        /// Prevent pawns from droping items that are in loadout.
        /// </summary>
        /// <param name="pawn"> Selected pawn. </param>
        /// <param name="thing"> Thing in question. </param>
        /// <returns> Returns true if continues to execute the original method. </returns>
        public static bool Prefix(Pawn pawn, Thing thing)
        {
            if (pawn.UseLoadout(out CompAwesomeInventoryLoadout comp))
            {
                if (comp.Loadout.Any(t => t.Allows(thing, out _)))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
