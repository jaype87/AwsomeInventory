// <copyright file="ThingGroupSelector.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using AwesomeInventory.UI;
using RimWorld;
using Verse;

namespace AwesomeInventory.Loadout
{
    /// <summary>
    /// A gorup of <see cref="ThingSelector"/> that shares a common <see cref="ThingDef"/>.
    /// </summary>
    [DebuggerDisplay("{AllowedThing}")]
    public class ThingGroupSelector : ICollection<ThingSelector>, IExposable, ILoadReferenceable, IReset
    {
        private List<ThingSelector> _selectors = new List<ThingSelector>();
        private bool _isGenericDef;

        /// <summary>
        /// When true, pawn will not restock until stack count drop to _bottomThresholdCount.
        /// </summary>
        private bool _useBottomThreshold = false;

        private int _bottomThresholdCount = 0;

        /// <summary>
        /// A callback for stack count changed event.
        /// </summary>
        private List<Action<ThingGroupSelector, int>> _stackCountChangedCallbacks = new List<Action<ThingGroupSelector, int>>();

        /// <summary>
        /// A callback that would be invoked when a <see cref="ThingSelector"/> is added to this group.
        /// </summary>
        private List<Action<ThingSelector>> _addNewThingSelectorCallbacks = new List<Action<ThingSelector>>();

        /// <summary>
        /// A callback that would be invoked when a <see cref="ThingSelector"/> is removed from this group.
        /// </summary>
        private List<Action<ThingSelector>> _removeThingSelectorCallbacks = new List<Action<ThingSelector>>();

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
            this.AllowedStackCount = other.AllowedStackCount;
            this.AllowedThing = other.AllowedThing;
            _bottomThresholdCount = other._bottomThresholdCount;
            _useBottomThreshold = other._useBottomThreshold;

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
                    return UIText.Assorted.Translate(AllowedThing.LabelCap).ToString().Colorize(QualityColor.Instance.Generic);
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
        /// Gets a value indicating whether this selector uses bottom threshold.
        /// </summary>
        public bool UseBottomThreshold => _useBottomThreshold;

        /// <summary>
        /// Gets stack count for bottom threshold.
        /// </summary>
        public int BottomThresoldCount => _bottomThresholdCount;

        /// <inheritdoc/>
        public void Reset()
        {
            this.SetBottomThreshold(false, 0);
        }

        /// <summary>
        /// Set bottom threshold. When <paramref name="useBottomThreshold"/> is true, pawn won't restock until stack count falls to <paramref name="thresholdCount"/>.
        /// </summary>
        /// <param name="useBottomThreshold"> Whether should use bottom threshold. </param>
        /// <param name="thresholdCount"> Stack count for threshold. </param>
        public void SetBottomThreshold(bool useBottomThreshold, int thresholdCount)
        {
            _bottomThresholdCount = (_useBottomThreshold = useBottomThreshold) ? thresholdCount : 0;
            _stackCountChangedCallbacks.ForEach(action => action.Invoke(this, this.AllowedStackCount));
        }

        /// <summary>
        /// Add callback to stack-count-changed event.
        /// </summary>
        /// <param name="callback"> It would be invoked when stack count in this <see cref="ThingGroupSelector"/> is changed. </param>
        public void AddStackCountChangedCallback(Action<ThingGroupSelector, int> callback)
        {
            _stackCountChangedCallbacks.Add(callback);
        }

        /// <summary>
        /// Add callback to add-new-thingselector event.
        /// </summary>
        /// <param name="callback"> It would be invoked when a new <see cref="ThingSelector"/> is added to this <see cref="ThingGroupSelector"/>. </param>
        public void AddAddNewThingSelectorCallback(Action<ThingSelector> callback)
        {
            _addNewThingSelectorCallbacks.Add(callback);
        }

        /// <summary>
        /// Add callback to add-remove-thingselector event.
        /// </summary>
        /// <param name="callback"> It would be invoked when a <see cref="ThingSelector"/> is removed from this <see cref="ThingGroupSelector"/>. </param>
        public void AddRemoveThingSelectorCallback(Action<ThingSelector> callback)
        {
            _removeThingSelectorCallbacks.Add(callback);
        }

        /// <summary>
        /// Remove callback to stack-count-changed event.
        /// </summary>
        /// <param name="callback"> It would be invoked when stack count in this <see cref="ThingGroupSelector"/> is changed. </param>
        public void RemoveStackCountChangedCallback(Action<ThingGroupSelector, int> callback)
        {
            _stackCountChangedCallbacks.Remove(callback);
        }

        /// <summary>
        /// Remove callback to add-new-thingselector event.
        /// </summary>
        /// <param name="callback"> It would be invoked when a new <see cref="ThingSelector"/> is added to this <see cref="ThingGroupSelector"/>. </param>
        public void RemoveAddNewThingSelectorCallback(Action<ThingSelector> callback)
        {
            _addNewThingSelectorCallbacks.Remove(callback);
        }

        /// <summary>
        /// Remove callback to add-remove-thingselector event.
        /// </summary>
        /// <param name="callback"> It would be invoked when a <see cref="ThingSelector"/> is removed from this <see cref="ThingGroupSelector"/>. </param>
        public void RemoveRemoveThingSelectorCallback(Action<ThingSelector> callback)
        {
            _removeThingSelectorCallbacks.Remove(callback);
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
            this._stackCountChangedCallbacks.ToList().ForEach(action => action.Invoke(this, oldStackCount));
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
            this._addNewThingSelectorCallbacks.ForEach(action => action.Invoke(item));
        }

        /// <inheritdoc/>
        public void Clear()
        {
            _selectors.ForEach(t => this._removeThingSelectorCallbacks.ForEach(action => action.Invoke(t)));
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

            this._removeThingSelectorCallbacks.ForEach(action => action.Invoke(item));
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
            bool isGenericDef = false;
            int allowedStackCount = this.AllowedStackCount;
            int groupID = this.GroupID;

            if (Scribe.mode == LoadSaveMode.Saving)
            {
                if (thingDef is AIGenericDef aIGenericDef)
                {
                    isGenericDef = true;
                    Scribe_Defs.Look(ref aIGenericDef, nameof(this.AllowedThing));
                }
                else
                {
                    Scribe_Defs.Look(ref thingDef, nameof(this.AllowedThing));
                }

                Scribe_Values.Look(ref isGenericDef, nameof(isGenericDef));
            }
            else if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                Scribe_Values.Look(ref isGenericDef, nameof(isGenericDef));
                _isGenericDef = isGenericDef;
            }

            Scribe_Values.Look(ref _useBottomThreshold, nameof(_useBottomThreshold));
            Scribe_Values.Look(ref _bottomThresholdCount, nameof(_bottomThresholdCount));
            Scribe_Values.Look(ref allowedStackCount, nameof(this.AllowedStackCount));
            Scribe_Values.Look(ref groupID, nameof(this.GroupID));
            Scribe_Collections.Look(ref _selectors, nameof(_selectors), LookMode.Deep);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (_isGenericDef)
                {
                    this.AllowedThing = (_selectors.First() as GenericThingSelector).GenericDef;
                }
                else
                {
                    this.AllowedThing = (_selectors.First() as SingleThingSelector).AllowedThing;
                }
            }

            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
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
