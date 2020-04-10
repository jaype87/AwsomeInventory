// <copyright file="ThingSelector.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace AwesomeInventory.Loadout
{
    /// <summary>
    /// Defines basic funtionality for a thing selector.
    /// </summary>
    public abstract class ThingSelector
        : IExposable, IEquatable<ThingSelector>
    {
        /// <summary>
        /// True if stuff source, quality level or hit point percentage has changed since last read.
        /// </summary>
        protected bool _dirty = true;

        /// <summary>
        /// Only its <see cref="ThingFilter.AllowedThingDefs"/>, <see cref="ThingFilter.AllowedQualityLevels"/>
        /// and <see cref="ThingFilter.AllowedHitPointsPercents"/> are used for filter purpose.
        /// </summary>
        protected ThingFilter _thingFilter;

        /// <summary>
        /// The stack count allowed by this selector.
        /// </summary>
        protected int _allowedStackCount;

        private static int nextID = 0;

        /// <summary>
        /// A callback which will be invoked whenever there is a change in the <see cref="ThingSelector._thingFilter"/>.
        /// </summary>
        private Action<ThingFilter> _qualityAndHitpointsChangedCallback;

        /// <summary>
        /// Initializes a new instance of the <see cref="ThingSelector"/> class.
        /// </summary>
        public ThingSelector()
        {
            _thingFilter = new ThingFilter(this.QualityAndHitpointsChangedCallback);
        }

        /// <summary>
        /// Gets a colored label without count that describes this selector.
        /// </summary>
        public abstract string LabelCapNoCount { get; }

        /// <summary>
        /// Gets weight of the selected thing.
        /// </summary>
        public abstract float Weight { get; }

        /// <summary>
        /// Gets hit points percentage of items that is allowed by this selector.
        /// </summary>
        public FloatRange AllowedHitPointsPercent { get => _thingFilter.AllowedHitPointsPercents; }

        /// <summary>
        /// Gets the ID for this selector.
        /// </summary>
        public int ID { get; } = nextID++;

        /// <summary>
        /// Gets the stack count that is allowed.
        /// </summary>
        public int AllowedStackCount { get => _allowedStackCount; }

        /// <summary>
        /// Compare equality between <paramref name="A"/> and <paramref name="B"/>.
        /// </summary>
        /// <param name="A"> Compare this <see cref="ThingSelector"/> to <paramref name="B"/>. </param>
        /// <param name="B"> Compare this <see cref="ThingSelector"/> to <paramref name="A"/>. </param>
        /// <returns> Returns true if <paramref name="A"/> is equal to <paramref name="B"/>. </returns>
        public static bool operator ==(ThingSelector A, ThingSelector B)
        {
            if (ReferenceEquals(A, B))
                return true;
            else if (A is null)
                return false;
            else
                return A.Equals(B);
        }

        /// <summary>
        /// Compare equality between <paramref name="A"/> and <paramref name="B"/>.
        /// </summary>
        /// <param name="A"> Compare this <see cref="ThingSelector"/> to <paramref name="B"/>. </param>
        /// <param name="B"> Compare this <see cref="ThingSelector"/> to <paramref name="A"/>. </param>
        /// <returns> Returns true if <paramref name="A"/> is not equal to <paramref name="B"/>. </returns>
        public static bool operator !=(ThingSelector A, ThingSelector B)
        {
            return !(A == B);
        }

        /// <summary>
        /// Set hit points.
        /// </summary>
        /// <param name="floatRange"> Hit points range to set. </param>
        public void SetHitPoints(FloatRange floatRange)
        {
            _thingFilter.AllowedHitPointsPercents = floatRange;
        }

        /// <summary>
        /// Update stack count.
        /// </summary>
        /// <param name="stackCount"> Number to replace the old count. </param>
        public virtual void SetStackCount(int stackCount)
        {
            _allowedStackCount = stackCount;
        }

        /// <summary>
        /// Check if <paramref name="thing"/> is allowed to add to inventory.
        /// </summary>
        /// <param name="thing"> <see cref="Thing"/> to add. </param>
        /// <returns> Returns true, if <paramref name="thing"/> is allowed to add. </returns>
        public abstract bool Allows(Thing thing);

        /// <summary>
        /// Add callback to the quality-hitpoints-changed event.
        /// </summary>
        /// <param name="callback"> Callback to add. </param>
        /// <returns> Returns true if <paramref name="callback"/> is added, otherwise, false when there is already a callback in place. </returns>
        public bool AddQualityAndHitpointsChangedCallback(Action<ThingFilter> callback)
        {
            if (_qualityAndHitpointsChangedCallback == null)
            {
                _qualityAndHitpointsChangedCallback = callback;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Save state.
        /// </summary>
        public virtual void ExposeData()
        {
            Scribe_Values.Look(ref _allowedStackCount, nameof(_allowedStackCount));
            Scribe_Deep.Look(ref _thingFilter, nameof(_thingFilter), new Action(this.QualityAndHitpointsChangedCallback));
        }

        /// <inheritdoc/>
        public virtual bool Equals(ThingSelector other)
        {
            if (ReferenceEquals(other, null))
                return false;

            return this.ID == other.ID;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return Equals(obj as ThingSelector);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return this.ID.GetHashCode();
        }

        /// <summary>
        /// Bubble up thing filter changed callback.
        /// </summary>
        private void QualityAndHitpointsChangedCallback()
        {
            _dirty = true;
            _qualityAndHitpointsChangedCallback?.Invoke(_thingFilter);
        }
    }
}
