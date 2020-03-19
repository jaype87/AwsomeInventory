// <copyright file="JobDriver_Cleanup_RPGI_Patch.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the GPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AwesomeInventory.Jobs;
using AwesomeInventory.Loadout;
using AwesomeInventory.Utilities;
using HarmonyLib;
using Verse;
using Verse.AI;

namespace AwesomeInventory.Common.HarmonyPatches
{
    [StaticConstructorOnStartup]
    public class JobDriver_Cleanup_RPGI_Patch
    {
        static JobDriver_Cleanup_RPGI_Patch()
        {
            MethodInfo original = AccessTools.Method(typeof(JobDriver), "Cleanup");
            MethodInfo postfix = AccessTools.Method(typeof(JobDriver_Cleanup_RPGI_Patch), "Postfix");
            Utility.Harmony.Patch(original, null, new HarmonyMethod(postfix));
        }

        public static void Postfix(JobCondition condition, JobDriver __instance)
        {
            // Change item's compProperties unload to false
            CompRPGIUnload comp = __instance.job.targetA.Thing?.TryGetComp<CompRPGIUnload>();
            if (comp != null)
            {
                comp.Unload = false;
            }
            if (__instance.job.def == AwesomeInventory_JobDefOf.AwesomeInventory_Unload)
            {
                JobGiver_AwesomeInventory_Unload.JobInProgress = false;
                Pawn pawn = __instance.pawn;
                if (condition == JobCondition.Succeeded || condition == JobCondition.Incompletable)
                {
                    foreach (QueuedJob qj in pawn.jobs.jobQueue.Where(j => j.job.def == AwesomeInventory_JobDefOf.AwesomeInventory_Fake).ToList())
                    {
                        pawn.jobs.jobQueue.Extract(qj.job);
                        if (qj.job.targetA.Thing != null)
                        {
                            Job newJob = JobGiver_AwesomeInventory_Unload.TryGiveJobStatic(pawn, qj.job.targetA.Thing);
                            if (newJob == null)
                            {
                                qj.job.targetA.Thing.TryGetComp<CompRPGIUnload>().Unload = false;
                                continue;
                            }
                            pawn.jobs.jobQueue.EnqueueFirst(newJob, JobTag.UnloadingOwnInventory);
                            return;
                        }
                    }
                    return;
                }
                else
                {
                    foreach (QueuedJob qj in pawn.jobs.jobQueue.Where(j => j.job.def == AwesomeInventory_JobDefOf.AwesomeInventory_Fake).ToList())
                    {
                        CompRPGIUnload comp1 = qj.job.targetA.Thing?.TryGetComp<CompRPGIUnload>();
                        if (comp1 != null)
                        {
                            comp1.Unload = false;
                        }
                        pawn.jobs.jobQueue.Extract(qj.job);
                    }
                }
            }
        }
    }
}
