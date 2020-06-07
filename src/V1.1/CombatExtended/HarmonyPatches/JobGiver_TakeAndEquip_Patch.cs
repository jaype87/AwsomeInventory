// <copyright file="JobGiver_TakeAndEquip_Patch.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CombatExtended;
using HarmonyLib;
using Verse;

namespace AwesomeInventory.HarmonyPatches
{
    /// <summary>
    /// Skip this job when pawn has a loadout.
    /// </summary>
    [StaticConstructorOnStartup]
    public static class JobGiver_TakeAndEquip_Patch
    {
        static JobGiver_TakeAndEquip_Patch()
        {
            MethodInfo original = typeof(JobGiver_TakeAndEquip).GetMethod("GetPriorityWork", BindingFlags.Instance | BindingFlags.NonPublic);
            MethodInfo prefix = typeof(JobGiver_TakeAndEquip_Patch).GetMethod("Prefix", BindingFlags.Static | BindingFlags.Public);
            Utility.Harmony.Patch(original, prefix: new HarmonyMethod(prefix));
        }

        /// <summary>
        /// If a pawn has a loadout, return priority 0.
        /// </summary>
        /// <param name="pawn"> Selected pawn. </param>
        /// <param name="__result"> Result returned by the original method. </param>
        /// <returns> If true, continue to execute. </returns>
        public static bool Prefix(Pawn pawn, ref object __result)
        {
            if (pawn.UseLoadout(out _))
            {
                __result = 0;
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
