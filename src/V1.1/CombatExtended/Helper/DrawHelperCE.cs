// <copyright file="DrawHelperCE.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CombatExtended;
using RimWorld;
using Verse;

namespace AwesomeInventory.UI
{
    /// <summary>
    /// A helper class for drawing in game.
    /// It only consists of member methods while <see cref="DrawUtility"/> only has static methods.
    /// </summary>
    [RegisterService(typeof(DrawHelper), typeof(DrawHelperCE))]
    public class DrawHelperCE : DrawHelper
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DrawHelperCE"/> class.
        /// </summary>
        [Obsolete(ErrorText.NoDirectCall, false)]
        public DrawHelperCE()
        {
        }

        /// <summary>
        /// Build a tooltip string for gears worn by <paramref name="pawn"/>.
        /// </summary>
        /// <param name="pawn"> Pawn that the tooltip is for. </param>
        /// <returns> A tooltip on bulk for gears worn by <paramref name="pawn"/>. </returns>
        public virtual string BulkTextFor(Pawn pawn)
        {
            ValidateArg.NotNull(pawn, nameof(pawn));

            string text = string.Empty;

            GetStatTooltipForThings(pawn.equipment?.AllEquipmentListForReading ?? Enumerable.Empty<Thing>(), CE_StatDefOf.Bulk, (value) => value.ToString("F2"), ref text);
            GetStatTooltipForThings(pawn.apparel?.WornApparel ?? Enumerable.Empty<Thing>(), CE_StatDefOf.WornBulk, (value) => value.ToString("0.#"), ref text);
            GetStatTooltipForThings(pawn.inventory?.innerContainer ?? Enumerable.Empty<Thing>(), CE_StatDefOf.Bulk, (value) => value.ToString("0.#"), ref text);

            return text;
        }

        /// <inheritdoc/>
        public override string TooltipTextFor(Thing thing, bool isForced)
        {
            ValidateArg.NotNull(thing, nameof(thing));

            return string.Concat(
                base.TooltipTextFor(thing, isForced)
                , UIText.Bulk.TranslateSimple(), ": ", thing.GetStatValue(CE_StatDefOf.Bulk) * thing.stackCount
                , thing.def.StatBaseDefined(CE_StatDefOf.WornBulk) ? Environment.NewLine + "WornBulk" + ": " + thing.GetStatValue(CE_StatDefOf.WornBulk) * thing.stackCount : string.Empty);
        }
    }
}
