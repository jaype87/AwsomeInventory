// <copyright file="JobGiver_OptimizeApparel_TryGiveJob_Patch.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AwesomeInventory.Jobs;
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
        [SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "Harmony patch")]
        [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Required to catch all")]
        public static void Postfix(Pawn pawn, ref Job __result)
        {
            if (__result == null)
                return;

            CompAwesomeInventoryLoadout comp = pawn.TryGetComp<CompAwesomeInventoryLoadout>();
            if (comp == null)
                return;

            Job job = __result;
            Thing targetThingA = job.targetA.Thing;
            if (comp.Loadout is AwesomeInventoryCostume costume)
            {
                if (__result.def == JobDefOf.Wear)
                {
                    if (targetThingA != null
                        && !costume.CostumeItems.Any(s => s.Allows(targetThingA, out _))
                        && !costume.CostumeItems.All(s => ApparelUtility.CanWearTogether(targetThingA.def, s.AllowedThing, BodyDefOf.Human)))
                    {
                        __result = null;
                        JobMaker.ReturnToPool(job);
                        return;
                    }
                }
            }
            else if (comp.Loadout is AwesomeInventoryLoadout loadout)
            {
                CancellationTokenSource source = new CancellationTokenSource();
                CancellationToken token = source.Token;
                try
                {
                    bool conflict = false;

                    Parallel.ForEach(
                        Partitioner.Create(pawn.apparel.WornApparel)
                        , (Apparel apparel) =>
                        {
                            if (!token.IsCancellationRequested
                                && targetThingA != null
                                && apparel.def != targetThingA.def
                                && !ApparelUtility.CanWearTogether(apparel.def, targetThingA.def, BodyDefOf.Human))
                            {
                                if (comp.Loadout.Any(selector => selector.Allows(apparel, out _)))
                                {
                                    conflict = true;
                                    source.Cancel();
                                }
                            }
                        });

                    if (conflict)
                    {
                        __result = new DressJob(AwesomeInventory_JobDefOf.AwesomeInventory_Dress, targetThingA, false);
                        JobMaker.ReturnToPool(job);
                    }
                }
                catch (Exception e)
                {
                    __result = JobMaker.MakeJob(JobDefOf.Wear, targetThingA);
                }
                finally
                {
                    source.Dispose();
                }
            }
        }
    }
}
