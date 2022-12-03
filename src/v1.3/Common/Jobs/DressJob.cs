// <copyright file="DressJob.cs" company="Zizhen Li">
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
    /// A job describes wearing apparels.
    /// </summary>
    public class DressJob : Job
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DressJob"/> class.
        /// </summary>
        public DressJob()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DressJob"/> class.
        /// </summary>
        /// <param name="jobDef"> Definitino of the job. </param>
        /// <param name="localTargetInfo"> Information on the job target. </param>
        /// <param name="force"> If true, set apparel forced to wear. </param>
        public DressJob(JobDef jobDef, LocalTargetInfo localTargetInfo, bool force)
            : base(jobDef, localTargetInfo)
        {
            this.ForceWear = force;
        }

        /// <summary>
        /// Gets or sets a value indicating whether set apparel forced to wear.
        /// </summary>
        public bool ForceWear { get; set; }
    }
}
