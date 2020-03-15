// <copyright file="JobGiver_RPGI_FindApparels.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the GPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RPG_Inventory_Remake.Loadout;
using RPG_Inventory_Remake_Common;
using Verse;
using Verse.AI;

namespace AwesomeInventory.Common.Loadout
{
    public class JobGiver_RPGI_FindApparels : JobGiver_FindItemByRadius<Apparel>
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            CompRPGILoadout RPGIloadout = ((ThingWithComps)pawn).TryGetComp<CompRPGILoadout>();
            Apparel targetA = null;

            if (RPGIloadout == null || !RPGIloadout.NeedRestock)
                return null;

            foreach (Thing item in RPGIloadout.ItemsToRestock)
            {
                if (item is Apparel apparel)
                {
                    ThingFilterAll filter = RPGIloadout.Loadout[apparel.MakeThingStuffPairWithQuality()];
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
                return null;
            else
            {
                return new Job(AwesomeInventory_JobDefOf.RPGI_ApparelOptions, targetA)
                {
                    // Check JobDriver_RPGI_ApparelOptions for details
                    count = 0
                };
            }
        }
    }
}
