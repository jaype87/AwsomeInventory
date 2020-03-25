// <copyright file="SingleThingSelector.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace AwesomeInventory.Loadout
{
    /// <summary>
    /// Select suitable thing for <see cref="AwesomeInventoryLoadout"/>.
    /// </summary>
    public class SingleThingSelector : ThingSelector, IEquatable<SingleThingSelector>
    {
        private ThingDef _allowedStuff;

        /// <summary>
        /// Initializes a new instance of the <see cref="SingleThingSelector"/> class.
        /// Used by xml serialization.
        /// </summary>
        public SingleThingSelector()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SingleThingSelector"/> class.
        /// </summary>
        /// <param name="thingDef"> <see cref="ThingDef"/> this selector is for. </param>
        /// <param name="stuff"> Stuff to make <paramref name="thingDef"/>. </param>
        public SingleThingSelector(ThingDef thingDef, ThingDef stuff = null)
        {
            _thingFilter = new ThingFilter();
            _thingFilter.SetAllow(thingDef, true);
            _allowedStuff = stuff;
        }

        /// <summary>
        /// Gets the <see cref="ThingDef"/> this selector is for.
        /// </summary>
        public ThingDef AllowedThing { get => _thingFilter.AllowedThingDefs.First(); }

        /// <summary>
        /// Gets a sample of thing that could be chosen by this selector.
        /// </summary>
        public Thing ThingSample
        {
            get =>
                LoadoutUtility.MakeThingWithoutID(
                    new ThingStuffPairWithQuality(this.AllowedThing, this.AllowedStuff, _thingFilter.AllowedQualityLevels.min));
        }

        /// <summary>
        /// Gets the stuff that is allowed.
        /// </summary>
        public ThingDef AllowedStuff { get => _allowedStuff; }

        /// <summary>
        /// Gets the allowed quality range.
        /// </summary>
        public QualityRange AllowedQualityRange { get => _thingFilter.AllowedQualityLevels; }

        /// <summary>
        /// Gets the allowed hit points range.
        /// </summary>
        public FloatRange AllowedHitPoints { get => _thingFilter.AllowedHitPointsPercents; }

        /// <inheritdoc/>
        public override string LabelNoCount { get => this.ThingSample.LabelCapNoCount; }

        /// <summary>
        /// Compare equality between <paramref name="A"/> and <paramref name="B"/>.
        /// </summary>
        /// <param name="A"> <see cref="SingleThingSelector"/> A to check equality. </param>
        /// <param name="B"> <see cref="SingleThingSelector"/> B to check equality. </param>
        /// <returns> Returns true if <paramref name="A"/> equals to <paramref name="B"/>. </returns>
        public static bool operator ==(SingleThingSelector A, SingleThingSelector B)
        {
            if (ReferenceEquals(A, B))
                return true;

            if (A is null)
                return false;
            else
                return A.Equals(B);
        }

        /// <summary>
        /// Compare inequality between <paramref name="A"/> and <paramref name="B"/>.
        /// </summary>
        /// <param name="A"> <see cref="SingleThingSelector"/> A to check equality. </param>
        /// <param name="B"> <see cref="SingleThingSelector"/> B to check equality. </param>
        /// <returns> Returns true if <paramref name="A"/> is not equals to <paramref name="B"/>. </returns>
        public static bool operator !=(SingleThingSelector A, SingleThingSelector B)
        {
            return !(A == B);
        }

        /// <summary>
        /// Add stuff to the allowed list.
        /// </summary>
        /// <param name="stuffDef"> Stuff to add. </param>
        public void SetStuff(ThingDef stuffDef)
        {
            _allowedStuff = stuffDef;
        }

        /// <summary>
        /// Set quality range.
        /// </summary>
        /// <param name="qualityRange"> Range to set. </param>
        public void SetQualityRange(QualityRange qualityRange)
        {
            _thingFilter.AllowedQualityLevels = qualityRange;
        }

        /// <summary>
        /// Set hit points.
        /// </summary>
        /// <param name="floatRange"> Hit points range to set. </param>
        public void SetHitPoints(FloatRange floatRange)
        {
            _thingFilter.AllowedHitPointsPercents = floatRange;
        }

        /// <inheritdoc/>
        public override bool Allows(Thing thing, int inventoryLevel)
        {
            ValidateArg.NotNull(thing, nameof(thing));

            return _thingFilter.Allows(thing)
                && _allowedStuff == null ? true : _allowedStuff.defName == thing.Stuff.defName
                && inventoryLevel + thing.stackCount <= _allowedStackCount;
        }

        /// <inheritdoc />
        public bool Equals(SingleThingSelector other)
        {
            if (other is null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return this.AllowedThing.defName == other.AllowedThing.defName
                && this._allowedStuff == other._allowedStuff;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return Equals(obj as SingleThingSelector);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return this.GetHashCode();
        }
    }
}
