// <copyright file="GenericAmmo.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CombatExtended;
using RimWorld;
using Verse;

namespace AwesomeInventory.Loadout
{
    /// <summary>
    /// Generic def for ammo.
    /// </summary>
    public class GenericAmmo : AIGenericDef
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GenericAmmo"/> class.
        /// </summary>
        /// <param name="defName"> Def Name. </param>
        /// <param name="description"> Description for this def. </param>
        /// <param name="label"> Label to display. </param>
        /// <param name="thingClass"> Type of Thing that this def defines. </param>
        /// <param name="ammoDefs"> A list of ammo defs. </param>
        public GenericAmmo(string defName, string description, string label, Type thingClass, IEnumerable<AmmoDef> ammoDefs)
            : base(defName, description, label, thingClass, ammoDefs.SelectMany(t => t.thingCategories).Distinct(), null)
        {
            ValidateArg.NotNull(ammoDefs, nameof(ammoDefs));

            this.statBases = new List<StatModifier>()
            {
                new StatModifier()
                {
                    stat = StatDefOf.Mass, value = ammoDefs.Average(t => t.GetStatValueAbstract(StatDefOf.Mass)),
                },
                new StatModifier()
                {
                    stat = CE_StatDefOf.Bulk, value = ammoDefs.Average(t => t.GetStatValueAbstract(CE_StatDefOf.Bulk)),
                },
            };
        }
    }
}
