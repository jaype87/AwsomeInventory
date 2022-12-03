// <copyright file="AwesomeInventoryStuffDefOf.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace AwesomeInventory.Loadout
{
    /// <summary>
    /// Stuff defined by Awesome Inventory.
    /// </summary>
    [DefOf]
    public static class AwesomeInventoryStuffDefOf
    {
        /// <summary>
        /// Generic resource is assigned to items in loadout window that have no other stuff source assign to.
        /// </summary>
        public static ThingDef AwesomeInventoryGenericResource;

        static AwesomeInventoryStuffDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(AwesomeInventoryStuffDefOf));
        }
    }
}
