// <copyright file="AIGenericRawFood.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the GPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AwesomeInventory.Resources;
using RimWorld;
using Verse;

namespace AwesomeInventory.Loadout
{
    /// <summary>
    /// Generic raw food def for loadout purpose.
    /// </summary>
    public class AIGenericRawFood : AIGenericDef
    {
        private static AIGenericRawFood _instance = null;

        private AIGenericRawFood()
            : base(
                  DefNames.AIGenericRawFood,
                  Descriptions.AIGenericRawFood.TranslateSimple(),
                  Labels.AIGenericRawFood.TranslateSimple(),
                  typeof(ThingWithComps),
                  ThingRequestGroup.HaulableAlways,
                  (ThingDef thingDef) => thingDef.IsNutritionGivingIngestible && !thingDef.IsCorpse && thingDef.ingestible.HumanEdible && !thingDef.HasComp(typeof(CompHatcher)) && thingDef.ingestible.preferability < FoodPreferability.MealAwful)
        {
            this.statBases = new List<StatModifier>() { new StatModifier() { stat = StatDefOf.Mass, value = ThingDefOf.Meat_Human.BaseMass } };
        }

        /// <summary>
        /// Gets a singleton instance of <see cref="AIGenericRawFood"/>.
        /// </summary>
        public static AIGenericRawFood Instance { get => _instance ?? (_instance = new AIGenericRawFood()); }
    }
}
