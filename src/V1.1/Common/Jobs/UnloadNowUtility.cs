// <copyright file="UnloadNowUtility.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using AwesomeInventory.Loadout;
using AwesomeInventory.UI;
using RimWorld;
using Verse;
using Verse.AI;

namespace AwesomeInventory.Jobs
{
    /// <summary>
    /// Handles the Unload Now functionality.
    /// </summary>
    public static class UnloadNowUtility
    {
        /// <summary>
        /// Check if <paramref name="thing"/> is already on an unload job assigned to <paramref name="pawn"/>.
        /// </summary>
        /// <param name="pawn"> Pawn that has unload jobs. </param>
        /// <param name="thing"> Thing to check. </param>
        /// <returns> Returns true if <paramref name="thing"/> is assigend to an unload job. </returns>
        public static bool ThingInQueue(Pawn pawn, Thing thing)
        {
            ValidateArg.NotNull(pawn, nameof(pawn));

            if (pawn.jobs == null)
                return false;

            return pawn.jobs.jobQueue.Any(j => j.job.targetA.Thing == thing && j.job.def == AwesomeInventory_JobDefOf.AwesomeInventory_Unload)
                || (pawn.CurJob?.targetA.Thing == thing && pawn.CurJob.def == AwesomeInventory_JobDefOf.AwesomeInventory_Unload);
        }

        /// <summary>
        /// Stop an unload job.
        /// </summary>
        /// <param name="pawn"> Looks for unload jobs on this pawn. </param>
        /// <param name="thing"> Thing that is assigned to an unload job. </param>
        /// <remarks> Safe to call if thing is null or thing is not assigned an unload job. </remarks>
        public static void StopJob(Pawn pawn, Thing thing)
        {
            ValidateArg.NotNull(pawn, nameof(pawn));

            if (pawn.CurJob.def == AwesomeInventory_JobDefOf.AwesomeInventory_Unload && pawn.CurJob.targetA.Thing == thing)
            {
                pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
            }
            else
            {
                Job job = pawn.jobs.jobQueue.FirstOrDefault(
                    j => j.job.def == AwesomeInventory_JobDefOf.AwesomeInventory_Unload
                      && j.job.targetA == thing).job;

                if (job != null)
                    pawn.jobs.EndCurrentOrQueuedJob(job, JobCondition.InterruptForced);
            }
        }

        /// <summary>
        /// Queue an unload job on <paramref name="pawn"/> for <paramref name="thing"/>.
        /// </summary>
        /// <param name="pawn"> Pawn the unload job assigns to. </param>
        /// <param name="thing"> Thing to be queued for an unload job. </param>
        internal static void QueueJob(Pawn pawn, Thing thing)
        {
            if (TryIssueJob(pawn, thing, out Job job))
            {
                if (pawn.CurJobDef != job.def)
                {
                    // Set fromQueue to true to avoid a Log.Warning in Pawn_JobTracker when curDriver.TryMakePreToilReservations(!flag) returns false.
                    pawn.jobs.StartJob(job, JobCondition.InterruptForced, tag: JobTag.UnloadingOwnInventory, fromQueue: true, canReturnCurJobToPool: true);
                }
                else
                {
                    pawn.jobs.jobQueue.EnqueueFirst(job, JobTag.UnloadingOwnInventory);
                }
            }
        }

        private static bool TryIssueJob(Pawn pawn, Thing thing, out Job job)
        {
            // Check if pawn can do an unload job.
            if (pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
            {
                job = JobMaker.MakeJob(AwesomeInventory_JobDefOf.AwesomeInventory_Unload, thing);
                job.ignoreJoyTimeAssignment = true;
                job.playerForced = true;
                return true;
            }
            else
            {
                Messages.Message(UIText.CantUnloadIncapableManipulation.TranslateSimple(), new TargetInfo(pawn.PositionHeld, pawn.MapHeld), MessageTypeDefOf.NeutralEvent);
                job = null;
                return false;
            }
        }
    }
}
