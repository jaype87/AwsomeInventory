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
using RimWorld;
using UnityEngine;
using Verse;

namespace AwesomeInventory.Loadout
{
    /// <summary>
    /// It inherits from the "Outfit" class, is added to outfitDatabase and holds information about loadout.
    /// </summary>
    /// <remarks>It uses LoadoutCaomparer to generate hash value for keys. </remarks>
    public class AwesomeInventoryLoadout : Outfit, ICollection<ThingSelector>
    {
        /// <summary>
        /// A collection of <see cref="ThingSelector"/>s that dictates what items to look for.
        /// </summary>
        protected SortedList<uint, List<ThingSelector>> _selectorViews
            = new SortedList<uint, List<ThingSelector>>();

        private uint _groupID = 0;
        private List<ThingSelector> _selectors;

        /// <summary>
        /// Initializes a new instance of the <see cref="AwesomeInventoryLoadout"/> class.
        /// </summary>
        public AwesomeInventoryLoadout()
        {
        }

        /// <summary>
        /// This event is raised whenever a <see cref="ThingSelector"/> in this loadout has its settings changed.
        /// </summary>
        public event Action<ThingSelector> SelectorChangedEvent;

        /// <summary>
        /// Raise <see cref="AwesomeInventoryLoadout.SelectorChangedEvent"/> whenever settings for <paramref name="thingSelector"/> is changed.
        /// </summary>
        /// <param name="thingSelector"> <see cref="ThingSelector"/> whose settings have been changed. </param>
        public void SettingsChangedCallbackHandler(ThingSelector thingSelector)
        {
            ValidateArg.NotNull(thingSelector, nameof(thingSelector));

            if (thingSelector.AllowedStackCount == 0)
                this.Remove(thingSelector);

            SelectorChangedEvent?.Invoke(thingSelector);
        }

        #region ICollection implementation

        /// <inheritdoc/>
        [SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1201:Elements should appear in the correct order", Justification = "Interface implementation")]
        public int Count => _selectorViews.Sum(p => p.Value.Count);

        /// <inheritdoc/>
        public bool IsReadOnly => false;

        /// <summary>
        /// Add new item to loadout.
        /// </summary>
        /// <param name="item"> Item to add. </param>
        public void Add(ThingSelector item)
        {
            ValidateArg.NotNull(item, nameof(item));

            this.Add(_groupID++, item);
        }

        /// <inheritdoc/>
        public void Clear()
        {
            _selectorViews.Clear();
        }

        /// <inheritdoc/>
        public bool Contains(ThingSelector item)
        {
            return _selectorViews.Any(pair => pair.Value.Any(s => s.ID == item.ID));
        }

        /// <inheritdoc/>
        public void CopyTo(ThingSelector[] array, int arrayIndex)
        {
            ValidateArg.NotNull(array, nameof(array));

            foreach (ThingSelector thingSelector in this)
            {
                array[arrayIndex++] = thingSelector;
            }
        }

        /// <inheritdoc/>
        public IEnumerator<ThingSelector> GetEnumerator()
        {
            return _selectorViews.SelectMany(g => g.Value).GetEnumerator();
        }

        /// <inheritdoc/>
        public bool Remove(ThingSelector item)
        {
            if (item == null)
                return false;

            foreach (var pair in _selectorViews)
            {
                if (pair.Value.Remove(item))
                    return true;
            }

            return false;
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion

        /// <summary>
        /// Add <paramref name="selector"/> to selectorViews. with <paramref name="groupID"/>.
        /// </summary>
        /// <param name="groupID"> The ID of group to which <paramref name="selector"/> belongs. </param>
        /// <param name="selector"> Selector to add. </param>
        public void Add(uint groupID, ThingSelector selector)
        {
            if (_selectorViews.TryGetValue(groupID, out List<ThingSelector> selectors))
                selectors.Add(selector);
            else
                _selectorViews[groupID++] = new List<ThingSelector>() { selector };
        }

        /// <summary>
        /// Save state.
        /// </summary>
        public new void ExposeData()
        {
            List<uint> groupIDs = _selectorViews.Keys.ToList();
            List<List<ThingSelector>> selectorGroups = _selectorViews.Values.ToList();

            Scribe_Collections.Look(ref groupIDs, nameof(groupIDs), LookMode.Value);
            for (int i = 0; i < groupIDs.Count; i++)
            {
                if (Scribe.mode == LoadSaveMode.Saving)
                {
                    List<ThingSelector> worklist = selectorGroups[i];
                    Scribe_Collections.Look(ref worklist, nameof(selectorGroups) + i, LookMode.Deep);
                }
                else if (Scribe.mode == LoadSaveMode.LoadingVars)
                {
                    List<ThingSelector> worklist = new List<ThingSelector>();
                    Scribe_Collections.Look(ref worklist, nameof(selectorGroups) + i, LookMode.Deep);
                    _selectorViews.Add(groupIDs[i], worklist);
                }
            }
        }
    }
}
