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
    /// <remarks> Vanilla optimizaiton only looks for better apparels on the map. </remarks>
    public class JobGiver_OptimizeApparel_Supplement : ThinkNode
    {
        private int _optimizedTick = Find.TickManager.TicksGame;
        private int _optmizedInterval = 6000;

        /// <inheritdoc/>
        public override ThinkResult TryIssueJobPackage(Pawn pawn, JobIssueParams jobParams)
        {
            ValidateArg.NotNull(pawn, nameof(pawn));

            if (Find.TickManager.TicksGame < _optimizedTick)
                return ThinkResult.NoJob;

            CompAwesomeInventoryLoadout comp = pawn.TryGetComp<CompAwesomeInventoryLoadout>();
            if (comp == null || !(comp.Loadout is AwesomeInventoryLoadout))
                return ThinkResult.NoJob;

            List<Thing> list = pawn.inventory?.innerContainer?.ToList();
            if (list == null)
                return ThinkResult.NoJob;

            if (!list.Any())
            {
                _optimizedTick += _optmizedInterval;
                return ThinkResult.NoJob;
            }

            float bestScore = 0f;
            Thing thing = null;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] is Apparel apparel && ApparelOptionUtility.CanWear(pawn, apparel))
                {
                    if (comp.Loadout is AwesomeInventoryCostume costume)
                    {
                        if (!costume.CostumeItems.Any(c => c.Allows(apparel, out _)))
                            continue;
                    }

                    float score = JobGiver_OptimizeApparel.ApparelScoreGain(pawn, apparel);
                    if (!(score < 0.05f) && !(score < bestScore))
                    {
                        thing = apparel;
                        bestScore = score;
                    }
                }
            }

            if (thing == null)
            {
                _optimizedTick += _optmizedInterval;
                return ThinkResult.NoJob;
            }
            else
            {
                return new ThinkResult(new DressJob(AwesomeInventory_JobDefOf.AwesomeInventory_Dress, thing, false), this, JobTag.ChangingApparel);
            }
        }
    }
}
