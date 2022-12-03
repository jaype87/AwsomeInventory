// <copyright file="JobGiver_OptimizeApparel_Supplement.cs" company="Zizhen Li">
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
    /// Have optimization look into inventory.
    /// </summary>
    /// <remarks> Vanilla optimization only looks for better apparels on the map. </remarks>
    public class JobGiver_OptimizeApparel_Supplement : ThinkNode
    {
        private const int _optmizedInterval = GenDate.TicksPerDay / GenDate.HoursPerDay * 2;

        private int _optimizedTick;

        /// <inheritdoc/>
        public override ThinkResult TryIssueJobPackage(Pawn pawn, JobIssueParams jobParams)
        {
            ValidateArg.NotNull(pawn, nameof(pawn));

            if (Find.TickManager.TicksGame < _optimizedTick)
                return ThinkResult.NoJob;

            CompAwesomeInventoryLoadout comp = pawn.TryGetComp<CompAwesomeInventoryLoadout>();
            if (comp?.Loadout == null || comp.Loadout.GetType() == typeof(AwesomeInventoryCostume))
                return ThinkResult.NoJob;

            List<Thing> list = pawn.inventory?.innerContainer?.ToList();
            if (list == null)
                return ThinkResult.NoJob;

            if (!list.Any())
            {
                _optimizedTick = _optmizedInterval + Find.TickManager.TicksGame;
                return ThinkResult.NoJob;
            }

            List<Apparel> wornApparels = pawn.apparel?.WornApparel;
            if (wornApparels == null)
                return ThinkResult.NoJob;

            AwesomeInventoryCostume costume = comp.Loadout as AwesomeInventoryCostume;

            var thingList = list
                .Where(t => t is Apparel apparel && ApparelOptionUtility.CanWear(pawn, apparel))
                .Select(t => new { thing = t, score = JobGiver_OptimizeApparel.ApparelScoreRaw(pawn, (Apparel)t) })
                .Where(thingScore => thingScore.score > 0.05f)
                .OrderByDescending(s => s.score)
                .ToList();

            if (!thingList.Any())
            {
                _optimizedTick = _optmizedInterval + Find.TickManager.TicksGame;
                return ThinkResult.NoJob;
            }

            List<Apparel> wornCostume = null;
            if (costume != null)
            {
                wornCostume = pawn.apparel.WornApparel
                    .AsParallel()
                    .Where(a => costume.CostumeItems.Any(c => c.Allows(a, out _)))
                    .ToList();
            }

            foreach (var t in thingList)
            {
                Thing thing = null;

                if (costume == null)
                {
                    thing = t.thing;
                }
                else
                {
                    foreach (ThingGroupSelector selector in costume.CostumeItems)
                    {
                        if (selector.Allows(t.thing, out _))
                        {
                            thing = t.thing;
                            break;
                        }
                    }

                    if (thing == null)
                    {
                        if (wornCostume.NullOrEmpty() || wornCostume.All(c => ApparelUtility.CanWearTogether(c.def, t.thing.def, BodyDefOf.Human)))
                            thing = t.thing;
                    }
                }

                if (thing != null)
                {
                    return new ThinkResult(
                        new DressJob(AwesomeInventory_JobDefOf.AwesomeInventory_Dress, thing, false)
                        , this
                        , JobTag.ChangingApparel);
                }
            }

            _optimizedTick = _optmizedInterval + Find.TickManager.TicksGame;
            return ThinkResult.NoJob;
        }
    }
}
