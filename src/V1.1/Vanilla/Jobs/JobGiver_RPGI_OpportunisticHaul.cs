// <copyright file="JobGiver_RPGI_OpportunisticHaul.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the GPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using AwesomeInventory.Jobs;
using RimWorld;
using Verse;
using Verse.AI;

namespace AwesomeInventory.Jobs
{
    public class JobGiver_RPGI_OpportunisticHaul : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            ValidateArg.NotNull(pawn, nameof(pawn));

            List<WorkGiverDef> workGivers = WorkTypeDefOf.Hauling.workGiversByPriority;

            if (workGivers[0].Worker is WorkGiver_Scanner scanner)
            {
                IEnumerable<Thing> enumerable = scanner.PotentialWorkThingsGlobal(pawn);
                if (enumerable == null || enumerable.Any())
                {
                    return null;
                }

                Thing thing = GenClosest.ClosestThing_Global_Reachable(
                      pawn.Position
                    , pawn.Map
                    , enumerable
                    , scanner.PathEndMode
                    , TraverseParms.For(pawn, scanner.MaxPathDanger(pawn))
                    , JobGiver_FindItemByRadius.Radius[0]
                    , t => !t.IsForbidden(pawn) && scanner.HasJobOnThing(pawn, t));
                if (thing != null)
                {
                    return scanner.JobOnThing(pawn, thing);
                }
            }
            return null;
        }
    }
}
