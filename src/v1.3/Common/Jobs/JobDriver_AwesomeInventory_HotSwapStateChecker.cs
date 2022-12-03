// <copyright file="JobDriver_AwesomeInventory_HotSwapStateChecker.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AwesomeInventory.Loadout;
using Verse;
using Verse.AI;

namespace AwesomeInventory.Jobs
{
    /// <summary>
    /// It is used as a tag for <see cref="AwesomeInventory.HarmonyPatches.Pawn_JobTracker_CleanQueuedJobs"/> to check.
    /// </summary>
    /// <remarks> As of 4/29/2020, the job system in RimWorld doesn't allow queued job to check JobCondition when interrupted. </remarks>
    public class JobDriver_AwesomeInventory_HotSwapStateChecker : JobDriver
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JobDriver_AwesomeInventory_HotSwapStateChecker"/> class.
        /// </summary>
        public JobDriver_AwesomeInventory_HotSwapStateChecker()
        {
        }

        /// <summary>
        /// Make reservation on apparel to undress.
        /// </summary>
        /// <param name="errorOnFailed"> If true, logs error when fails. </param>
        /// <returns> Returns true, if all reservations are made. </returns>
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        /// <summary>
        /// Make instruction on what to do.
        /// </summary>
        /// <returns> A list of instruction. </returns>
        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return new Toil()
            {
                initAction = () =>
                {
                    return;
                },
            };
        }
    }
}
