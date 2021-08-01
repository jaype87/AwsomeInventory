// <copyright file="AIGenericDrugs.cs" company="Zizhen Li">
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
    /// Generic drugs for loadout purpose. Includes medical drugs.
    /// Its mass is the same as Flake.
    /// </summary>
    public class AIGenericDrugs : AIGenericDef
    {
        private static AIGenericDrugs _instance;

        private AIGenericDrugs()
            : base(
                  DefNames.AIGenericDrugs,
                  Descriptions.AIGenericDrugs.TranslateSimple(),
                  Labels.AIGenericDrugs.TranslateSimple(),
                  typeof(ThingWithComps),
                  new[] { ThingCategoryDefOf.Drugs },
                  null)
        {
            this.statBases = new List<StatModifier>() { new StatModifier() { stat = StatDefOf.Mass, value = DefDatabase<ThingDef>.GetNamed("Flake").BaseMass } };
        }

        /// <summary>
        /// Gets a singleton instance of <see cref="AIGenericDrugs"/>.
        /// </summary>
        public static AIGenericDrugs Instance { get => _instance ?? (_instance = new AIGenericDrugs()); }
    }
}
