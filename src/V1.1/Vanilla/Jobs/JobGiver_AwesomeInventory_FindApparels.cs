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
            Apparel targetA = null;

            if (ailoadout == null || !ailoadout.NeedRestock)
                return null;

            foreach (Thing item in ailoadout.ItemsToRestock)
            {
                if (item is Apparel apparel)
                {
                    ThingFilterAll filter = ailoadout.Loadout[apparel.MakeThingStuffPairWithQuality()];
                    if (apparel.Stuff == RPGI_StuffDefOf.RPGIGenericResource)
                    {
                        targetA = FindItem(pawn, apparel.def, ThingRequestGroup.Undefined, (thing) => filter.Allows(thing, false));
                    }
                    else
                    {
                        targetA = FindItem(pawn, apparel.def, ThingRequestGroup.Undefined, (thing) => filter.Allows(thing));
                    }
                }
            }

            if (targetA == null)
            {
                return null;
            }
            else
            {
                return new DressJob(AwesomeInventory_JobDefOf.AwesomeInventory_Dress, targetA, false);
            }
        }
    }
}
