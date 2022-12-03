// <copyright file="JobGiver_AwesomeInventory_FindItems.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
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
    /// <summary>
    /// Find items that fits in <see cref="AwesomeInventoryLoadout"/>.
    /// </summary>
    public class JobGiver_AwesomeInventory_FindItems : ThinkNode_JobGiver
    {
        private JobGiver_FindItemByRadius _parent;

        /// <summary>
        /// Try to give a job to <paramref name="pawn"/> for items that needs to restock.
        /// </summary>
        /// <param name="pawn"> Pawn for the job. </param>
        /// <returns> A job to stock up items in <see cref="AwesomeInventoryLoadout"/>. </returns>
        protected override Job TryGiveJob(Pawn pawn)
        {
            ValidateArg.NotNull(pawn, nameof(pawn));

            CompAwesomeInventoryLoadout aiLoadout = ((ThingWithComps)pawn).TryGetComp<CompAwesomeInventoryLoadout>();

            if (aiLoadout == null || !aiLoadout.NeedRestock)
                return null;

            if (_parent == null)
            {
                if (parent is JobGiver_FindItemByRadius p)
                {
                    _parent = p;
                }
                else
                {
                    throw new InvalidOperationException(ErrorText.WrongTypeParentThinkNode);
                }
            }

            Thing targetA = null;
            ThingGroupSelector groupSelector;
            int stackcount = 0;
            foreach (KeyValuePair<ThingGroupSelector, int> pair in aiLoadout.ItemsToRestock)
            {
                groupSelector = pair.Key;
                stackcount = pair.Value;
                if (!groupSelector.AllowedThing.IsApparel && (!groupSelector.AllowedThing.IsWeapon || groupSelector.AllowedThing.IsDrug))
                {
                    if (groupSelector.AllowedThing is AIGenericDef genericDef)
                    {
                        List<List<Thing>> thingLists = new List<List<Thing>>();
                        foreach (ThingDef thingDef in genericDef.AvailableDefs)
                        {
                            List<Thing> things = pawn.Map.listerThings.ThingsOfDef(thingDef);
                            if (things.Any())
                                thingLists.Add(things);
                        }

                        Thing foundThing = null;
                        foreach (List<Thing> thingList in thingLists)
                        {
                            foundThing =
                                _parent.FindItem(
                                    pawn
                                    , thingList
                                    , (thing) =>
                                    {
                                        Thing innerThing = thing.GetInnerIfMinified();
                                        return groupSelector.Allows(innerThing, out _)
                                               &&
                                               !aiLoadout.Loadout.IncludedInBlacklist(innerThing);
                                    });

                            if (foundThing != null)
                                break;
                        }

                        targetA = foundThing;
                    }
                    else
                    {
                        if (groupSelector.AllowedThing.Minifiable)
                        {
                            // There is a bug if add minifiedThings to searchSet by searchSet.AddRange() or searchSet.Add()
                            IEnumerable<Thing> minifiedThings = pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.MinifiedThing)
                                .Where(t => (t as MinifiedThing).InnerThing.def == groupSelector.AllowedThing);

                            targetA =
                            _parent.FindItem(
                                pawn
                                , minifiedThings
                                , (thing) =>
                                {
                                    Thing innerThing = thing.GetInnerIfMinified();
                                    return groupSelector.Allows(innerThing, out _)
                                           &&
                                           !aiLoadout.Loadout.IncludedInBlacklist(innerThing);
                                });
                        }

                        if (targetA == null)
                        {
                            targetA =
                            _parent.FindItem(
                                pawn
                                , pawn.Map.listerThings.ThingsOfDef(groupSelector.AllowedThing)
                                , (thing) =>
                                {
                                    Thing innerThing = thing.GetInnerIfMinified();
                                    return groupSelector.Allows(innerThing, out _)
                                           &&
                                           !aiLoadout.Loadout.IncludedInBlacklist(innerThing);
                                });
                        }
                    }

                    if (targetA != null)
                        break;
                }
            }

            if (targetA == null)
            {
                return null;
            }
            else
            {
                Job job = JobMaker.MakeJob(AwesomeInventory_JobDefOf.AwesomeInventory_TakeInventory, targetA);
                job.count = Math.Min(targetA.stackCount, stackcount * -1);
                return job;
            }
        }
    }
}
