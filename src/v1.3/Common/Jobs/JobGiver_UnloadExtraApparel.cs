// <copyright file="JobGiver_UnloadExtraApparel.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AwesomeInventory.Loadout;
using RimWorld;
using Verse;
using Verse.AI;

namespace AwesomeInventory.Jobs
{
    /// <summary>
    /// Unload extra apparel in pawn's inventory.
    /// </summary>
    public class JobGiver_UnloadExtraApparel : ThinkNode
    {
        /// <inheritdoc/>
        public override ThinkResult TryIssueJobPackage(Pawn pawn, JobIssueParams jobParams)
        {
            if (pawn?.inventory?.innerContainer == null)
                return ThinkResult.NoJob;

            if (!pawn.UseLoadout(out CompAwesomeInventoryLoadout comp))
                return ThinkResult.NoJob;

            foreach (Thing thing in pawn.inventory.innerContainer)
            {
                if (thing is Apparel apparel)
                {
                    bool extra = true;
                    CompAwesomeInventoryLoadout.ThingGroupSelectorPool pool = comp.FindPotentialThingGroupSelectors(thing, comp.Loadout);
                    if (pool.OrderedSelectorTuples.Any())
                    {
                        foreach (var tuple in pool.OrderedSelectorTuples)
                        {
                            if (!(comp.InventoryMargins[tuple.Item2] > 0))
                            {
                                extra = false;
                                break;
                            }
                        }
                    }

                    if (extra)
                    {
                        if (StoreUtility.TryFindBestBetterStorageFor(apparel, pawn, pawn.Map, StoreUtility.CurrentStoragePriorityOf(apparel), pawn.Faction, out _, out _))
                        {
                            UnloadApparelJob job = new UnloadApparelJob(thing);
                            return new ThinkResult(job, this, JobTag.UnloadingOwnInventory);
                        }
                    }
                }
            }

            return ThinkResult.NoJob;
        }
    }
}
