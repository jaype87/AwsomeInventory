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
    [RegisterService(typeof(DrawHelper), typeof(DrawHelper))]
    public class DrawHelper
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DrawHelper"/> class.
        /// </summary>
        [Obsolete(ErrorText.NoDirectCall, false)]
        public DrawHelper()
        {
        }

        /// <summary>
        /// Build tooltip text for <paramref name="thing"/>.
        /// </summary>
        /// <param name="thing"> Thing that the tooltip text is for. </param>
        /// <param name="isForced"> True, if <paramref name="thing"/> is apparel and the wearer is forced to wear. </param>
        /// <returns> Tooltip text for <paramref name="thing"/>. </returns>
        public virtual string TooltipTextFor(Thing thing, bool isForced)
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
            text.AppendLine();

            if (thing.def.useHitPoints)
            {
                text.AppendLine(UIText.HitPointsBasic.Translate().CapitalizeFirst() + ": " + thing.HitPoints + " / " + thing.MaxHitPoints);
            }

            // Draw armor stats
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

            text.AppendLine();

            // mass
            string mass = (thing.GetStatValue(StatDefOf.Mass, true) * (float)thing.stackCount).ToString("G") + " kg";
            text.AppendLine(UIText.Mass.Translate() + ": " + mass);

            return text.ToString();
        }

        /// <summary>
        /// Build tooltip text on weight <paramref name="pawn"/> that carries.
        /// </summary>
        /// <param name="pawn"> Pawn who carries weight. </param>
        /// <returns> A tooltip text on weight <paramref name="pawn"/> carries. </returns>
        public virtual string WeightTextFor(Pawn pawn)
        {
            ValidateArg.NotNull(pawn, nameof(pawn));

            string text = string.Empty;

            GetStatTooltipForThings(pawn.equipment?.AllEquipmentListForReading ?? Enumerable.Empty<Thing>(), StatDefOf.Mass, (value) => value.ToStringMass(), ref text);
            GetStatTooltipForThings(pawn.apparel?.WornApparel ?? Enumerable.Empty<Thing>(), StatDefOf.Mass, (value) => value.ToStringMass(), ref text);
            GetStatTooltipForThings(pawn.inventory?.innerContainer ?? Enumerable.Empty<Thing>(), StatDefOf.Mass, (value) => value.ToStringMass(), ref text);

            return text;
        }

        /// <summary>
        /// Build a <paramref name="tooltip"/> text for <paramref name="things"/> on <paramref name="stat"/>.
        /// </summary>
        /// <param name="things"> A list of things that need tooltip text. </param>
        /// <param name="stat"> Stat for <paramref name="things"/>. </param>
        /// <param name="toString"> A function to transform float value to string presentation. </param>
        /// <param name="tooltip"> A string to build on.</param>
        protected static void GetStatTooltipForThings(IEnumerable<Thing> things, StatDef stat, Func<float, string> toString, ref string tooltip)
        {
            ValidateArg.NotNull(things, nameof(things));

            float value;
            StringBuilder stringBuilder = new StringBuilder(tooltip);
            foreach (Thing thing in things)
            {
                value = thing.GetStatValue(stat) * thing.stackCount;
                if (value > 0.1)
                {
                    stringBuilder.Append(thing.LabelShortCap);
                    stringBuilder.AppendWithSeparator(toString(value), ": ");
                    stringBuilder.AppendLine();
                }
            }

            tooltip = stringBuilder.ToString();
        }
    }
}
