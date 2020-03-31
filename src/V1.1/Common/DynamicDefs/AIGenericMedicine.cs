// <copyright file="AIGenericMedicine.cs" company="Zizhen Li">
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
    /// Generic medicine def used for loadout purpose.
    /// </summary>
    public class AIGenericMedicine : AIGenericDef
    {
        private static AIGenericMedicine _instance = new AIGenericMedicine();

        private AIGenericMedicine()
            : base(
                  DefNames.AIGenericMedicine,
                  Descriptions.AIGenericMedicine.TranslateSimple(),
                  Labels.AIGenericMedicine.TranslateSimple(),
                  typeof(Medicine),
                  new[] { ThingCategoryDefOf.Medicine },
                  null)
        {
            this.statBases = new List<StatModifier>() { new StatModifier() { stat = StatDefOf.Mass, value = DefDatabase<ThingDef>.GetNamed("Penoxycyline").BaseMass } };
        }

        /// <summary>
        /// Gets a singleton instance of <see cref="AIGenericMedicine"/>.
        /// </summary>
        public static AIGenericMedicine Instance { get => _instance ?? (_instance = new AIGenericMedicine()); }
    }
}
