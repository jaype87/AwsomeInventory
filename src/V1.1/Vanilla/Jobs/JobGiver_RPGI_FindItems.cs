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
    public class JobGiver_RPGI_FindItems : JobGiver_FindItemByRadius
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            CompAwesomeInventoryLoadout aiLoadout = ((ThingWithComps)pawn).TryGetComp<CompAwesomeInventoryLoadout>();
            Thing targetA = null;

            if (aiLoadout == null || !aiLoadout.NeedRestock)
                return null;

            foreach (ThingGroupSelector groupSelector in aiLoadout.ItemsToRestock)
            {
                if (!groupSelector.AllowedThing.IsApparel && !groupSelector.AllowedThing.IsWeapon)
                {
                    if (groupSelector.AllowedThing is AIGenericDef genericDef)
                    {
                        targetA = FindItem(
                            pawn,
                            null,
                            ThingListGroupHelper.AllGroups.Where(
                                g => genericDef.ThingCategoryDefs.SelectMany(c => c.DescendantThingDefs).Any(t => g.Includes(t))),
                            (Thing thing) => genericDef.Includes(thing.def));
                    }
                    else
                    {
                        targetA = FindItem(pawn, groupSelector.AllowedThing, new[] { ThingRequestGroup.Undefined }, (thing) => groupSelector.Allows(thing, out _));
                    }
                }
            }

            if (targetA == null)
            {
                return null;
            }
            else
            {
                return JobMaker.MakeJob(JobDefOf.TakeInventory, targetA);
            }
        }
    }
}
