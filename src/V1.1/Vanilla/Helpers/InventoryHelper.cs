// <copyright file="InventoryHelper.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the GPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace AwesomeInventory.Utilities
{
    /// <summary>
    /// Helper class for inventory management.
    /// </summary>
    public class InventoryHelper : IInventoryHelper
    {
        /// <inheritdoc/>
        public bool WillBeOverEncumberedAfterPickingUp(Pawn pawn, Thing thing, int count)
        {
            return MassUtility.WillBeOverEncumberedAfterPickingUp(pawn, thing, count);
        }
    }
}
