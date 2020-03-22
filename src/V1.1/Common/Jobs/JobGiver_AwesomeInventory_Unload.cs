// <copyright file="JobGiver_AwesomeInventory_Unload.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System.Linq;
using AwesomeInventory.Loadout;
using AwesomeInventory.UI;
using RimWorld;
using Verse;
using Verse.AI;

namespace AwesomeInventory.Jobs
{
    public class JobGiver_AwesomeInventory_Unload : ThinkNode_JobGiver
    {
        private Thing _thing;
        public static bool JobInProgress = false;

        // TODO need to add code for thing with count
        private JobGiver_AwesomeInventory_Unload(Thing t)
        {
            _thing = t;
        }

        protected override Job TryGiveJob(Pawn pawn)
        {
            if (JobInProgress)
            {
                Job fakeJob = new Job(AwesomeInventory_JobDefOf.AwesomeInventory_Fake)
                {
                    playerForced = true
                };
                return fakeJob;
            }

            Job job = HaulAIUtility.HaulToStorageJob(pawn, _thing);
            if (job == null)
            {
                Messages.Message("NoEmptyPlaceLower".Translate(), new TargetInfo(pawn.PositionHeld, pawn.MapHeld), MessageTypeDefOf.NeutralEvent);
                return null;
            }

            job.def = AwesomeInventory_JobDefOf.AwesomeInventory_Unload;

            return job;
        }

        public static Job TryGiveJobStatic(Pawn pawn, Thing thing)
        {
            ValidateArg.NotNull(pawn, nameof(pawn));
            ValidateArg.NotNull(thing, nameof(thing));

            if (JobInProgress)
            {
                Job fakeJob = new Job(AwesomeInventory_JobDefOf.AwesomeInventory_Fake, thing)
                {
                    playerForced = true,
                };
                return fakeJob;
            }

            Job job = HaulAIUtility.HaulToStorageJob(pawn, thing);
            if (job == null)
            {
                Messages.Message(UIText.NoEmptyPlaceLower.Translate(), new TargetInfo(pawn.PositionHeld, pawn.MapHeld), MessageTypeDefOf.NeutralEvent);
                thing.TryGetComp<CompRPGIUnload>().Unload = false;
                return null;
            }

            job.def = AwesomeInventory_JobDefOf.AwesomeInventory_Unload;
            job.playerForced = true;

            return job;
        }

        internal static void QueueJob(Pawn pawn, Job job)
        {
            if (job != null)
            {
                if (JobInProgress)
                {
                    pawn.jobs.jobQueue.EnqueueLast(job, JobTag.UnloadingOwnInventory);
                }
                else
                {
                    // TODO check constant thinknode before stop jobs
                    pawn.jobs.StopAll();
                    pawn.jobs.jobQueue.EnqueueFirst(job, JobTag.UnloadingOwnInventory);
                    JobInProgress = true;
                }
            }
        }
    }
}
