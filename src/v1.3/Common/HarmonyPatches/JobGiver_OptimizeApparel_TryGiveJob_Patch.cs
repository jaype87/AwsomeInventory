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
using Verse.Noise;

namespace AwesomeInventory.HarmonyPatches
{
    /// <summary>
    /// Patch is for the costume feature. Give up the job if the apparel to wear is not in costume.
    /// </summary>
    [StaticConstructorOnStartup]
    public static class JobGiver_OptimizeApparel_TryGiveJob_Patch
    {
        private static MethodInfo _setNextOptimizeTick = typeof(JobGiver_OptimizeApparel).GetMethod("SetNextOptimizeTick", BindingFlags.NonPublic | BindingFlags.Instance);

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
        /// <param name="__instance"> Instance of type which this postfix patches. </param>
        [SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "Harmony patch")]
        [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Required to catch all")]
        public static void Postfix(Pawn pawn, ref Job __result, JobGiver_OptimizeApparel __instance)
        {
            if (__result == null || pawn == null || __instance == null)
                return;

            CompAwesomeInventoryLoadout comp = pawn.TryGetComp<CompAwesomeInventoryLoadout>();
            if (comp == null)
                return;

            Job job = __result;
            Thing targetThingA = job.targetA.Thing;
            switch (comp.Loadout)
            {
                case AwesomeInventoryCostume costume when __result.def != JobDefOf.Wear:
                    return;

                case AwesomeInventoryCostume costume when targetThingA == null || costume.CostumeItems.Any(s => s.Allows(targetThingA, out _)) || costume.CostumeItems.All(s => ApparelUtility.CanWearTogether(targetThingA.def, s.AllowedThing, BodyDefOf.Human)):
                    return;

                case AwesomeInventoryCostume costume:
                    __result = null;
                    JobMaker.ReturnToPool(job);
                    _setNextOptimizeTick.Invoke(__instance, new[] { pawn });
                    return;

                case AwesomeInventoryLoadout loadout:
                {
                    var source = new CancellationTokenSource();
                    var token  = source.Token;
                    try
                    {
                        var conflict = false;

                        if (!loadout.Any(selector => selector.Allows(targetThingA, out _)))
                        {
                            Parallel.ForEach(
                                Partitioner.Create(pawn.apparel.WornApparel)
                                , (Apparel apparel) =>
                                {
                                    if (token.IsCancellationRequested || targetThingA == null || ApparelUtility.CanWearTogether(apparel.def, targetThingA.def, BodyDefOf.Human))
                                        return;

                                    if (!comp.Loadout.Any(selector => selector.Allows(apparel, out _)))
                                        return;
                                    
                                    conflict = true;
                                    source.Cancel();
                                });
                        }

                        if (!conflict)
                            return;
                    
                        __result = new DressJob(AwesomeInventory_JobDefOf.AwesomeInventory_Dress, targetThingA, false);
                        JobMaker.ReturnToPool(job);
                    }
                    catch (Exception e)
                    {
                        Log.ErrorOnce(e.Message, 129555056);
                        __result = JobMaker.MakeJob(JobDefOf.Wear, targetThingA);
                    }
                    finally
                    {
                        source.Dispose();
                    }

                    break;
                }
            }
        }
    }
}
