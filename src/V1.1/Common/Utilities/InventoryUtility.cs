// <copyright file="InventoryUtility.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace AwesomeInventory.Loadout
{
    /// <summary>
    /// Utility support for inventory.
    /// </summary>
    public static class InventoryUtility
    {
        /// <summary>
        /// Make a list of thing that consists of gears and inventory a pawn carries.
        /// </summary>
        /// <param name="pawn"> Pawn who carriees <see cref="Thing"/>s. </param>
        /// <returns> A list of <see cref="Thing"/> on <paramref name="pawn"/>. </returns>
        public static List<Thing> MakeListForPawnGearAndInventory(Pawn pawn)
        {
            List<Thing> things = new List<Thing>();
            things.AddRange(pawn.equipment.AllEquipmentListForReading.Cast<Thing>());
            things.AddRange(pawn.apparel.WornApparel.Cast<Thing>());
            things.AddRange(pawn.inventory.innerContainer);

            return things;
        }
    }
}
