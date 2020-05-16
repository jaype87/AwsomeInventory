// <copyright file="ThingComparer.cs" company="Zizhen Li">
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

namespace AwesomeInventory
{
    /// <summary>
    /// Compare things with their def, stuff def and quality.
    /// </summary>
    public class ThingComparer : IEqualityComparer<Thing>
    {
        /// <summary>
        /// Gets an intance of this comparer.
        /// </summary>
        public static ThingComparer Instance { get; } = new ThingComparer();

        /// <inheritdoc/>
        public bool Equals(Thing x, Thing y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }
            else if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
            {
                return false;
            }
            else if (x.def == y.def && x.Stuff == y.Stuff)
            {
                bool flag = true;
                flag ^= x.TryGetQuality(out QualityCategory qualityX);
                flag ^= y.TryGetQuality(out QualityCategory qualityY);

                if (flag)
                    return qualityX == qualityY;

                return false;
            }
            else
            {
                return false;
            }
        }

        /// <inheritdoc/>
        public int GetHashCode(Thing obj)
        {
            ValidateArg.NotNull(obj, nameof(obj));

            return Gen.HashCombine(
                Gen.HashCombine(
                    obj.def.shortHash, obj.Stuff), obj.TryGetQuality(out QualityCategory qc) ? qc.GetHashCode() : 0);
        }
    }
}
