// <copyright file="DrawHelper.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using CombatExtended;
using RimWorld;
using UnityEngine;
using Verse;

namespace AwesomeInventory.UI
{
    /// <summary>
    /// A helper class for drawing in game.
    /// It only consists of member methods while <see cref="DrawUtility"/> only has static methods.
    /// </summary>
    public class DrawHelper : IDrawHelper
    {
        /// <summary>
        /// Build a tooltip string for gears worn by <paramref name="pawn"/>.
        /// </summary>
        /// <param name="pawn"> Pawn that the tooltip is for. </param>
        /// <returns> A tooltip on bulk for gears worn by <paramref name="pawn"/>. </returns>
        public static string BulkBreakdownTooltip(Pawn pawn)
        {
            ValidateArg.NotNull(pawn, nameof(pawn));

            string text = string.Empty;
            float value;

            GetBulkTextFromThings(pawn.equipment.AllEquipmentListForReading, ref text);
            GetBulkTextFromThings(pawn.apparel.WornApparel, ref text);
            GetBulkTextFromThings(pawn.inventory.innerContainer, ref text);

            void GetBulkTextFromThings(IEnumerable<Thing> things, ref string tooltip)
            {
                foreach (Thing thing in things)
                {
                    value = thing.GetStatValue(CE_StatDefOf.Bulk) * thing.stackCount;
                    if (value > 0.1)
                    {
                        tooltip += string.Concat(thing.LabelShortCap, ": ", value, Environment.NewLine);
                    }
                }
            }

            return text;
        }

        public string TooltipTextFor(Thing thing, bool isForced)
        {
            throw new System.NotImplementedException();
        }

        public string WeightTextFor(Pawn pawn)
        {
            throw new System.NotImplementedException();
        }
    }
}
