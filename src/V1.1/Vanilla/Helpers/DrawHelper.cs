// <copyright file="DrawHelper.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the GPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace AwesomeInventory.UI
{
    /// <summary>
    /// A helper class for drawing in game.
    /// It only consists of member methods while <see cref="DrawUtility"/> only has static methods.
    /// </summary>
    public class DrawHelper : IDrawHelper
    {
        /// <inheritdoc/>
        public string TooltipTextFor(Thing thing, bool isForced)
        {
            ValidateArg.NotNull(thing, nameof(thing));

            StringBuilder text = new StringBuilder();
            text.Append(thing.LabelCap);
            if (isForced)
            {
                text.Append(", " + UIText.ApparelForcedLower.Translate());
            }

            text.AppendLine();

            if (thing.def.IsRangedWeapon && thing.holdingOwner?.Owner.ParentHolder is Pawn selPawn && selPawn.story != null && selPawn.story.traits.HasTrait(TraitDefOf.Brawler))
            {
                text.AppendLine(UIText.EquipWarningBrawler.Translate());
                text.AppendLine();
            }

            text.AppendLine(thing.DescriptionDetailed);
            if (thing.def.useHitPoints)
            {
                text.AppendLine(UIText.HitPointsBasic.Translate().CapitalizeFirst() + ": " + thing.HitPoints + " / " + thing.MaxHitPoints);
            }

            // mass
            string mass = (thing.GetStatValue(StatDefOf.Mass, true) * (float)thing.stackCount).ToString("G") + " kg";
            text.AppendLine(UIText.Mass.Translate() + ": " + mass);

            if (thing is Apparel app)
            {
                float sharp = app.GetStatValue(StatDefOf.ArmorRating_Sharp);
                float blunt = app.GetStatValue(StatDefOf.ArmorRating_Blunt);
                float heat = app.GetStatValue(StatDefOf.ArmorRating_Heat);

                if (sharp > 0.005)
                {
                    text.AppendLine(UIText.ArmorSharp.Translate() + ":" + sharp.ToStringPercent());
                }

                if (blunt > 0.005)
                {
                    text.AppendLine(string.Concat(UIText.ArmorBlunt.Translate(), ":", blunt.ToStringPercent()));
                }

                if (heat > 0.005)
                {
                    text.AppendLine(string.Concat(UIText.ArmorHeat.Translate(), ":", heat.ToStringPercent()));
                }
            }

            return text.ToString();
        }

        /// <inheritdoc/>
        public string WeightTextFor(Pawn pawn)
        {
            ValidateArg.NotNull(pawn, nameof(pawn));

            StringBuilder stringBuilder = new StringBuilder();

            if (pawn.equipment != null)
            {
                IEnumerable<Tuple<ThingWithComps, float>> tuples =
                    pawn.equipment.AllEquipmentListForReading
                    .Select(e => Tuple.Create(e, e.GetStatValue(StatDefOf.Mass)))
                    .OrderByDescending(t => t.Item2);

                foreach (var tuple in tuples)
                {
                    stringBuilder.Append(tuple.Item1.LabelCap);
                    stringBuilder.AppendWithSeparator(tuple.Item2.ToStringMass(), ": ");
                    stringBuilder.AppendLine();
                }
            }

            if (pawn.apparel != null)
            {
                IEnumerable<Tuple<Apparel, float>> tuples =
                    pawn.apparel.WornApparel
                    .Select(e => Tuple.Create(e, e.GetStatValue(StatDefOf.Mass)))
                    .OrderByDescending(t => t.Item2);

                foreach (var tuple in tuples)
                {
                    stringBuilder.Append(tuple.Item1.LabelCap);
                    stringBuilder.AppendWithSeparator(tuple.Item2.ToStringMass(), ": ");
                    stringBuilder.AppendLine();
                }
            }

            if (pawn.inventory != null)
            {
                IEnumerable<Tuple<Thing, float>> tuples =
                    pawn.inventory.innerContainer
                    .Select(e => Tuple.Create(e, e.GetStatValue(StatDefOf.Mass) * e.stackCount))
                    .OrderByDescending(t => t.Item2);

                foreach (var tuple in tuples)
                {
                    stringBuilder.Append(tuple.Item1.LabelCap);
                    stringBuilder.AppendWithSeparator(tuple.Item2.ToStringMass(), ": ");
                    stringBuilder.AppendLine();
                }
            }

            return stringBuilder.ToString();
        }
    }
}
