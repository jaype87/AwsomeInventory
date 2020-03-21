// <copyright file="AIGenericDef.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the GPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Diagnostics.CodeAnalysis;
using RimWorld;
using Verse;

namespace AwesomeInventory.Loadout
{
    /// <summary>
    /// Generic def used for loadout purpose.
    /// </summary>
    public abstract class AIGenericDef : ThingDef, IEquatable<AIGenericDef>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AIGenericDef"/> class.
        /// </summary>
        /// <param name="defName"> Def Name. </param>
        /// <param name="description"> Description for this def. </param>
        /// <param name="label"> Label to display. </param>
        /// <param name="thingClass"> Type of Thing that this def defines. </param>
        /// <param name="thingRequestGroup"> Group the this def belongs. </param>
        /// <param name="filter"> Filter that help defines this def. </param>
        protected AIGenericDef(string defName, string description, string label, Type thingClass, ThingRequestGroup thingRequestGroup, Predicate<ThingDef> filter = null)
        {
            this.defName = defName;
            this.description = description;
            this.label = label;
            this.thingClass = thingClass;
            this.Filter = filter;
            this.ThingRequestGroup = thingRequestGroup;
            this.Includes = (thingDef) => this.ThingRequestGroup.Includes(thingDef) && (filter?.Invoke(thingDef) ?? true);
        }

        /// <remarks> Enforce singleton pattern. </remarks>
        private AIGenericDef()
        {
        }

        /// <summary>
        /// Gets a group used for requesting things from <see cref="ListerThings"/>.
        /// </summary>
        public ThingRequestGroup ThingRequestGroup { get; }

        /// <summary>
        /// Gets a predicate function which returns true if <see cref="ThingDef"/> belongs.
        /// </summary>
        public Predicate<ThingDef> Includes { get; }

        /// <summary>
        /// Gets a filter that rules out <see cref="ThingDef"/> that does not belong.
        /// </summary>
        protected Predicate<ThingDef> Filter { get; }

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
