// <copyright file="BulkUtility.cs" company="Zizhen Li">
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

namespace AwesomeInventory.Loadout
{
    /// <summary>
    /// Provide utility support for bulk stats on <see cref="Pawn"/> and <see cref="Thing"/>.
    /// </summary>
    public static class BulkUtility
    {
        /// <summary>
        /// Get bulk value for <see cref="Thing"/>.
        /// </summary>
        /// <param name="thing"> Thing to inspect. </param>
        /// <returns> Bulk value on <paramref name="thing"/>. </returns>
        public static float BulkFor(Thing thing)
        {
            return thing.GetStatValue(CE_StatDefOf.Bulk);
        }

        /// <summary>
        /// Get bulk value for <paramref name="apparel"/>.
        /// </summary>
        /// <param name="apparel"> Apparel to inspect. </param>
        /// <returns> WornBulk value on <paramref name="apparel"/>. </returns>
        public static float WornBulkFor(Apparel apparel)
        {
            return apparel.GetStatValue(CE_StatDefOf.WornBulk);
        }

        /// <summary>
        /// Get bulk value for <see cref="ThingDef"/>.
        /// </summary>
        /// <param name="thingDef"> ThingDef that requests bulk stat. </param>
        /// <param name="stuff"> Stuff that thing is made of. </param>
        /// <param name="wornBulk"> Whether the thing is to be worn. </param>
        /// <returns> Bulk value for request. </returns>
        public static float BulkFor(ThingDef thingDef, ThingDef stuff, bool wornBulk)
        {
            if (wornBulk)
            {
                return thingDef.GetStatValueAbstract(CE_StatDefOf.WornBulk, stuff);
            }
            else
            {
                return thingDef.GetStatValueAbstract(CE_StatDefOf.Bulk, stuff);
            }
        }
    }
}
