// <copyright file="JobGiver_AwesomeInventory_FindApparels.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the GPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using AwesomeInventory.Jobs;
using RimWorld;
using Verse;
using Verse.AI;

namespace AwesomeInventory.Loadout
{
    /// <summary>
    /// Gives out a job if a proper apparel is found on the map.
    /// </summary>
    public class JobGiver_AwesomeInventory_FindApparels : JobGiver_FindItemByRadius<Apparel>
    {
        public JobGiver_AwesomeInventory_FindApparels()
        {
        }

        /// <summary>
        /// Gives out a job if a proper apparel is found on the map.
        /// </summary>
        /// <param name="pawn"> The pawn in question. </param>
        /// <returns> A 9 to 5 job. </returns>
        protected override Job TryGiveJob(Pawn pawn)
        {
            CompAwesomeInventoryLoadout ailoadout = ((ThingWithComps)pawn).TryGetComp<CompAwesomeInventoryLoadout>();

            if (ailoadout == null || !ailoadout.NeedRestock)
                return null;

            foreach (ThingGroupSelector groupSelector in ailoadout.ItemsToRestock)
            {
                if (groupSelector.AllowedThing.IsApparel)
                {
                    Apparel targetA = this.FindItem(pawn, groupSelector.AllowedThing, new[] { ThingRequestGroup.Apparel }, (thing) => groupSelector.Allows(thing));
                    if (targetA != null)
                    {
                        return new DressJob(AwesomeInventory_JobDefOf.AwesomeInventory_Dress, targetA, false);
                    }
                }
            }

            return null;
        }
    }
}
