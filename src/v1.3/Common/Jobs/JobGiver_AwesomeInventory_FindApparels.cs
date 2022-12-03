// <copyright file="JobGiver_AwesomeInventory_FindApparels.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the GPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Linq;
using AwesomeInventory.Loadout;
using RimWorld;
using Verse;
using Verse.AI;

namespace AwesomeInventory.Jobs
{
    /// <summary>
    /// Gives out a job if a proper apparel is found on the map.
    /// </summary>
    public class JobGiver_AwesomeInventory_FindApparels : ThinkNode_JobGiver
    {
        private JobGiver_FindItemByRadius _parent;

        /// <summary>
        /// Gives out a job if a proper apparel is found on the map.
        /// </summary>
        /// <param name="pawn"> The pawn in question. </param>
        /// <returns> A 9 to 5 job. </returns>
        protected override Job TryGiveJob(Pawn pawn)
        {
            ValidateArg.NotNull(pawn, nameof(pawn));
#if DEBUG
            Log.Message(pawn.Name + " Looking for apparels");
#endif

            CompAwesomeInventoryLoadout ailoadout = ((ThingWithComps)pawn).TryGetComp<CompAwesomeInventoryLoadout>();

            if (ailoadout == null || !ailoadout.NeedRestock)
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

            foreach (ThingGroupSelector groupSelector in ailoadout.ItemsToRestock.Select(p => p.Key))
            {
                if (groupSelector.AllowedThing.IsApparel)
                {
                    Thing targetA =
                        _parent.FindItem(
                            pawn
                            , pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.Apparel)
                            , (thing) => ailoadout.Loadout.filter.Allows(thing)
                                         && groupSelector.Allows(thing, out _)
                                         && !ailoadout.Loadout.IncludedInBlacklist(thing)
                                         && (thing.def.apparel.gender == Gender.None || thing.def.apparel.gender == pawn.gender)
                                         && (!thing.def.apparel.tags.Contains("Royal") || pawn.royalty.AllTitlesInEffectForReading.Count != 0));

                    if (targetA != null)
                    {
                        if (pawn.outfits?.CurrentOutfit is AwesomeInventoryCostume costume)
                        {
                            if (costume.CostumeItems.Any(c => c.Allows(targetA, out _)))
                            {
                                return new DressJob(AwesomeInventory_JobDefOf.AwesomeInventory_Dress, targetA, false);
                            }
                        }
                        else if (!pawn.IsOldApparelForced(targetA as Apparel))
                        {
                            return new DressJob(AwesomeInventory_JobDefOf.AwesomeInventory_Dress, targetA, false);
                        }

                        Job job = JobMaker.MakeJob(JobDefOf.TakeInventory, targetA);
                        job.count = targetA.stackCount;
                        return job;
                    }
                }
            }

            return null;
        }
    }
}
