// <copyright file="InventoryUtility.cs" company="Zizhen Li">
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
            ValidateArg.NotNull(pawn, nameof(pawn));

            List<Thing> things = new List<Thing>();
            things.AddRange(pawn.equipment.AllEquipmentListForReading.Cast<Thing>());
            things.AddRange(pawn.apparel.WornApparel.Cast<Thing>());
            things.AddRange(pawn.inventory.innerContainer);

            return things;
        }

        /// <summary>
        /// Returns a list of things that merges identical things in <paramref name="things"/>.
        /// </summary>
        /// <typeparam name="TSort"> Key for sorting. </typeparam>
        /// <param name="things"> Source of things. </param>
        /// <param name="comparer"> A comparer used for finding identical things. </param>
        /// <param name="sorter"> Used for sorting the result. </param>
        /// <returns> A list with identical things merged. </returns>
        public static List<Thing> GetMergedList<TSort>(this IEnumerable<Thing> things, IEqualityComparer<Thing> comparer, Func<Thing, TSort> sorter)
        {
            List<Thing> result = things.GroupBy(
                                    (thing) => thing.GetInnerIfMinified()
                                    , comparer)
                                .Select(
                                    (group) =>
                                    {
                                        Thing thing = group.Key.MakeThingStuffPairWithQuality().MakeThingWithoutID();
                                        thing.stackCount = group.Sum(t => t.stackCount);
                                        return thing;
                                    })
                                .ToList();

            if (sorter != null)
                result = result.OrderBy(sorter).ToList();

            return result;
        }
    }
}
