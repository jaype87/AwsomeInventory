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
    public class GenericAmmo : AIGenericDef, IExposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GenericAmmo"/> class.
        /// </summary>
        /// <remarks> Only used for deserialization. </remarks>
        public GenericAmmo()
        {
        }

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

        /// <summary>
        /// Save ammo def.
        /// </summary>
        public void ExposeData()
        {
            Scribe_Values.Look(ref this.defName, nameof(this.defName));
            Scribe_Values.Look(ref this.description, nameof(this.description));
            Scribe_Values.Look(ref this.label, nameof(this.label));
            Scribe_Collections.Look(ref this.thingCategories, nameof(thingCategories), LookMode.Def);

            string thingClass = string.Empty;

            if (Scribe.mode == LoadSaveMode.Saving)
            {
                thingClass = this.thingClass.AssemblyQualifiedName;

                Scribe_Values.Look(ref thingClass, nameof(thingClass));
            }
            else if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                Scribe_Values.Look(ref thingClass, nameof(thingClass));
                this.thingClass = Type.GetType(thingClass);
            }
            else if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                this.Includes = (thingDef) => this.AvailableDefs.Contains(thingDef);
                this.AvailableDefs = from thingDef in this.ThingCategoryDefs.SelectMany(t => t.DescendantThingDefs).Distinct()
                                     where this.ExcepDefs.EnumerableNullOrEmpty() || !this.ExcepDefs.Contains(thingDef)
                                     select thingDef;
                this.tradeability = Tradeability.None;
                IEnumerable<AmmoDef> ammoDefs = this.thingCategories.SelectMany(t => t.DescendantThingDefs).OfType<AmmoDef>();
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
}
