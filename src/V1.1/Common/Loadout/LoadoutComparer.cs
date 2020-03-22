// <copyright file="LoadoutComparer.cs" company="Zizhen Li">
// Copyright (c) 2019 - 2020 Zizhen Li. All rights reserved.
// Licensed under the LGPL-3.0-only license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using RimWorld;
using Verse;

namespace AwesomeInventory.Loadout
{
    /// <summary>
    /// Compare equality of two things of T.
    /// </summary>
    /// <typeparam name="T"> Type that is subclass of <see cref="Thing"/>. </typeparam>
    public class LoadoutComparer<T> : EqualityComparer<T>
        where T : Thing
    {
        /// <inheritdoc/>
        public override bool Equals(T x, T y)
        {
            if (ReferenceEquals(x, y))
                return true;

            if (x == null || y == null)
                return false;

            ThingStuffPairWithQuality pairX = x.MakeThingStuffPairWithQuality();
            ThingStuffPairWithQuality pairY = y.MakeThingStuffPairWithQuality();
            return pairX == pairY;
        }

        /// <inheritdoc/>
        public override int GetHashCode(T obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(string.Empty);
            }

            return obj.MakeThingStuffPairWithQuality().GetHashCode();
        }
    }
}
