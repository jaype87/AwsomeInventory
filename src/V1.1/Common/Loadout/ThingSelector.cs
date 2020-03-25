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
    public abstract class ThingSelector : IExposable, IEquatable<ThingSelector>
    {
        /// <summary>
        /// Filter used for screening things.
        /// </summary>
        protected ThingFilter _thingFilter;

        /// <summary>
        /// The stack count allowed by this selector.
        /// </summary>
        protected int _allowedStackCount;

        /// <summary>
        /// Gets a list of callbacks to be invoked whenever stack count for this selector is changed.
        /// </summary>
        protected List<Action<int>> _stackCountChangedCallback = new List<Action<int>>();

        private static uint nextID = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="ThingSelector"/> class.
        /// </summary>
        public ThingSelector()
        {
        }

        /// <summary>
        /// Gets a label without count that describes this selector.
        /// </summary>
        public abstract string LabelNoCount { get; }

        /// <summary>
        /// Gets the ID for this selector.
        /// </summary>
        public uint ID { get; } = nextID++;

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
            else if (ReferenceEquals(A, null))
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
        /// Update stack count.
        /// </summary>
        /// <param name="stackCount"> Number to replace the old count. </param>
        public void SetStackCount(int stackCount)
        {
            _allowedStackCount = stackCount;
            _stackCountChangedCallback.ForEach(a => a.Invoke(stackCount));
        }

        /// <summary>
        /// Add callback to stack count changed event.
        /// </summary>
        /// <param name="callback"> Callback to add. </param>
        public void AddStackCountChangedCallback(Action<int> callback)
        {
            ValidateArg.NotNull(callback, nameof(callback));
            _stackCountChangedCallback.Add(callback);
        }

        /// <summary>
        /// Remove callback from stack count changed event.
        /// </summary>
        /// <param name="callback"> Callback to remove. </param>
        public void RemoveStackCountChangedCallback(Action<int> callback)
        {
            ValidateArg.NotNull(callback, nameof(callback));
            _stackCountChangedCallback.Remove(callback);
        }

        /// <summary>
        /// Check if <paramref name="thing"/> is allowed to add to inventory given current <paramref name="inventoryLevel"/>.
        /// </summary>
        /// <param name="thing"> <see cref="Thing"/> to add. </param>
        /// <param name="inventoryLevel"> Current inventory level. </param>
        /// <returns> Returns true, if <paramref name="thing"/> is allowed to add. </returns>
        public abstract bool Allows(Thing thing, int inventoryLevel);

        /// <summary>
        /// Save state.
        /// </summary>
        public virtual void ExposeData()
        {
            Scribe_Values.Look(ref _allowedStackCount, nameof(_allowedStackCount));
            Scribe_Deep.Look(ref _thingFilter, nameof(_thingFilter));
        }

        /// <inheritdoc/>
        public bool Equals(ThingSelector other)
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
    }
}
