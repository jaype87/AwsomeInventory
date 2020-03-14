// <copyright file="AIGenericMeal.cs" company="Zizhen Li">
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

namespace AwesomeInventory.Common.Loadout
{
    /// <summary>
    /// Generic meal def used for loadout purpose.
    /// </summary>
    public sealed class AIGenericMeal : AIGenericDef
    {
        private static AIGenericMeal _instance = null;

        private AIGenericMeal()
            : base(
                  DefNames.AIGenericMeal,
                  Descriptions.AIGenericMeal.Translate(),
                  Labels.AIGenericMeal.Translate(),
                  typeof(ThingWithComps),
                  ThingRequestGroup.FoodSourceNotPlantOrTree,
                  (ThingDef thingDef) => thingDef.IsNutritionGivingIngestible && thingDef.ingestible.preferability >= FoodPreferability.MealAwful && thingDef.GetCompProperties<CompProperties_Rottable>()?.daysToRotStart <= 5 && !thingDef.IsDrug)
        {
            this.statBases = new List<StatModifier>() { new StatModifier() { stat = StatDefOf.Mass, value = ThingDefOf.MealFine.BaseMass } };
        }

        /// <summary>
        /// Gets a singleton instance of <see cref="AIGenericMeal"/>.
        /// </summary>
        public static AIGenericMeal Instance => _instance ?? (_instance = new AIGenericMeal());
    }
}
