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
        protected List<ThingGroupSelector> _thingGroupSelectors = new List<ThingGroupSelector>();

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
        /// <param name="shallow"> Make a shallow copy for costume. </param>
        public AwesomeInventoryLoadout(AwesomeInventoryLoadout other, bool shallow = false)
        {
            ValidateArg.NotNull(other, nameof(other));

            this.uniqueId = Current.Game.outfitDatabase.AllOutfits.Max(o => o.uniqueId) + 1;
            this.label = LoadoutManager.GetIncrementalLabel(other.label);

            if (shallow)
            {
                _thingGroupSelectors = other._thingGroupSelectors;
                _blacklistSelectors = other._blacklistSelectors;
                this.filter = other.filter;
                _isDirty = true;
                foreach (ThingGroupSelector selector in _thingGroupSelectors)
                {
                    this.AddAddNewThingSelectorCallbackTo(selector);
                    this.AddRemoveThingSelectorCallbackTo(selector);
                    this.AddStackCountChangedCallbackTo(selector);
                }
            }
            else
            {
                foreach (ThingGroupSelector selector in other._thingGroupSelectors)
                {
                    ThingGroupSelector newSelector = new ThingGroupSelector(selector);
                    this.Add(newSelector);
                }

                foreach (ThingGroupSelector selector in other._blacklistSelectors)
                {
                    ThingGroupSelector newSelector = new ThingGroupSelector(selector);
                    this.AddToBlacklist(newSelector);
                }

                this.filter.CopyAllowancesFrom(other.filter);

                this.CopyCostumeFrom(other);
            }
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

            if (pawn.outfits?.CurrentOutfit?.filter is ThingFilter filter)
                this.filter.CopyAllowancesFrom(filter);
            else
                this.filter.SetAllow(ThingCategoryDefOf.Apparel, true);

            this.uniqueId = Current.Game.outfitDatabase.AllOutfits.Max(o => o.uniqueId) + 1;
            CompAwesomeInventoryLoadout compLoadout = pawn.TryGetComp<CompAwesomeInventoryLoadout>();
            this.label = compLoadout?.Loadout == null
                ? AwesomeInventoryLoadout.GetDefaultLoadoutName(pawn)
                : LoadoutManager.GetIncrementalLabel(compLoadout.Loadout.label);

            pawn.SetLoadout(this);
        }

        /// <summary>
        /// Gets a list of ThingGroupSelector used by this loadout.
        /// </summary>
        /// <remarks> Should only be used for children costumes. </remarks>
        public List<ThingGroupSelector> ThingGroupSelectors => _thingGroupSelectors;

        /// <summary>
        /// Gets costumes.
        /// </summary>
        public List<AwesomeInventoryCostume> Costumes { get; private set; } = new List<AwesomeInventoryCostume>();

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
        /// Make an empty loadout.
        /// </summary>
        /// <param name="pawn"> Make empty loadout for this pawn. </param>
        /// <returns> Returns an empty loadout. </returns>
        public static AwesomeInventoryLoadout MakeEmptyLoadout(Pawn pawn)
        {
            ValidateArg.NotNull(pawn, nameof(pawn));

            AwesomeInventoryLoadout loadout = new AwesomeInventoryLoadout();
            loadout.filter.SetAllow(ThingCategoryDefOf.Apparel, true);
            loadout.uniqueId = Current.Game.outfitDatabase.AllOutfits.Max(o => o.uniqueId) + 1;
            loadout.label = pawn.NameShortColored + "'s " + UIText.EmptyLoadout.TranslateSimple();

            return loadout;
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
            this.Add(item, false);
        }

        /// <summary>
        /// Add new item to loadout.
        /// </summary>
        /// <param name="item"> Item to add. </param>
        /// <param name="fromSibling"> Whether <paramref name="item"/> is added by a sibling loadout. </param>
        public void Add(ThingGroupSelector item, bool fromSibling)
        {
            ValidateArg.NotNull(item, nameof(item));

            _isDirty = true;

            this.AddAddNewThingSelectorCallbackTo(item);
            this.AddRemoveThingSelectorCallbackTo(item);
            this.AddStackCountChangedCallbackTo(item);

            if (!fromSibling)
            {
                _thingGroupSelectors.Add(item);
                if (!(item.AllowedThing is AIGenericDef))
                {
                    this.filter.SetAllow(item.AllowedThing, true);
                }

                this.NotifySiblingsSelectorAdded(item);
            }

            _addNewThingGroupSelectorCallbacks.ForEach(c => c.Invoke(item));
        }

        /// <inheritdoc/>
        public void Clear()
        {
            this.Clear(false);
        }

        /// <summary>
        /// Clear all selectors in loadout.
        /// </summary>
        /// <param name="fromSibling"> Whether it is called from a sibling loadout. </param>
        public void Clear(bool fromSibling)
        {
            _isDirty = true;
            _thingGroupSelectors.ForEach(
                group =>
                {
                    _removeThingGroupSelectorCallbacks.ForEach(c => c.Invoke(group));
                    if (!fromSibling)
                        this.NotifySiblingsSelectorRemoved(group);
                });

            if (!fromSibling)
            {
                _thingGroupSelectors.Clear();
            }
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
            return this.Remove(item, false);
        }

        /// <summary>
        /// Remove <paramref name="item"/> from loadout.
        /// </summary>
        /// <param name="item"> Selector to remove. </param>
        /// <param name="fromSibling"> Whether it is called from a sibling loadout. </param>
        /// <returns> Whether the removal of <paramref name="item"/> is successful. </returns>
        public virtual bool Remove(ThingGroupSelector item, bool fromSibling)
        {
            ValidateArg.NotNull(item, nameof(item));

            _isDirty = true;
            _removeThingGroupSelectorCallbacks.ForEach(c => c.Invoke(item));
            item.RemoveAddNewThingSelectorCallback(this.AddNewThingSelectorCallback);
            item.RemoveStackCountChangedCallback(this.StackCountChangedCallback);
            item.RemoveRemoveThingSelectorCallback(this.RemoveThingSelectorCallback);

            if (!fromSibling)
            {
                this.NotifySiblingsSelectorRemoved(item);
                return _thingGroupSelectors.Remove(item);
            }

            return true;
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
            this.Insert(index, item, false);
        }

        /// <summary>
        /// Insert <paramref name="item"/> to index position <paramref name="index"/>.
        /// </summary>
        /// <param name="index"> Index position to which <paramref name="item"/> is inserted. </param>
        /// <param name="item"> Selector to add. </param>
        /// <param name="fromSibling"> Whether it is called from a sibling loadout. </param>
        public void Insert(int index, ThingGroupSelector item, bool fromSibling)
        {
            ValidateArg.NotNull(item, nameof(item));

            _isDirty = true;
            _addNewThingGroupSelectorCallbacks.ForEach(c => c.Invoke(item));

            this.AddAddNewThingSelectorCallbackTo(item);
            this.AddRemoveThingSelectorCallbackTo(item);
            this.AddStackCountChangedCallbackTo(item);

            if (!fromSibling)
            {
                _thingGroupSelectors.Insert(index, item);
                if (!(item.AllowedThing is AIGenericDef))
                {
                    this.filter.SetAllow(item.AllowedThing, true);
                }

                this.NotifySiblingsSelectorAdded(item);
            }

            _addNewThingGroupSelectorCallbacks.ForEach(c => c.Invoke(item));
        }

        /// <inheritdoc/>
        public void RemoveAt(int index)
        {
            this.RemoveAt(index, false);
        }

        /// <summary>
        /// Remove <see cref="ThingGroupSelector"/> at <paramref name="index"/>.
        /// </summary>
        /// <param name="index"> Index postion of the selector to remove. </param>
        /// <param name="fromSibling"> Whether it is called from a sibling loadout. </param>
        public void RemoveAt(int index, bool fromSibling)
        {
            _isDirty = true;
            ThingGroupSelector toRemove = _thingGroupSelectors[index];
            _removeThingGroupSelectorCallbacks.ForEach(c => c.Invoke(toRemove));

            if (!fromSibling)
            {
                _thingGroupSelectors.RemoveAt(index);
            }
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
        /// Copy costume from <paramref name="loadout"/>.
        /// </summary>
        /// <param name="loadout"> Loadout from which costumes are copied. </param>
        public void CopyCostumeFrom(AwesomeInventoryLoadout loadout)
        {
            ValidateArg.NotNull(loadout, nameof(loadout));

            foreach (AwesomeInventoryCostume costume in loadout.Costumes)
            {
                AwesomeInventoryCostume cos = new AwesomeInventoryCostume(this);
                cos.label = $"{this.label} {costume.label}";
                this.Costumes.Add(cos);
                LoadoutManager.AddLoadout(cos);

                foreach (ThingGroupSelector selector in costume.CostumeItems)
                {
                    ThingGroupSelector copy = this.FirstOrDefault(s => s.AllowedThing == selector.AllowedThing);
                    if (copy != null)
                        cos.AddItemToCostume(copy);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual ThingFilter GetGlobalThingFilter()
        {
            if (this is AwesomeInventoryCostume costume)
                return costume.Base.filter;

            return this.filter;
        }

        /// <summary>
        /// Save state.
        /// </summary>
#pragma warning disable CS0108 // Member hides inherited member; missing new keyword
        public virtual void ExposeData()
        {
            List<AwesomeInventoryCostume> costumes = this.Costumes;
            List<ThingGroupSelector> groupSelectorsCopy = _thingGroupSelectors;

            Scribe_Collections.Look(ref groupSelectorsCopy, nameof(_thingGroupSelectors), LookMode.Deep);
            Scribe_Collections.Look(ref costumes, nameof(this.Costumes), LookMode.Reference);

            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                foreach (ThingGroupSelector groupSelector in groupSelectorsCopy)
                {
                    this.Add(groupSelector);
                }
            }

            if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs)
            {
                this.Costumes = costumes;
            }
        }
#pragma warning restore CS0108 // Member hides inherited member; missing new keyword

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
                Thing innerThing = thing.GetInnerIfMinified();
                ThingGroupSelector groupSelector = new ThingGroupSelector(innerThing.def);
                groupSelector.SetStackCount(innerThing.stackCount);
                groupSelector.Add(AwesomeInventoryServiceProvider.MakeInstanceOf<SingleThingSelector>(innerThing));
                this.Add(groupSelector);
            }
        }

        private void StackCountChangedCallback(ThingGroupSelector thingGroupSelector, int oldStackCount)
        {
            if (thingGroupSelector.AllowedStackCount == 0)
            {
                this.Remove(thingGroupSelector);
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

        private void NotifySiblingsSelectorAdded(ThingGroupSelector selector)
        {
            if (this is AwesomeInventoryCostume costume)
            {
                costume.Base.Costumes.Except(this).ToList().ForEach(c => c.Add(selector, true));
                costume.Base.Add(selector, true);
            }
            else
            {
                this.Costumes.ForEach(c => c.Add(selector, true));
            }
        }

        private void NotifySiblingsSelectorRemoved(ThingGroupSelector selector)
        {
            if (this is AwesomeInventoryCostume costume)
                costume.Base.Costumes.ForEach(c => c.Remove(selector, true));
            else
                this.Costumes.ForEach(c => c.Remove(selector, true));
        }
    }
}
