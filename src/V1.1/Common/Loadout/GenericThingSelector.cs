// <copyright file="GenericThingSelector.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace AwesomeInventory.Loadout
{
    /// <summary>
    /// Selector for generic things, e.g., generic meals and generic medicines.
    /// </summary>
    public class GenericThingSelector : ThingSelector
    {
        private AIGenericDef _genericDef;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericThingSelector"/> class.
        /// </summary>
        /// <param name="genericDef"> The generic def used for this selector. </param>
        public GenericThingSelector(AIGenericDef genericDef)
        {
            _genericDef = genericDef;
        }

        /// <inheritdoc/>
        public override string LabelNoCount => _genericDef.label;

        /// <inheritdoc/>
        public override bool Allows(Thing thing, int inventoryLevel)
        {
            ValidateArg.NotNull(thing, nameof(thing));
            if (inventoryLevel < 0)
            {
                Log.Error(string.Format(CultureInfo.InvariantCulture, ErrorText.InventoryStackcountLessThanZero, thing.LabelNoCount));
                return false;
            }

            return _thingFilter.Allows(thing)
                && inventoryLevel + thing.stackCount <= _allowedStackCount;
        }
    }
}
