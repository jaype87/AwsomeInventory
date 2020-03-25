// <copyright file="AwesomeInventoryLoadoutTracker.cs" company="Zizhen Li">
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
    /// Tracks inventory level that is pertinent to <see cref="AwesomeInventoryLoadout"/>.
    /// </summary>
    public class AwesomeInventoryLoadoutTracker : AwesomeInventoryLoadout
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AwesomeInventoryLoadoutTracker"/> class.
        /// </summary>
        public AwesomeInventoryLoadoutTracker()
        {
        }

        public AwesomeInventoryLoadoutTracker(AwesomeInventoryLoadout loadout, List<Thing> currentInventory)
        {
            ValidateArg.NotNull(currentInventory, nameof(currentInventory));

            foreach (SingleThingSelector thingSelector in loadout)
            {
                SingleThingSelector invertedSelector = new SingleThingSelector()
                if (_selectorViews.TryGetValue(thingSelector.AllowedThing.defName, out List<SingleThingSelector> selectors))
                {
                }
            }
        }
    }
}
