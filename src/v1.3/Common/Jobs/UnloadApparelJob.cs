// <copyright file="UnloadApparelJob.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace AwesomeInventory.Jobs
{
    /// <summary>
    /// A job to unload extra apparels on pawn.
    /// </summary>
    public class UnloadApparelJob : Job
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnloadApparelJob"/> class.
        /// </summary>
        public UnloadApparelJob()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnloadApparelJob"/> class.
        /// </summary>
        /// <param name="localTargetInfo"> Apparel to unload. </param>
        public UnloadApparelJob(LocalTargetInfo localTargetInfo)
            : base(AwesomeInventory_JobDefOf.AwesomeInventory_Unload, localTargetInfo)
        {
        }
    }
}
