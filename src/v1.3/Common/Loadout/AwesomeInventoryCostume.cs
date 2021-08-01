// <copyright file="AwesomeInventoryCostume.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AwesomeInventory.UI;
using Verse;

namespace AwesomeInventory.Loadout
{
    /// <summary>
    /// A predefined set of apparels and equipment for pawn to wear.
    /// </summary>
    public class AwesomeInventoryCostume : AwesomeInventoryLoadout
    {
        /// <summary>
        /// Items in a costume.
        /// </summary>
        private HashSet<ThingGroupSelector> _costumeItems = new HashSet<ThingGroupSelector>();

        /// <summary>
        /// Initializes a new instance of the <see cref="AwesomeInventoryCostume"/> class.
        /// </summary>
        public AwesomeInventoryCostume()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AwesomeInventoryCostume"/> class.
        /// </summary>
        /// <param name="other"> Costume to copy from. </param>
        public AwesomeInventoryCostume(AwesomeInventoryCostume other)
            : base(other?.Base, true)
        {
            foreach (ThingGroupSelector selector in other._costumeItems)
            {
                _costumeItems.Add(selector);
            }

            this.label = string.Concat(other.label, UIText.CopySuffix.TranslateSimple());
            this.Base = other.Base;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AwesomeInventoryCostume"/> class.
        /// </summary>
        /// <param name="loadout"> The loadout this costume creates from. </param>
        public AwesomeInventoryCostume(AwesomeInventoryLoadout loadout)
            : base(loadout, true)
        {
            ValidateArg.NotNull(loadout, nameof(loadout));

            this.label = string.Concat(loadout.label, UIText.Costume.TranslateSimple());
            this.Base = loadout;
        }

        /// <summary>
        /// Gets the base for this costume.
        /// </summary>
        public AwesomeInventoryLoadout Base { get; private set; }

        /// <summary>
        /// Gets a costum.
        /// </summary>
        public List<ThingGroupSelector> CostumeItems => _costumeItems.ToList();

        /// <summary>
        /// Add item to costume.
        /// </summary>
        /// <param name="selector"> Item to add. </param>
        public void AddItemToCostume(ThingGroupSelector selector)
        {
            ValidateArg.NotNull(selector, nameof(selector));

            _costumeItems.Add(selector);
        }

        /// <summary>
        /// Remove item from costume.
        /// </summary>
        /// <param name="selector"> Item to remove. </param>
        public void RemoveItemFromCostume(ThingGroupSelector selector)
        {
            _costumeItems.Remove(selector);
        }

        /// <inheritdoc/>
        public override bool Remove(ThingGroupSelector item, bool fromSibling)
        {
            this.RemoveItemFromCostume(item);
            return base.Remove(item, fromSibling);
        }

        /// <summary>
        /// Check if this costume is derived from <paramref name="loadout"/>.
        /// </summary>
        /// <param name="loadout"> Loadout used for query. </param>
        /// <returns> Returns true if this costume is derived from <paramref name="loadout"/>. </returns>
        public virtual bool CostumeOf(AwesomeInventoryLoadout loadout)
        {
            ValidateArg.NotNull(loadout, nameof(loadout));

            return loadout.Costumes.Contains(this);
        }

        /// <summary>
        /// Check if <paramref name="costume"/> is a sibling of this costume.
        /// </summary>
        /// <param name="costume"> Costume to check. </param>
        /// <returns> If true, these two costumes share the same loadout parent. </returns>
        public virtual bool SibilingOf(AwesomeInventoryCostume costume)
        {
            return this.Base.Costumes.Contains(costume);
        }

        /// <summary>
        /// Check if <paramref name="loadout"/> is in the same family tree of this costume.
        /// </summary>
        /// <param name="loadout"> loadout to check. </param>
        /// <returns> Returns true if <paramref name="loadout"/> is the parent or sibling of this costume. </returns>
        public virtual bool InSameLoadoutTree(AwesomeInventoryLoadout loadout)
        {
            return this.CostumeOf(loadout)
                || (loadout is AwesomeInventoryCostume costume
                    &&
                    this.SibilingOf(costume));
        }

        /// <summary>
        /// Save states of this costume to xml.
        /// </summary>
        public override void ExposeData()
        {
            AwesomeInventoryLoadout b = this.Base;
            Scribe_Collections.Look(ref _costumeItems, nameof(_costumeItems), LookMode.Reference);
            Scribe_References.Look(ref b, nameof(this.Base));

            this.Base = b;
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                _thingGroupSelectors = this.Base.ThingGroupSelectors;
                _thingGroupSelectors.ForEach(g => this.Add(g, true));
            }
        }
    }
}
