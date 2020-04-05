// <copyright file="AwesomeInventoryLoadout.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AwesomeInventory.UI;
using RimWorld;
using UnityEngine;
using Verse;

namespace AwesomeInventory.Loadout
{
    /// <summary>
    /// It inherits from the "Outfit" class, is added to outfitDatabase and holds information about loadout.
    /// </summary>
    /// <remarks>
    /// <para>
    ///     The reason for these callback calling design is to prevent code outside the callback loop to
    ///  invoke the callbacks. TAS pattern can be used to make the code cleaner but exposed to the risk of
    ///  race condition in multi-player mod and future iteration in the vanilla game code. In such cases,
    ///  locking is desired. As for now, a good old callback loop will suffice.
    /// </para>
    /// </remarks>
    public class AwesomeInventoryLoadout : Outfit, IList<ThingGroupSelector>
    {
        /// <summary>
        /// A collection of <see cref="ThingGroupSelector"/>s that dictates what items to look for.
        /// </summary>
        private List<ThingGroupSelector> _thingGroupSelectors = new List<ThingGroupSelector>();

        /// <summary>
        /// Items fit in this selectors will be excluded from things the pawn are actively searching for.
        /// </summary>
        private List<ThingGroupSelector> _blacklistSelectors = new List<ThingGroupSelector>();

        /// <summary>
        /// Gets callbacks that are raised whenever a <see cref="ThingGroupSelector"/> is added to this loadout.
        /// </summary>
        private List<Action<ThingGroupSelector>> _addNewThingGroupSelectorCallbacks = new List<Action<ThingGroupSelector>>();

        /// <summary>
        /// Gets callbacks that are raised whenever a <see cref="ThingGroupSelector"/> is removed from this loadout.
        /// </summary>
        private List<Action<ThingGroupSelector>> _removeThingGroupSelectorCallbacks = new List<Action<ThingGroupSelector>>();

        /// <summary>
        /// Gets callbacks that are raised whenever stack count in a <see cref="ThingGroupSelector"/> is changed..
        /// </summary>
        private List<Action<ThingGroupSelector, int>> _thingGroupSelectorStackCountChangedCallbacks = new List<Action<ThingGroupSelector, int>>();

        /// <summary>
        /// If true, this loadout has changed since last read.
        /// </summary>
        private bool _isDirty = true;

        private float _weight;

        /// <summary>
        /// Initializes a new instance of the <see cref="AwesomeInventoryLoadout"/> class.
        /// </summary>
        public AwesomeInventoryLoadout()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AwesomeInventoryLoadout"/> class.
        /// </summary>
        /// <param name="other"> Copy <paramref name="other"/> to this loadout. </param>
        public AwesomeInventoryLoadout(AwesomeInventoryLoadout other)
        {
            ValidateArg.NotNull(other, nameof(other));

            foreach (ThingGroupSelector selector in other._thingGroupSelectors)
            {
                ThingGroupSelector newSelector = new ThingGroupSelector(selector);
                this.Add(newSelector);
            }

            this.uniqueId = Current.Game.outfitDatabase.AllOutfits.Max(o => o.uniqueId) + 1;
            this.label = LoadoutManager.GetIncrementalLabel(other.label);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AwesomeInventoryLoadout"/> class.
        /// </summary>
        /// <param name="pawn"> Initialize <see cref="AwesomeInventoryLoadout"/> with items on this <paramref name="pawn"/>. </param>
        public AwesomeInventoryLoadout(Pawn pawn)
        {
            ValidateArg.NotNull(pawn, nameof(pawn));

            this.AddItems(pawn.equipment?.AllEquipmentListForReading);
            this.AddItems(pawn.apparel?.WornApparel);
            this.AddItems(pawn.inventory?.innerContainer);

            this.uniqueId = Current.Game.outfitDatabase.AllOutfits.Max(o => o.uniqueId) + 1;
            CompAwesomeInventoryLoadout compLoadout = pawn.TryGetComp<CompAwesomeInventoryLoadout>();
            this.label = compLoadout?.Loadout == null
                ? AwesomeInventoryLoadout.GetDefaultLoadoutName(pawn)
                : LoadoutManager.GetIncrementalLabel(compLoadout.Loadout.label);

            pawn.SetLoadout(this);
        }

        /// <summary>
        /// Gets weight for this loadout.
        /// </summary>
        public float Weight
        {
            get
            {
                if (_isDirty)
                    this.UpdateReadout();

                return _weight;
            }
        }

        /// <inheritdoc/>
        [SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1201:Elements should appear in the correct order", Justification = "Interface implementation")]
        public int Count => _thingGroupSelectors.Count;

        /// <inheritdoc/>
        public bool IsReadOnly => false;

        /// <summary>
        /// Gets a blacklist of items that thie loadout does not accept.
        /// </summary>
        public List<ThingGroupSelector> BlackList => _blacklistSelectors;

        /// <inheritdoc/>
        public ThingGroupSelector this[int index]
        {
            get => _thingGroupSelectors[index];
            set => _thingGroupSelectors[index] = value;
        }

        /// <summary>
        /// Check if <paramref name="thing"/> is included in the blacklist.
        /// </summary>
        /// <param name="thing"> Thing to check. </param>
        /// <returns> Returns true if <paramref name="thing"/> is in the blacklist. </returns>
        public bool IncludedInBlacklist(Thing thing)
        {
            return _blacklistSelectors.Any(s => s.Allows(thing, out _));
        }

        /// <summary>
        /// Add new item to loadout.
        /// </summary>
        /// <param name="item"> Item to add. </param>
        public void Add(ThingGroupSelector item)
        {
            ValidateArg.NotNull(item, nameof(item));

            this.AddAddNewThingSelectorCallbackTo(item);
            this.AddRemoveThingSelectorCallbackTo(item);
            this.AddStackCountChangedCallbackTo(item);
            _thingGroupSelectors.Add(item);
            this.filter.SetAllow(item.AllowedThing, true);
            _addNewThingGroupSelectorCallbacks.ForEach(c => c.Invoke(item));
        }

        /// <inheritdoc/>
        public void Clear()
        {
            _thingGroupSelectors.ForEach(
                group => _removeThingGroupSelectorCallbacks.ForEach(
                    c => c.Invoke(group)));

            _thingGroupSelectors.Clear();
        }

        /// <inheritdoc/>
        public bool Contains(ThingGroupSelector item)
        {
            return _thingGroupSelectors.Contains(item);
        }

        /// <inheritdoc/>
        public void CopyTo(ThingGroupSelector[] array, int arrayIndex)
        {
            ValidateArg.NotNull(array, nameof(array));

            foreach (ThingGroupSelector thingSelector in this)
            {
                array[arrayIndex++] = thingSelector;
            }
        }

        /// <inheritdoc/>
        public IEnumerator<ThingGroupSelector> GetEnumerator()
        {
            return _thingGroupSelectors.GetEnumerator();
        }

        /// <inheritdoc/>
        public bool Remove(ThingGroupSelector item)
        {
            _removeThingGroupSelectorCallbacks.ForEach(c => c.Invoke(item));
            return _thingGroupSelectors.Remove(item);
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <summary>
        /// Add <paramref name="groupSelector"/> to blacklist.
        /// </summary>
        /// <param name="groupSelector"> <see cref="ThingGroupSelector"/> to add.</param>
        public void AddToBlacklist(ThingGroupSelector groupSelector)
        {
            _blacklistSelectors.Add(groupSelector);
        }

        /// <summary>
        /// Remove <paramref name="groupSelector"/> from blacklist.
        /// </summary>
        /// <param name="groupSelector"> <see cref="ThingGroupSelector"/> to remove. </param>
        public void RemoveFromBlacklist(ThingGroupSelector groupSelector)
        {
            _blacklistSelectors.Remove(groupSelector);
        }

        /// <inheritdoc/>
        public int IndexOf(ThingGroupSelector item)
        {
            return _thingGroupSelectors.IndexOf(item);
        }

        /// <inheritdoc/>
        public void Insert(int index, ThingGroupSelector item)
        {
            _thingGroupSelectors.Insert(index, item);
            _addNewThingGroupSelectorCallbacks.ForEach(c => c.Invoke(item));
        }

        /// <inheritdoc/>
        public void RemoveAt(int index)
        {
            _removeThingGroupSelectorCallbacks.ForEach(c => c.Invoke(_thingGroupSelectors[index]));
            _thingGroupSelectors.RemoveAt(index);
        }

        /// <summary>
        /// Add callback to stack-count-changed event.
        /// </summary>
        /// <param name="callback"> Callback to add. </param>
        public void AddStackCountChangedCallback(Action<ThingGroupSelector, int> callback) => _thingGroupSelectorStackCountChangedCallbacks.Add(callback);

        /// <summary>
        /// Add callback to event in which a new <see cref="ThingGroupSelector"/> is added to this loadout.
        /// </summary>
        /// <param name="callback"> Callback to add. </param>
        public void AddAddNewThingGroupSelectorCallback(Action<ThingGroupSelector> callback) => _addNewThingGroupSelectorCallbacks.Add(callback);

        /// <summary>
        /// Add callback to event in which a <see cref="ThingGroupSelector"/> is removed from this loadout.
        /// </summary>
        /// <param name="callback"> Callback to add. </param>
        public void AddRemoveThingGroupSelectorCallback(Action<ThingGroupSelector> callback) => _removeThingGroupSelectorCallbacks.Add(callback);

        /// <summary>
        /// Remove a callback from an event in which stack count of a <see cref="ThingGroupSelector"/> is changed.
        /// </summary>
        /// <param name="callback"> Callback to add. </param>
        public void RemoveStackCountChangedCallback(Action<ThingGroupSelector, int> callback) => _thingGroupSelectorStackCountChangedCallbacks.Remove(callback);

        /// <summary>
        /// Remove a callback from an event in which a new <see cref="ThingGroupSelector"/> is added to this loadout.
        /// </summary>
        /// <param name="callback"> Callback to add. </param>
        public void RemoveAddNewThingGroupSelectorCallback(Action<ThingGroupSelector> callback) => _addNewThingGroupSelectorCallbacks.Remove(callback);

        /// <summary>
        /// Remove a callback from an event in which a <see cref="ThingGroupSelector"/> is removed from this loadout.
        /// </summary>
        /// <param name="callback"> Callback to add. </param>
        public void RemoveRemoveThingGroupSelectorCallback(Action<ThingGroupSelector> callback) => _removeThingGroupSelectorCallbacks.Remove(callback);

        /// <summary>
        /// Add callback to <paramref name="groupSelector"/>.
        /// </summary>
        /// <param name="groupSelector"> The object that will invoke the callback. </param>
        public void AddStackCountChangedCallbackTo(ThingGroupSelector groupSelector)
        {
            ValidateArg.NotNull(groupSelector, nameof(groupSelector));

            groupSelector.AddStackCountChangedCallback(this.StackCountChangedCallback);
        }

        /// <summary>
        /// Add callback to <paramref name="groupSelector"/>.
        /// </summary>
        /// <param name="groupSelector"> The object that will invoke the callback. </param>
        public void AddAddNewThingSelectorCallbackTo(ThingGroupSelector groupSelector)
        {
            ValidateArg.NotNull(groupSelector, nameof(groupSelector));

            groupSelector.AddAddNewThingSelectorCallback(this.AddNewThingSelectorCallback);
        }

        /// <summary>
        /// Add callback to <paramref name="groupSelector"/>.
        /// </summary>
        /// <param name="groupSelector"> The object that will invoke the callback. </param>
        public void AddRemoveThingSelectorCallbackTo(ThingGroupSelector groupSelector)
        {
            ValidateArg.NotNull(groupSelector, nameof(groupSelector));

            groupSelector.AddRemoveThingSelectorCallback(this.RemoveThingSelectorCallback);
        }

        /// <summary>
        /// Save state.
        /// </summary>
        public new void ExposeData()
        {
            List<ThingGroupSelector> groupSelectorsCopy = _thingGroupSelectors;
            Scribe_Collections.Look(ref groupSelectorsCopy, nameof(_thingGroupSelectors), LookMode.Deep);

            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                foreach (ThingGroupSelector groupSelector in groupSelectorsCopy)
                {
                    this.Add(groupSelector);
                }
            }
        }

        /// <summary>
        /// Update readout if the loadout has changed.
        /// </summary>
        protected virtual void UpdateReadout()
        {
            _weight = _thingGroupSelectors.Sum(s => s.Weight);
            _isDirty = false;
        }

        private static string GetDefaultLoadoutName(Pawn pawn)
        {
            ValidateArg.NotNull(pawn, nameof(pawn));

            return UIText.LoadoutName.Translate(pawn.Name.ToStringFull);
        }

        /// <summary>
        /// Create <see cref="ThingGroupSelector"/> for each items in <paramref name="things"/> and add it to this loadout.
        /// </summary>
        /// <param name="things"> Things to add to loadout. </param>
        private void AddItems(IEnumerable<Thing> things)
        {
            if (things == null)
                return;

            foreach (Thing thing in things)
            {
                ThingGroupSelector groupSelector = new ThingGroupSelector(thing.def);
                groupSelector.SetStackCount(thing.stackCount);
                groupSelector.Add(new SingleThingSelector(thing));
                this.Add(groupSelector);
            }
        }

        private void StackCountChangedCallback(ThingGroupSelector thingGroupSelector, int oldStackCount)
        {
            if (thingGroupSelector.AllowedStackCount == 0)
            {
                this.Remove(thingGroupSelector);
                _removeThingGroupSelectorCallbacks.ForEach(c => c.Invoke(thingGroupSelector));
            }
            else
            {
                _thingGroupSelectorStackCountChangedCallbacks.ForEach(c => c.Invoke(thingGroupSelector, oldStackCount));
            }

            _isDirty = true;
        }

        private void AddNewThingSelectorCallback(ThingSelector thingSelector)
        {
            _isDirty = true;
        }

        private void RemoveThingSelectorCallback(ThingSelector thingSelector)
        {
            _isDirty = true;
        }
    }
}
