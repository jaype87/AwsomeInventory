// <copyright file="ThingSelectorComparer.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AwesomeInventory.Loadout
{
    /// <summary>
    /// Compare <see cref="ThingSelector"/> to choose the most stringent selector for newly added things to inventory.
    /// </summary>
    public class ThingSelectorComparer : Comparer<ThingSelector>
    {
        /// <summary>
        /// Gets a comparer instance for <see cref="ThingSelectorComparer"/>.
        /// </summary>
        public static readonly ThingSelectorComparer Instance = new ThingSelectorComparer();

        /// <inheritdoc/>
        /// <remarks> More stringent selector takes precedence, e.g. Compare will return -1 if x is more stringent. </remarks>
        public override int Compare(ThingSelector x, ThingSelector y)
        {
            if (ReferenceEquals(x, y))
                return 0;

            if (x is null)
                return 1;
            else if (y is null)
                return -1;

            SingleThingSelector xSelector = x as SingleThingSelector;
            SingleThingSelector ySelector = y as SingleThingSelector;

            return SingleThingSelector.Comparer.Instance.Compare(xSelector, ySelector);
        }
    }
}
