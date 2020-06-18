// <copyright file="ApparelOptionUtility.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AwesomeInventory.Jobs;
using RimWorld;
using Verse;
using Verse.AI;

namespace AwesomeInventory
{
    /// <summary>
    /// Provides support for apparel options displayed in menu.
    /// </summary>
    public static class ApparelOptionUtility
    {
        /// <summary>
        /// Check if a <paramref name="pawn"/> can wear <paramref name="apparel"/>.
        /// </summary>
        /// <param name="pawn"> Pawn that wants to wear <paramref name="apparel"/>. </param>
        /// <param name="apparel"> Apparel to wear. </param>
        /// <returns> Returns true if <paramref name="pawn"/> can wear <paramref name="apparel"/>. </returns>
        public static bool CanWear(Pawn pawn, Apparel apparel)
        {
            ValidateArg.NotNull(pawn, nameof(pawn));
            ValidateArg.NotNull(apparel, nameof(apparel));

            return !pawn.apparel.WouldReplaceLockedApparel(apparel)
                && (apparel.def.apparel.gender == Gender.None || apparel.def.apparel.gender == pawn.gender)
                && (!apparel.def.apparel.tags.Contains("Royal") || pawn.royalty.AllTitlesInEffectForReading.Count != 0)
                && ApparelUtility.HasPartsToWear(pawn, apparel.def);
        }

        /// <summary>
        /// Stop jobs that are either of <see cref="AwesomeInventory_JobDefOf.AwesomeInventory_Dress"/> or <see cref="AwesomeInventory_JobDefOf.AwesomeInventory_Undress"/>.
        /// </summary>
        /// <param name="pawn"> Pawn who has jobs. </param>
        public static void StopDressingJobs(Pawn pawn)
        {
            ValidateArg.NotNull(pawn, nameof(pawn));

            if (IsDressingJob(pawn.CurJobDef))
            {
                pawn.jobs.EndCurrentJob(JobCondition.InterruptForced, false);
            }

            List<QueuedJob> queuedJobs = new List<QueuedJob>(pawn.jobs?.jobQueue ?? Enumerable.Empty<QueuedJob>());
            foreach (QueuedJob queuedJob in queuedJobs)
            {
                if (IsDressingJob(queuedJob.job.def))
                {
                    pawn.jobs.EndCurrentOrQueuedJob(queuedJob.job, JobCondition.InterruptForced);
                }
            }

            bool IsDressingJob(JobDef jobDef)
            {
                return jobDef == AwesomeInventory_JobDefOf.AwesomeInventory_Undress || jobDef == AwesomeInventory_JobDefOf.AwesomeInventory_Dress;
            }
        }

        /// <summary>
        /// Check if pawn is in a capable state to change apparel.
        /// </summary>
        /// <param name="pawn"> Pawn to check. </param>
        /// <returns> If true, pawn is capbale of changing apparel. </returns>
        public static bool CapableOfWearing(Pawn pawn)
        {
            ValidateArg.NotNull(pawn, nameof(pawn));

            if (pawn.Downed || pawn.IsBurning() || pawn.InMentalState || !pawn.health.capacities.CanBeAwake || !pawn.health.capacities.CapableOf(PawnCapacityDefOf.Moving) || !pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
            {
                return false;
            }

            return true;
        }
    }
}
