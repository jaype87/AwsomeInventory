// <copyright file="AIGenericMeal.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
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
    /// Generic meal def used for loadout purpose.
    /// </summary>
    public sealed class AIGenericMeal : AIGenericDef
    {
        private static AIGenericMeal _instance = null;

        private AIGenericMeal()
            : base(
                  DefNames.AIGenericMeal,
                  Descriptions.AIGenericMeal.TranslateSimple(),
                  Labels.AIGenericMeal.TranslateSimple(),
                  typeof(ThingWithComps),
                  new[] { ThingCategoryDefOf.FoodMeals },
                  null,
                  DefDatabase<ThingDef>.AllDefsListForReading.Where(
                      def => def.IsIngestible
                          && (VGPGardenUtility.IsActive
                              && def.IsSweet()))
                  .ToList())
        {
            // Use fine meal for mass.
            this.statBases = new List<StatModifier>() { new StatModifier() { stat = StatDefOf.Mass, value = ThingDefOf.MealFine.BaseMass } };
        }

        /// <summary>
        /// Gets a singleton instance of <see cref="AIGenericMeal"/>.
        /// </summary>
        public static AIGenericMeal Instance => _instance ?? (_instance = new AIGenericMeal());
    }
}
