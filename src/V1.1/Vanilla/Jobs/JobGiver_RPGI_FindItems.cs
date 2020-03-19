// <copyright file="JobGiver_RPGI_FindItems.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the GPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AwesomeInventory.Loadout;
using RimWorld;
using Verse;
using Verse.AI;

namespace AwesomeInventory.Jobs
{
    public class JobGiver_RPGI_FindItems : JobGiver_FindItemByRadius<Thing>
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            CompAwesomeInventoryLoadout RPGIloadout = ((ThingWithComps)pawn).TryGetComp<CompAwesomeInventoryLoadout>();
            Thing targetA = null;

            if (RPGIloadout == null || !RPGIloadout.NeedRestock)
                return null;

            foreach (Thing item in RPGIloadout.ItemsToRestock)
            {
                if (!item.def.IsApparel || !item.def.IsWeapon)
                {
                    ThingFilterAll filter = RPGIloadout.Loadout[item.MakeThingStuffPairWithQuality()];
                    
                    if (item.def is LoadoutGenericDef genericDef)
                    {
                        targetA = FindItem(pawn, null, genericDef.thingRequestGroup, (Thing thing) => genericDef.Validator(thing.def));
                    }
                    else if (targetA.Stuff == RPGI_StuffDefOf.RPGIGenericResource)
                    {
                        targetA = FindItem(pawn, item.def, ThingRequestGroup.Undefined, (thing) => filter.Allows(thing, false));
                    }
                    else
                    {
                        targetA = FindItem(pawn, item.def, ThingRequestGroup.Undefined, (thing) => filter.Allows(thing));
                    }
                }
            }
            if (targetA == null)
            {
                return null;
            }
            else
            {
                return new Job(JobDefOf.TakeInventory, targetA)
                {
                    // Check JobDriver_RPGI_ApparelOptions for details
                    count = 0
                };
            }
        }
    }
}
