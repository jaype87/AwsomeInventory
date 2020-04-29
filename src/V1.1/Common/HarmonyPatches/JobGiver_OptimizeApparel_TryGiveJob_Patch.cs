// <copyright file="JobGiver_OptimizeApparel_TryGiveJob_Patch.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AwesomeInventory.Loadout;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace AwesomeInventory.HarmonyPatches
{
    /// <summary>
    /// Patch is for the costume feature. Give up the job if the apparel to wear is not in costume.
    /// </summary>
    [StaticConstructorOnStartup]
    public static class JobGiver_OptimizeApparel_TryGiveJob_Patch
    {
        static JobGiver_OptimizeApparel_TryGiveJob_Patch()
        {
            MethodInfo original = AccessTools.Method(typeof(JobGiver_OptimizeApparel), "TryGiveJob");
            MethodInfo postfix = AccessTools.Method(typeof(JobGiver_OptimizeApparel_TryGiveJob_Patch), "Postfix");
            Utility.Harmony.Patch(original, postfix: new HarmonyMethod(postfix));
        }

        /// <summary>
        /// Assign null to the returned job if target thing is not in costume.
        /// </summary>
        /// <param name="pawn"> Pawn who is about to doing a job.</param>
        /// <param name="__result"> A scheduled job. </param>
        public static void Postfix(Pawn pawn, ref Job __result)
        {
            if (__result == null)
                return;

            CompAwesomeInventoryLoadout comp = pawn.TryGetComp<CompAwesomeInventoryLoadout>();
            if (comp == null)
                return;

            if (comp.Loadout is AwesomeInventoryCostume costume)
            {
                if (__result.def == JobDefOf.Wear)
                {
                    Job job = __result;
                    if (__result.targetA.HasThing && !costume.CostumeItems.Any(s => s.Allows(job.targetA.Thing, out _)))
                    {
                        __result = null;
                        JobMaker.ReturnToPool(job);
                        return;
                    }
                }
            }
        }
    }
}
