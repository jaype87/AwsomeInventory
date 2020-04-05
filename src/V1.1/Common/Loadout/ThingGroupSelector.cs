// <copyright file="ThingGroupSelector.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections;
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
    /// A gorup of <see cref="ThingSelector"/> that shares a common <see cref="ThingDef"/>.
    /// </summary>
    public class ThingGroupSelector : ICollection<ThingSelector>, IExposable, ILoadReferenceable
    {
        private List<ThingSelector> _selectors = new List<ThingSelector>();

        /// <summary>
        /// A callback for stack count changed event.
        /// </summary>
        private Action<ThingGroupSelector, int> _stackCountChangedCallback;

        /// <summary>
        /// A callback that would be invoked when a <see cref="ThingSelector"/> is added to this group.
        /// </summary>
        private Action<ThingSelector> _addNewThingSelectorCallback;

        /// <summary>
        /// A callback that would be invoked when a <see cref="ThingSelector"/> is removed from this group.
        /// </summary>
        private Action<ThingSelector> _removeThingSelectorCallback;

        /// <summary>
        /// Initializes a new instance of the <see cref="ThingGroupSelector"/> class.
        /// </summary>
        /// It is reserved for xml serialization and should not be called elsewhere.
        public ThingGroupSelector()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ThingGroupSelector"/> class.
        /// </summary>
        /// <param name="allowedThing"> A <see cref="ThingDef"/> that is allowed by this selector. </param>
        public ThingGroupSelector(ThingDef allowedThing)
        {
            this.GroupID = LoadoutManager.ThingGroupSelectorID;
            this.AllowedThing = allowedThing;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ThingGroupSelector"/> class.
        /// </summary>
        /// <param name="other"> Copy <paramref name="other"/> to this selector. </param>
        public ThingGroupSelector(ThingGroupSelector other)
        {
            ValidateArg.NotNull(other, nameof(other));

            this.GroupID = LoadoutManager.ThingGroupSelectorID;
            AllowedStackCount = other.AllowedStackCount;
            AllowedThing = other.AllowedThing;

            foreach (ThingSelector thingSelector in other._selectors)
            {
                Type selectorType = thingSelector.GetType();
                ThingSelector newSelector = (ThingSelector)Activator.CreateInstance(selectorType, new object[] { thingSelector });
                this.Add(newSelector);
            }
        }

        /// <summary>
        /// Gets a list of <see cref="SingleThingSelector"/> in this group selector.
        /// </summary>
        public List<SingleThingSelector> SingleThingSelectors { get => this.OfType<SingleThingSelector>().ToList(); }

        /// <summary>
        /// Gets a <see cref="ThingDef"/> that is allowed by this selector.
        /// </summary>
        public ThingDef AllowedThing { get; private set; }

        /// <summary>
        /// Gets the stack count that is allowed.
        /// </summary>
        public int AllowedStackCount { get; private set; }

        /// <summary>
        /// Gets a colorized label without count that describes this selector.
        /// </summary>
        public string LabelCapNoCount
        {
            get
            {
                if (_selectors.Count == 1)
                {
                    return _selectors[0].LabelCapNoCount;
                }
                else
                {
                    return UIText.Assorted.Translate(AllowedThing.LabelCap).ToString().Colorize(AwesomeInventoryTex.Lavendar);
                }
            }
        }

        /// <summary>
        /// Gets weight for this thing group.
        /// </summary>
        public float Weight { get => _selectors.Average(s => s.Weight) * this.AllowedStackCount; }

        /// <summary>
        /// Gets ID for this group of selectors.
        /// </summary>
        public int GroupID { get; private set; }

        /// <inheritdoc/>
        public int Count => _selectors.Count;

        /// <inheritdoc/>
        public bool IsReadOnly => false;

        /// <summary>
        /// Add callback to stack-count-changed event.
        /// </summary>
        /// <param name="callback"> It would be invoked when stack count in this <see cref="ThingGroupSelector"/> is changed. </param>
        /// <returns> Returns true if <paramref name="callback"/> is added, otherwise, false when there is already a callback in place. </returns>
        public bool AddStackCountChangedCallback(Action<ThingGroupSelector, int> callback)
        {
            if (_stackCountChangedCallback == null)
            {
                _stackCountChangedCallback = callback;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Add callback to add-new-thingselector event.
        /// </summary>
        /// <param name="callback"> It would be invoked when a new <see cref="ThingSelector"/> is added to this <see cref="ThingGroupSelector"/>. </param>
        /// <returns> Returns true if <paramref name="callback"/> is added, otherwise, false when there is already a callback in place. </returns>
        public bool AddAddNewThingSelectorCallback(Action<ThingSelector> callback)
        {
            if (_addNewThingSelectorCallback == null)
            {
                _addNewThingSelectorCallback = callback;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Add callback to add-remove-thingselector event.
        /// </summary>
        /// <param name="callback"> It would be invoked when a <see cref="ThingSelector"/> is removed from this <see cref="ThingGroupSelector"/>. </param>
        /// <returns> Returns true if <paramref name="callback"/> is added, otherwise, false when there is already a callback in place. </returns>
        public bool AddRemoveThingSelectorCallback(Action<ThingSelector> callback)
        {
            if (_removeThingSelectorCallback == null)
            {
                _removeThingSelectorCallback = callback;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Add <see cref="ThingGroupSelector.QualityAndHitpointsChangedCallbackHandler(ThingFilter)"/> to <see cref="ThingSelector"/>.
        /// </summary>
        /// <param name="thingSelector"> Add Handler to <paramref name="thingSelector"/>. </param>
        public void AddQualityAndHitpointsChangedCallbackTo(ThingSelector thingSelector)
        {
            ValidateArg.NotNull(thingSelector, nameof(thingSelector));

            thingSelector.AddQualityAndHitpointsChangedCallback(this.QualityAndHitpointsChangedCallbackHandler);
        }

        /// <summary>
        /// Update stack count.
        /// </summary>
        /// <param name="stackCount"> Number to replace the old count. </param>
        public void SetStackCount(int stackCount)
        {
            int oldStackCount = this.AllowedStackCount;
            this.AllowedStackCount = stackCount;
            _selectors.ForEach(s => s.SetStackCount(stackCount));
            this._stackCountChangedCallback?.Invoke(this, oldStackCount);
        }

        /// <summary>
        /// Check if <paramref name="thing"/> is allowed to add to inventory.
        /// </summary>
        /// <param name="thing"> <see cref="Thing"/> to add. </param>
        /// <param name="thingSelector"> <see cref="ThingSelector"/> that <paramref name="thing"/> fits. </param>
        /// <returns> Returns true, if <paramref name="thing"/> is allowed to add. </returns>
        public bool Allows(Thing thing, out ThingSelector thingSelector)
        {
            IEnumerable<ThingSelector> allowedSelectors = _selectors.Where(s => s.Allows(thing));
            if (allowedSelectors.EnumerableNullOrEmpty())
            {
                thingSelector = null;
                return false;
            }
            else
            {
                IEnumerable<SingleThingSelector> singleThingSelectors = allowedSelectors.OfType<SingleThingSelector>();

                // If the selector is of type GenericThingSelector or it only has one SingleThingSelector in the enumerable, return that ThingSelector.
                if (singleThingSelectors.EnumerableNullOrEmpty() || singleThingSelectors.Count() == 1)
                {
                    thingSelector = allowedSelectors.First();
                    return true;
                }
                else
                {
                    thingSelector = Enumerable.OrderByDescending(singleThingSelectors, (t) => t, SingleThingSelector.Comparer.Instance).First();
                    return true;
                }
            }
        }

        /// <inheritdoc/>
        public void Add(ThingSelector item)
        {
            _selectors.Add(item);
            this.AddQualityAndHitpointsChangedCallbackTo(item);
            this._addNewThingSelectorCallback?.Invoke(item);
        }

        /// <inheritdoc/>
        public void Clear()
        {
            _selectors.ForEach(t => this._removeThingSelectorCallback?.Invoke(t));
            _selectors.Clear();
        }

        /// <inheritdoc/>
        public bool Contains(ThingSelector item)
        {
            return _selectors.Contains(item);
        }

        /// <inheritdoc/>
        public void CopyTo(ThingSelector[] array, int arrayIndex)
        {
            ValidateArg.NotNull(array, nameof(array));

            foreach (ThingSelector selector in _selectors)
            {
                array[arrayIndex++] = selector;
            }
        }

        /// <inheritdoc/>
        public IEnumerator<ThingSelector> GetEnumerator()
        {
            return _selectors.GetEnumerator();
        }

        /// <inheritdoc/>
        public bool Remove(ThingSelector item)
        {
            if (item == null)
                return false;

            this._removeThingSelectorCallback?.Invoke(item);
            return _selectors.Remove(item);
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <summary>
        /// Returns a unique ID for references when save/load the game.
        /// </summary>
        /// <returns> A unique string ID for references when save/load the game. </returns>
        public string GetUniqueLoadID()
        {
            return nameof(ThingGroupSelector) + this.GroupID;
        }

        /// <summary>
        /// Save state.
        /// </summary>
        public void ExposeData()
        {
            ThingDef thingDef = this.AllowedThing;
            int allowedStackCount = this.AllowedStackCount;
            int groupID = this.GroupID;

            Scribe_Defs.Look(ref thingDef, nameof(this.AllowedThing));
            Scribe_Values.Look(ref allowedStackCount, nameof(this.AllowedStackCount));
            Scribe_Values.Look(ref groupID, nameof(this.GroupID));
            Scribe_Collections.Look(ref _selectors, nameof(_selectors), LookMode.Deep);

            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                this.AllowedThing = thingDef;
                this.AllowedStackCount = allowedStackCount;
                this.GroupID = groupID;

                foreach (ThingSelector thingSelector in _selectors)
                {
                    this.AddQualityAndHitpointsChangedCallbackTo(thingSelector);
                }
            }
        }

        /// <summary>
        /// A function that handles quality and hit points changed callback invoked by underlying <see cref="ThingFilter"/>.
        /// </summary>
        private void QualityAndHitpointsChangedCallbackHandler(ThingFilter thingfilter)
        {
            // no op.
        }
    }
}
