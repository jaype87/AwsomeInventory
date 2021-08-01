// <copyright file="AIGenericMeat.cs" company="Zizhen Li">
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
    /// Generic meat def used for loadout purpose.
    /// </summary>
    public sealed class AIGenericMeat : AIGenericDef
    {
        private static AIGenericMeat _instance = null;

        private AIGenericMeat()
            : base(
                  DefNames.AIGenericMeatRaw
                , Descriptions.AIGenericMeatRaw.TranslateSimple()
                , Labels.AIGenericMeatRaw.TranslateSimple()
                , typeof(ThingWithComps)
                , new[] { ThingCategoryDefOf.MeatRaw })
        {
            this.statBases = new List<StatModifier>() { new StatModifier() { stat = StatDefOf.Mass, value = ThingDefOf.Meat_Human.BaseMass } };
        }

        /// <summary>
        /// Gets an instance of <see cref="AIGenericMeat "/>.
        /// </summary>
        public static AIGenericMeat Instance => _instance ?? (_instance = new AIGenericMeat());
    }
}
