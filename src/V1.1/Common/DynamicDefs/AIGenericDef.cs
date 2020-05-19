// <copyright file="AIGenericDef.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using RimWorld;
using Verse;

namespace AwesomeInventory.Loadout
{
    /// <summary>
    /// Generic def used for loadout purpose.
    /// </summary>
    public class AIGenericDef : ThingDef, IEquatable<AIGenericDef>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AIGenericDef"/> class.
        /// </summary>
        public AIGenericDef()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AIGenericDef"/> class.
        /// </summary>
        /// <param name="defName"> Def Name. </param>
        /// <param name="description"> Description for this def. </param>
        /// <param name="label"> Label to display. </param>
        /// <param name="thingClass"> Type of Thing that this def defines. </param>
        /// <param name="thingCategoryDefs"> The group to which this def belongs. </param>
        /// <param name="moreDefs"> Additinal defs to add. </param>
        /// <param name="exceptDefs"> Defs should not be in this category. </param>
        protected AIGenericDef(string defName, string description, string label, Type thingClass, IEnumerable<ThingCategoryDef> thingCategoryDefs, IEnumerable<ThingDef> moreDefs = null, IEnumerable<ThingDef> exceptDefs = null)
        {
            this.defName = defName;
            this.description = description;
            this.label = label;
            this.thingClass = thingClass;
            this.ExcepDefs = exceptDefs;
            this.ThingCategoryDefs = thingCategoryDefs;
            this.Includes = (thingDef) => this.AvailableDefs.Contains(thingDef);
            this.AvailableDefs = (from thingDef in this.ThingCategoryDefs.SelectMany(t => t.DescendantThingDefs).Distinct()
                                 where this.ExcepDefs.EnumerableNullOrEmpty() || !this.ExcepDefs.Contains(thingDef)
                                 select thingDef).Concat(moreDefs ?? Enumerable.Empty<ThingDef>());
            this.tradeability = Tradeability.None;
        }

        /// <summary>
        /// Gets or sets available <see cref="ThingDef"/> in this generic category.
        /// </summary>
        public IEnumerable<ThingDef> AvailableDefs { get; protected set; }

        /// <summary>
        /// Gets a group used for requesting things from <see cref="ListerThings"/>.
        /// </summary>
        public IEnumerable<ThingCategoryDef> ThingCategoryDefs { get => thingCategories; private set => thingCategories = value.ToList(); }

        /// <summary>
        /// Gets or sets a predicate function which returns true if <see cref="ThingDef"/> belongs.
        /// </summary>
        public virtual Predicate<ThingDef> Includes { get; protected set; }

        /// <summary>
        /// Gets a filter that rules out <see cref="ThingDef"/> that does not belong.
        /// </summary>
        public IEnumerable<ThingDef> ExcepDefs { get; }

        /// <summary>
        /// Compare equality between <paramref name="a"/> and <paramref name="b"/>.
        /// </summary>
        /// <param name="a"> Def to compare to <paramref name="b"/>. </param>
        /// <param name="b"> Def to compare to <paramref name="a"/>. </param>
        /// <returns> Returns true if <paramref name="a"/> equals <paramref name="b"/>. </returns>
        [SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "Bug of StyleCop")]
        public static bool operator ==(AIGenericDef a, AIGenericDef b)
        {
            return object.ReferenceEquals(a, b);
        }

        /// <summary>
        /// Compare inequality between <paramref name="a"/> and <paramref name="b"/>.
        /// </summary>
        /// <param name="a"> Def to compare to <paramref name="b"/>. </param>
        /// <param name="b"> Def to compare to <paramref name="a"/>. </param>
        /// <returns> Returns true if <paramref name="a"/> is not equal to <paramref name="b"/>. </returns>
        public static bool operator !=(AIGenericDef a, AIGenericDef b)
        {
            return !(a == b);
        }

        /// <inheritdoc/>
        public bool Equals(AIGenericDef other)
        {
            return this == other;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return Equals(obj as AIGenericDef);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
