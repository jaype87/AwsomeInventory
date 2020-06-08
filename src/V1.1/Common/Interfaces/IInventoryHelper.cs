// <copyright file="IInventoryHelper.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace AwesomeInventory
{
    /// <summary>
    /// Helper class for inventory management.
    /// </summary>
    public interface IInventoryHelper
    {
        /// <summary>
        /// Check if <paramref name="pawn"/> will be over encumbered after picking up <paramref name="count"/> of <paramref name="thing"/>.
        /// </summary>
        /// <param name="pawn"> Pawn to check. </param>
        /// <param name="thing"> Thing to pick up. </param>
        /// <param name="count"> Number of <paramref name="thing"/> to pick up. </param>
        /// <returns> Returns true if <paramref name="pawn"/> will be over encumbered. </returns>
        bool WillBeOverEncumberedAfterPickingUp(Pawn pawn, Thing thing, int count);
    }
}
