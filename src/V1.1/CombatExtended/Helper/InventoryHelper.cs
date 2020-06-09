// <copyright file="InventoryHelper.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CombatExtended;
using RimWorld;
using Verse;

namespace AwesomeInventory
{
    /// <summary>
    /// Helper class for inventory management.
    /// </summary>
    [RegisterService(typeof(IInventoryHelper), typeof(InventoryHelper))]
    public class InventoryHelper : IInventoryHelper
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InventoryHelper"/> class.
        /// </summary>
        [Obsolete(ErrorText.NoDirectCall, false)]
        public InventoryHelper()
        {
        }

        /// <inheritdoc/>
        public bool WillBeOverEncumberedAfterPickingUp(Pawn pawn, Thing thing, int count)
        {
            return MassUtility.WillBeOverEncumberedAfterPickingUp(pawn, thing, count);
        }
    }
}
