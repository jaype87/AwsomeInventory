// <copyright file="DrawHelper.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the GPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

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
        public string TooltipTextFor(Thing thing, bool labelCap, bool isForced)
        {
            ValidateArg.NotNull(thing, nameof(thing));

            StringBuilder text = new StringBuilder();
            if (labelCap)
            {
                text.Append(thing.LabelCap);
                if (isForced)
                {
                    text.Append(", " + UIText.ApparelForcedLower.Translate());
                }
            }

            text.Append(thing.DescriptionDetailed);
            text.AppendLine();

            // hit points
            if (thing.def.useHitPoints)
            {
                text.AppendLine(thing.HitPoints + " / " + thing.MaxHitPoints);
            }

            // mass
            string mass = (thing.GetStatValue(StatDefOf.Mass, true) * (float)thing.stackCount).ToString("G") + " kg";
            text.AppendLine(mass);

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
    }
}
