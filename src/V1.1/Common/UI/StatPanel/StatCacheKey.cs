// <copyright file="StatCacheKey.cs" company="Zizhen Li">
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

namespace AwesomeInventory.UI
{
    /// <summary>
    /// Key for querying stat cache table.
    /// </summary>
    public struct StatCacheKey : IEquatable<StatCacheKey>
    {
        /// <summary>
        /// Selected pawn.
        /// </summary>
        public readonly Pawn Pawn;

        /// <summary>
        /// StatDef for cache.
        /// </summary>
        public readonly StatDef StatDef;

        /// <summary>
        /// Initializes a new instance of the <see cref="StatCacheKey"/> struct.
        /// </summary>
        /// <param name="pawn"> Selected pawn. </param>
        /// <param name="statDef"> StatDef for cache. </param>
        public StatCacheKey(Pawn pawn, StatDef statDef)
        {
            Pawn = pawn;
            StatDef = statDef;
        }

        /// <summary>
        /// Compares <paramref name="left"/> to <paramref name="right"/> to check if they are equal.
        /// </summary>
        /// <param name="left"> left key. </param>
        /// <param name="right"> right key. </param>
        /// <returns> Returns true if <paramref name="left"/> equals to <paramref name="right"/>. </returns>
        public static bool operator ==(StatCacheKey left, StatCacheKey right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Compares <paramref name="left"/> to <paramref name="right"/> to check whether they are not equal.
        /// </summary>
        /// <param name="left"> left key. </param>
        /// <param name="right"> right key. </param>
        /// <returns> Returns true if <paramref name="left"/> is not equal to <paramref name="right"/>. </returns>
        public static bool operator !=(StatCacheKey left, StatCacheKey right)
        {
            return !(left == right);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return ((Pawn != null ? Pawn.GetHashCode() : 0) * 397) ^ (StatDef != null ? StatDef.GetHashCode() : 0);
            }
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is StatCacheKey other && Equals(other);
        }

        /// <inheritdoc />
        public bool Equals(StatCacheKey other)
        {
            return Equals(Pawn, other.Pawn) && Equals(StatDef, other.StatDef);
        }
    }
}
