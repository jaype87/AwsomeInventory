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
using AwesomeInventory.UI;
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
        /// Used by xml serialization, should not be called anywhere else.
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
            _thingFilter.SetAllow(thingDef, true);
            _allowedStuff = stuff;
            _thingFilter.AllowedQualityLevels = new QualityRange(QualityCategory.Normal, QualityCategory.Legendary);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SingleThingSelector"/> class.
        /// </summary>
        /// <param name="thing"> A template for initializing the selector. </param>
        public SingleThingSelector(Thing thing)
        {
            ValidateArg.NotNull(thing, nameof(thing));

            _allowedStuff = thing.Stuff;
            _allowedStackCount = thing.stackCount;

            _thingFilter.SetAllow(thing.def, true);

            if (thing.TryGetQuality(out QualityCategory qualityCategory))
            {
                _thingFilter.allowedQualitiesConfigurable = true;
                _thingFilter.AllowedQualityLevels = new QualityRange(qualityCategory, QualityCategory.Legendary);
            }
            else
            {
                _thingFilter.allowedQualitiesConfigurable = false;
            }

            if (thing.def.useHitPoints)
            {
                _thingFilter.allowedHitPointsConfigurable = true;
                _thingFilter.AllowedHitPointsPercents = new FloatRange(0.5f, 1);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SingleThingSelector"/> class.
        /// </summary>
        /// <param name="other"> Copy <paramref name="other"/> to this selector. </param>
        public SingleThingSelector(SingleThingSelector other)
        {
            ValidateArg.NotNull(other, nameof(other));

            _allowedStuff = other._allowedStuff;
            _allowedStackCount = other._allowedStackCount;

            _thingFilter = new ThingFilter();
            _thingFilter.SetAllow(other.AllowedThing, true);

            _thingFilter.allowedQualitiesConfigurable = other._thingFilter.allowedQualitiesConfigurable;
            _thingFilter.AllowedQualityLevels = other._thingFilter.AllowedQualityLevels;

            _thingFilter.allowedHitPointsConfigurable = other._thingFilter.allowedHitPointsConfigurable;
            _thingFilter.AllowedHitPointsPercents = other._thingFilter.AllowedHitPointsPercents;
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
        /// Gets the allowed hit points range.
        /// </summary>
        public FloatRange AllowedHitPoints { get => _thingFilter.AllowedHitPointsPercents; }

        /// <inheritdoc/>
        public override string LabelCapNoCount { get => this.ThingSample.LabelCapNoCount.ColorizeByQuality(this.ThingSample); }

        /// <inheritdoc/>
        public override float Weight => this.ThingSample.GetStatValue(StatDefOf.Mass);

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

        public QualityRange AllowedQualityLevel { get => _thingFilter.AllowedQualityLevels; }

        /// <summary>
        /// Set hit points.
        /// </summary>
        /// <param name="floatRange"> Hit points range to set. </param>
        public void SetHitPoints(FloatRange floatRange)
        {
            _thingFilter.AllowedHitPointsPercents = floatRange;
        }

        public FloatRange AllowedHitPointsPercent { get => _thingFilter.AllowedHitPointsPercents; }

        /// <inheritdoc/>
        public override bool Allows(Thing thing)
        {
            ValidateArg.NotNull(thing, nameof(thing));

            return _thingFilter.Allows(thing)
                && (_allowedStuff == null ? true : _allowedStuff.shortHash == thing.Stuff.shortHash);
        }

        /// <inheritdoc />
        public bool Equals(SingleThingSelector other)
        {
            if (other is null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return this.ID == other.ID;
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

        /// <summary>
        /// Save state.
        /// </summary>
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref _allowedStuff, nameof(_allowedStuff));
        }
    }
}
